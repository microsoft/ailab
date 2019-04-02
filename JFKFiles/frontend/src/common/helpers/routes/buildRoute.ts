export const buildRoute = (baseRoute: string, params: any): string => (
  Boolean(params) ?
    Object.keys(params).reduce((route, key) => (
      replaceRouteParam(route, key, params[key])
    ), baseRoute) : ''
);

const replaceRouteParam = (baseRoute: string, key: string, value) => (
  Boolean(baseRoute) ?
    baseRoute.replace(new RegExp(`:${key}`, 'g'), value)
    : ''
);
