/**
 * Types for Facets.
 */

export interface AzPayloadFacetConfigCountSort {
  count?: number;
  sort?: "count" | "-count" | "value" | "-value";
}

export interface AzPayloadFacetConfigValues {
  values: number[];
}

export interface AzPayloadFacetConfigInterval {
  interval: number;
}

export type AzPayloadFacetConfig =
  | AzPayloadFacetConfigCountSort
  | AzPayloadFacetConfigValues
  | AzPayloadFacetConfigInterval;

export interface AzPayloadFacet {
  fieldName: string;
  config?: AzPayloadFacetConfig;
}
