/**
 * Types for Filters.
 */


export interface AzFilterSingle {
  fieldName: string;
  operator: "eq" | "ne" | "gt" | "lt" | "ge" | "le";
  value: string[];
  logic?: "and" | "or"; // Only applicable to multiple values.
}

export interface AzFilterCollection {
  fieldName: string;
  mode: "any" | "all";
  operator: "eq" | "ne" | "gt" | "lt" | "ge" | "le";
  value: string[];
  logic?: "and" | "or";
}

export type AzFilter = AzFilterSingle | AzFilterCollection;

export type AzFilterGroupItem = (AzFilter | AzFilterGroup);

export interface AzFilterGroup {
  items: AzFilterGroupItem[];
  logic: "and" | "or";
}