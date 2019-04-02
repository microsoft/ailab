/**
 * Object that represents API conection parameters.
 */

export type GraphMethodType = "GET";

export interface GraphConfig {
  protocol: string;
  serviceName: string;
  serviceDomain: string;
  servicePath: string;
  method: GraphMethodType;
  authCodeParam: string;
}
