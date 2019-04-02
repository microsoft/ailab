import { GraphConfig } from "./config.model";
import { GraphResponseConfig, defaultGraphResponseConfig, GraphResponse } from "./response.model";
import { parseResponse } from "./response.parser";
import { CreateRequest } from "./request";
import { GraphPayload } from "./payload.model";

/**
 * Main entry point. An API object represents an interface to run queries. An API object
 * is created by passing a config object. Once created, we can run queries by calling
 * 'runQuery' with the desired payload.
 */

export interface GraphApi {
  setConfig: (config: GraphConfig) => GraphApi;
  setResponseConfig: (responseConfig: GraphResponseConfig) => GraphApi;
  runQuery: (payload: GraphPayload) => Promise<GraphResponse>;
}

export const CreateGraphApi = (config: GraphConfig,
  responseConfig: GraphResponseConfig = defaultGraphResponseConfig): GraphApi => {
  return {
    setConfig(newConfig) {
      return CreateGraphApi(newConfig, responseConfig);
    },
    setResponseConfig(newResponseConfig) {
      return CreateGraphApi(config, newResponseConfig);
    },
    async runQuery(payload) {
      try {
        const request = CreateRequest(config, payload);
        console.debug("Running Query:", request.url); // Debug only.
        const response = await fetch(request.url, request.options);
        return await parseResponse(response, responseConfig);
      } catch (e) {
        throw new Error(e);
      }
    },
  };
};

