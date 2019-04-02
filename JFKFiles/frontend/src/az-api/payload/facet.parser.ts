import { isArrayEmpty, checkDuckType } from "../../util";
import {
  AzPayloadFacet,
  AzPayloadFacetConfigCountSort,
  AzPayloadFacetConfigValues,
  AzPayloadFacetConfigInterval,
  AzPayloadFacetConfig,
} from "./facet.model";

/**
 * Parsers for Facets.
 * A parser will do a transformaton from Facet object to GET or POST query params.
 * TODO: Advanced faceting with timeOffset.
 * TODO: Implement POST parser.
 */

const invalidFacet = (facet: AzPayloadFacet) => {
  return !facet.fieldName || facet.fieldName.length === 0;
};

const parseConfigCountSortGET = (config: AzPayloadFacetConfigCountSort): string => {
  return [config.count ? `count:${config.count}` : "", config.sort ? `sort:${config.sort}` : ""]
    .filter(i => i)
    .join(",");
};

const parseConfigValuesGET = (config: AzPayloadFacetConfigValues): string => {
  return isArrayEmpty(config.values) ? "" : `values:${config.values.join("|")}`;
};

const parseConfigIntervalGET = (config: AzPayloadFacetConfigInterval): string => {
  return config.interval ? `interval:${config.interval}` : "";
};

const parseConfigGET = (config: AzPayloadFacetConfig): string => {
  checkDuckType(config as AzPayloadFacetConfigCountSort, "count" );
  if (checkDuckType(config as AzPayloadFacetConfigCountSort, "count" ) || 
    checkDuckType(config as AzPayloadFacetConfigCountSort, "sort" )) {
    return parseConfigCountSortGET(config as AzPayloadFacetConfigCountSort);
  } else if (checkDuckType(config as AzPayloadFacetConfigValues, "values")) {
    return parseConfigValuesGET(config as AzPayloadFacetConfigValues);
  } else if (checkDuckType(config as AzPayloadFacetConfigInterval, "interval")) {
    return parseConfigIntervalGET(config as AzPayloadFacetConfigInterval);
  } else {
    return "";
  }
};

export const parseFacetGET = (facet: AzPayloadFacet): string => {
  if (invalidFacet(facet)) return "";

  let config = "";
  if (facet.config) {
    config = parseConfigGET(facet.config);
  }

  return `facet=${facet.fieldName}${config ? `,${config}` : ""}`;
};
