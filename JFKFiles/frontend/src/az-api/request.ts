import { AzConfig } from "./config.model";
import { AzPayload, parsePayloadGET } from "./payload";
import { parseConfig } from "./config.parser";

/**
 * Given an API configuration and payload, it creates the Request object for the fetch method.
 * TODO: Implement POST request.
 */


export interface AzRequest {
  url: string;
  options: RequestInit;
}

const buildURL = (config: AzConfig, payload: AzPayload): string => {
  return [
    parseConfig(config),
    config.method === "GET" ? parsePayloadGET(payload) : "",
  ].filter(i => i).join("&");
};

const buildBody = (config: AzConfig, payload: AzPayload): any => {
  config.method === "GET" ? null : {}; // TODO: Implement POST body.
}

export const CreateRequest = (config: AzConfig, payload: AzPayload): AzRequest => ({
  url: buildURL(config, payload),
  options: {
    method: config.method,
    headers: {
      "Content-Type": "application/json",
      "api-key": config.apiKey,
    },
    mode: "cors",
    body: buildBody(config, payload),
  }
});
