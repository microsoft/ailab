import { GraphConfig } from "./config.model";
import { GraphPayload } from "./payload.model";
import { parsePayload } from "./payload.parser";
import { parseConfig } from "./config.parser";

/**
 * Given an API configuration and payload, it creates the Request object for the fetch method.
 */


export interface GraphRequest {
  url: string;
  options: RequestInit;
}

const buildURL = (config: GraphConfig, payload: GraphPayload): string => {
  return [
    parseConfig(config),
    parsePayload(payload),
  ].filter(i => i).join("");
};

const defaultOptions: RequestInit = {
  method: "GET",
  headers: {
    "Accept": "application/json",
  },
  mode: "cors"
};

export const CreateRequest = (config: GraphConfig, payload: GraphPayload): GraphRequest => {
  return {
    url: buildURL(config, payload),
    options: {
      ...defaultOptions,
      method: config.method,
    }
  };
}
