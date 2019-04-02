import { AzConfig } from "./config.model";

/**
 * Parsers for Config.
 * A parser will do a transformaton from Config object to a connection URL.
 */

export const parseConfig = (config: AzConfig): string => {
  const root = `${config.protocol}://${config.serviceName}.${config.serviceDomain}/`;
  const path = `${config.servicePath}?api-version=${config.apiVer}`;

  return (root + path);
}