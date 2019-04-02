/**
 * Object that represents the RESPONSE and RESPONSE configuration parameters. 
 * These config parameters will help in parsing the raw API response (JSON) to a RESPONSE object.
 * We should not be modifying this, server API should not publish break changes.
 * TODO: Only needed logic for JFK demo has been implemented so far. 
 */


export interface AzResponseFacetValue {
  value: string;
  count: number;
  from?: string;
  to?: string;
}

export interface AzResponseFacet {
  fieldName: string;
  type?: string;
  values: AzResponseFacetValue[];
}

export interface AzResponse {
  value: any[];
  count?: number;
  coverage?: number;
  facets?: AzResponseFacet[];
}

export interface AzResponseConfig {
  countAccessor: string;
  coverageAccessor: string;
  facetsAccessor: string;
  facetTypeSuffix: string;
  facetValueAccessor: string;
  facetCountAccessor: string;
  facetFromAccessor: string;
  facetToAccessor: string;
  valueAccessor: string;
  highlightAccessor: string;
  highlightTextAccessor: string;
}

export const defaultAzResponseConfig: AzResponseConfig = {
  countAccessor: "@odata.count",
  coverageAccessor: "@search.coverage",
  facetsAccessor: "@search.facets",
  facetTypeSuffix: "@odata.type",
  facetValueAccessor: "value",
  facetCountAccessor: "count",
  facetFromAccessor: "from",
  facetToAccessor: "to",
  valueAccessor: "value",
  highlightAccessor: "@search.highlights",
  highlightTextAccessor: "text",
}