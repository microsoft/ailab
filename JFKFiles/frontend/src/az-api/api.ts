import { AzConfig, defaultAzConfig } from "./config.model";
import { AzResponseConfig, defaultAzResponseConfig, AzResponse } from "./response.model";
import { parseResponse } from "./response.parser";
import { CreateRequest } from "./request";
import { AzPayload } from "./payload";

/**
 * Main entry point. An API object represents an user interface to run queries. An API object
 * is created by passing a config object (and optionally a response config, rarely used).
 * Once created, we can run queries by calling 'runQuery' with the desired payload.
 */

export interface AzApi {
  setConfig: (config: AzConfig) => AzApi;
  setResponseConfig: (responseConfig: AzResponseConfig) => AzApi;
  runQuery: (payload: AzPayload) => Promise<AzResponse>;
}

export const CreateAzApi = (config: AzConfig, responseConfig: AzResponseConfig = defaultAzResponseConfig): AzApi => {
  return {
    setConfig(newConfig) {
      return CreateAzApi(newConfig, responseConfig);
    },
    setResponseConfig(newResponseConfig) {
      return CreateAzApi(config, newResponseConfig);
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
