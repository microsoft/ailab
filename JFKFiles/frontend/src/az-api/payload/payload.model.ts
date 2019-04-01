import { AzPayloadFacet } from "./facet.model";
import { AzFilterGroup } from "./filter.model";

/**
 * Object that represents API payload parameters. These params are used to fetch
 * certain data from the API and they will usually differ from query to query.
 * TODO: Some pending payload features: scoring, highlight, etc.
 */


export interface AzOrderBy {
  fieldName: string;
  order: "asc" | "desc";
}

export type AzSearchMode = "any" | "all";

export interface AzPayload {
  search: string;
  
  // Search payload
  count?: boolean;
  facets?: AzPayloadFacet[];
  filters?: AzFilterGroup;
  minimumCoverage?: number;
  orderBy?: AzOrderBy[];
  searchFields?: string[];
  searchMode?: AzSearchMode;
  select?: string[];
  skip?: number;
  top?: number;
  
  // Suggestions payload
  fuzzy?: boolean;
  suggesterName?: string;
  autocompleteMode?: string;
  scoringProfile?: string;
  highlight?: string;
}

export const defaultAzPayload: AzPayload = {
  search: "*",
  count: true,
  searchMode: "all",
  top: 10,
  scoringProfile: "demoBooster",
  highlight: "text",
}
