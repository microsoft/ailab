export interface FacetValue {
  value: string;
  count: number;
}

export interface FacetConfigCountSort {
  count?: number;
  sort?: "count" | "-count" | "value" | "-value";
}

export interface FacetConfigValues {
  values: number[];
}

export interface FacetConfigInterval {
  interval: number;
}

export type FacetConfig =
  | FacetConfigCountSort
  | FacetConfigValues
  | FacetConfigInterval;

export interface Facet {
  fieldId: string;
  displayName: string;
  iconName?: string;
  selectionControl: string;
  values: FacetValue[];
  config: FacetConfig;
}

export type FacetCollection = Facet[];
