import { isArrayEmpty, isValueInArray, getUniqueStrings } from "../../../../util";
import { ServiceConfig } from "../../service";
import {
  AzResponse,
  AzResponseFacet,
  AzResponseFacetValue,
  AzPayload,
  AzPayloadFacet,
  AzFilter,
  AzFilterGroup,
  AzFilterCollection,
  AzFilterSingle,
  AzResponseConfig,
} from "../../../../az-api";
import {
  Item,
  ItemCollection,
  FacetCollection,
  FacetValue,
  Facet,
  State,
  FilterCollection,
  Filter,
} from "../../view-model";


// [Search] FROM AzApi response TO view model.

const mapImgUrlInMetadata = (metadata: string) => {
  const captures = /title=(?:'|")image\s?"(.+)"/g.exec(metadata);
  return captures && captures.length ? captures[1] : "";
};

const extractHighlightWords = (accumulator: string[], line: string): string[] => {
  const regexp = new RegExp('<em>(.+?)<\/em>','g');
  return regexp.test(line) ? 
    [
      ...accumulator,
      ...line.match(regexp)
    ] :
    accumulator;
};

const cleanHighlightWords = (words: string[]): string[] => {
  return words.map(word => (
    word.replace('<em>', '').replace('</em>', '').toLowerCase()
  ));
};

const mapHighlightWords = (rawHighlights: string[]): string[] => {
  const highlightWords = rawHighlights.reduce(extractHighlightWords, []);
  const cleanedHighlightWords = cleanHighlightWords(highlightWords);
  return getUniqueStrings(cleanedHighlightWords);
}

const checkHightlightWordsAvailable = (result: any, responseConfig: AzResponseConfig) => (
  Boolean(result) &&
  Boolean(responseConfig) &&
  Boolean(result[responseConfig.highlightAccessor]) &&
  Boolean(result[responseConfig.highlightAccessor][responseConfig.highlightTextAccessor])
)

const getHighlightWords = (result: any, responseConfig: AzResponseConfig): string[] => {
  if (checkHightlightWordsAvailable(result, responseConfig)) {
    const rawHightlights = result[responseConfig.highlightAccessor][responseConfig.highlightTextAccessor];
    return mapHighlightWords(rawHightlights);
  } else {
    return [];
  }
}

const mapResultToItem = (result: any, responseConfig: AzResponseConfig): Item => {
  return result ? {
    title: result.fileName,
    subtitle: "",
    thumbnail: mapImgUrlInMetadata(result.metadata),
    excerpt: "",
    rating: 0,
    extraFields: [result.entities],
    metadata: result.metadata,
    demoInitialPage: Boolean(result.demoInitialPage) ?
      result.demoInitialPage :
      undefined,
    type: result.type,
    highlightWords: getHighlightWords(result, responseConfig),
  } : null;
};

const mapSearchResponseForResults = (response: AzResponse, responseConfig: AzResponseConfig): ItemCollection => {
  return isArrayEmpty(response.value) ? null : response.value.map(r => mapResultToItem(r, responseConfig));
};

const mapResponseFacetValueToViewFacetValue = (responseFacetValue: AzResponseFacetValue): FacetValue => {
  // CUSTOM MAP to support 'Redactions' facet for Demo event.
  // TODO: This is not generic, remove it.
  let value = responseFacetValue.value;
  if (!value) {
    value = responseFacetValue.to ? "No" : "Yes";
  }

  return {
    value,
    count: responseFacetValue.count,
  }
};

const mapResponseFacetToViewFacet = (responseFacet: AzResponseFacet, baseFacet: Facet): Facet => {
  return responseFacet ? ({
    ...baseFacet,
    values: responseFacet.values.map(mapResponseFacetValueToViewFacetValue),
  }) : null;
};

const mapSearchResponseForFacets = (response: AzResponse, baseFacets: FacetCollection): FacetCollection => {
  return isArrayEmpty(response.facets) ? null :
    baseFacets.map(bf =>
      mapResponseFacetToViewFacet(response.facets.find(rf => rf.fieldName === bf.fieldId), bf)
    ).filter(f => f && !isArrayEmpty(f.values));
};

export const mapSearchResponseToState = (state: State, response: AzResponse, config: ServiceConfig): State => {
  const viewFacets = isArrayEmpty(state.facetCollection) ? config.initialState.facetCollection :
    state.facetCollection;
  return {
    ...state,
    resultCount: response.count,
    itemCollection: mapSearchResponseForResults(response, config.searchConfig.responseConfig),
    facetCollection: mapSearchResponseForFacets(response, viewFacets),
  }
};


// [Search] FROM view model TO AzApi.

const mapViewFilterToPayloadCustomFilter = (filter: Filter): AzFilterCollection => {
  // TODO: This is just tailor made for JFK Demo event.
  const yesTag = isValueInArray(filter.store, "Yes");
  return filter ? {
    fieldName: filter.fieldId,
    mode: yesTag ? "any" : "all",
    operator: yesTag ? "ge" : "lt",
    value: ["50"],
  } : null;
}

const mapViewFilterToPayloadCollectionFilter = (filter: Filter): AzFilterCollection => {
  return filter ? {
    fieldName: filter.fieldId,
    mode: "any",
    operator: "eq",
    value: filter.store,
  } : null;
}

const mapViewFilterToPayloadFilter = (filter: Filter): AzFilter => {
  // TODO: This is just tailor made for JFK Demo event.
  return filter.fieldId === "redactions" ?
    mapViewFilterToPayloadCustomFilter(filter) :
    mapViewFilterToPayloadCollectionFilter(filter);
}


const mapViewFiltersToPayloadFilters = (filters: FilterCollection): AzFilterGroup => {
  if (isArrayEmpty(filters)) return null;
  // TODO: Only collection filter implemented.
  const filterGroup: AzFilterGroup = {
    logic: "and",
    items: filters.map(f => mapViewFilterToPayloadFilter(f)).filter(f => f),
  };
  return filterGroup;
};

const mapViewFacetToPayloadFacet = (viewFacet: Facet): AzPayloadFacet => {
  return {
    fieldName: viewFacet.fieldId,
    config: viewFacet.config,
  };
};

export const mapStateToSearchPayload = (state: State, config: ServiceConfig): AzPayload => {
  const viewFacets = isArrayEmpty(state.facetCollection) ? config.initialState.facetCollection :
    state.facetCollection;
  return {
    ...config.searchConfig.defaultPayload,
    search: state.searchValue,
    top: state.pageSize,
    skip: state.pageIndex * state.pageSize,
    facets: viewFacets.map(f => mapViewFacetToPayloadFacet(f)),
    filters: mapViewFiltersToPayloadFilters(state.filterCollection),
  };
}
