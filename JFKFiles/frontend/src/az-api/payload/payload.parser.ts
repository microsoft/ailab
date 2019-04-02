import { AzPayload, AzOrderBy } from "./payload.model";
import { parseFacetGET } from "./facet.parser";
import { parseFilterGroup } from "./filter.parser";
import { checkDuckType, isArrayEmpty } from "../../util";

/**
 * Parsers for Payload.
 * A parser will do a transformation from Payload object to GET or POST query params.
 * TODO: Implement POST parser.
 */

const parseOrderByGET = (ob: AzOrderBy): string => {
  return `"${ob.fieldName} ${ob.order}"`;
};

export const parsePayloadGET = (p: AzPayload): string => {
  return [
    p.search ? `search=${p.search}` : "",
    p.searchMode === "all" ? "searchMode=all" : "",
    isArrayEmpty(p.searchFields) ? "" : `searchFields=${p.searchFields.join(",")}`,
    isArrayEmpty(p.orderBy) ? "" : `$orderby=${p.orderBy.map(ob => parseOrderByGET(ob)).join(",")}`,
    isArrayEmpty(p.facets) ? "" : p.facets.map(f => parseFacetGET(f)).join("&"),
    isArrayEmpty(p.select) ? "" : `$select=${p.select.join(",")}`,
    p.filters ? `$filter=${parseFilterGroup(p.filters)}` : "",
    p.minimumCoverage ? `minimumCoverage=${p.minimumCoverage}` : "",
    p.count ? `$count=true` : "",
    p.top ? `$top=${p.top}` : "",
    p.skip ? `$skip=${p.skip}` : "",
    p.fuzzy ? `fuzzy=true` : "",
    p.suggesterName ? `suggesterName=${p.suggesterName}` : "",
    p.autocompleteMode ? `autocompleteMode=${p.autocompleteMode}` : "",
    p.scoringProfile ? `scoringProfile=${p.scoringProfile}` : "",
    p.highlight ? `highlight=${p.highlight}` : ""
  ]
    .filter(i => i)
    .join("&");
};
