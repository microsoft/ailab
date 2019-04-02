import {
  AzResponseConfig,
  AzResponse,
  AzResponseFacet,
  AzResponseFacetValue,
} from "./response.model";
import { isArrayEmpty } from "../util";

/**
 * Parser for Response.
 * It will transform a raw JSON response from server to a Response object.
 */

const parseResponseFacetObject = (facetObj: any, config: AzResponseConfig): AzResponseFacet[] => {
  if (!facetObj) return null;

  return Object.keys(facetObj).filter(k => !k.includes(config.facetTypeSuffix)).map(k => ({
    fieldName: k,
    type: facetObj[k + config.facetTypeSuffix],
    values: facetObj[k].map(fv => ({
      value: fv[config.facetValueAccessor],
      count: fv[config.facetCountAccessor],
      from: fv[config.facetFromAccessor],
      to: fv[config.facetToAccessor],
    } as AzResponseFacetValue)),
  } as AzResponseFacet));
};

export const parseResponse = async (response: Response, config: AzResponseConfig
): Promise<AzResponse> => {
  const jsonObject = await response.json();

  if (!response.ok) {
    console.debug(jsonObject);
    throw new Error(`${response.status} - ${response.statusText}
      Message: ${jsonObject.error.message}`);
  }

  return {
    value: jsonObject[config.valueAccessor],
    count: jsonObject[config.countAccessor],
    coverage: jsonObject[config.coverageAccessor],
    facets: parseResponseFacetObject(jsonObject[config.facetsAccessor], config),
  };
};


