/**
 * Object that represents API conection parameters, set and forget parameters
 * or those parameters that do not change frequently.
 */

export interface AzConfig {
  protocol: string;
  serviceName: string;
  serviceDomain: string;
  servicePath: string;
  apiVer: string;
  apiKey: string;
  method: "GET" | "POST";
}

export const defaultAzConfig: AzConfig = {
  protocol: "https",
  serviceName: "",
  serviceDomain: "search.windows.net",
  servicePath: "",
  apiVer: "2017-11-11",
  apiKey: "",
  method: "GET",
}
