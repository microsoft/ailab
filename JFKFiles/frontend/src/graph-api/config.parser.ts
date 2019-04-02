import { GraphConfig } from "./config.model";

/**
 * Parsers for Config.
 * A parser will do a transformation from Config object to a connection URL.
 */

export const parseConfig = (config: GraphConfig): string => {
  const root = `${config.protocol}://${config.serviceName}.${config.serviceDomain}/`;
  const path = `${config.servicePath}?`;
  const authCode = `${config.authCodeParam}&`;
  return (root + path + authCode);
}