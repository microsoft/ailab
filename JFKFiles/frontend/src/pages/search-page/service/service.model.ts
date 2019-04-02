import { FacetCollection } from "../view-model";
import { AzConfig, AzPayload, AzResponse, AzResponseConfig } from "../../../az-api";
import { GraphConfig } from "../../../graph-api";

export type MapperToPayload = (state: any, config: ServiceConfig) => AzPayload;
export type MapperToState = (state: any, response: AzResponse, config: ServiceConfig) => any;

export interface ActionConfig {
  apiConfig: AzConfig;
  responseConfig?: AzResponseConfig;
  defaultPayload?: AzPayload;
  mapStateToPayload: MapperToPayload;
  mapResponseToState: MapperToState;
}

export interface ServiceConfig {
  serviceId: string;
  serviceName: string;
  serviceIcon: string;
  searchConfig: ActionConfig;
  suggestionConfig: ActionConfig;
  initialState?: any;
  graphConfig: GraphConfig;
}

export type StateReducer = <S>(state: S) => S;

export interface Service {
  config: ServiceConfig;
  search: <S>(state: S) => Promise<StateReducer>;
  suggest: <S>(state: S) => Promise<StateReducer>;
}
