import { CreateAzApi, AzResponse } from "../../../az-api";
import { Service, ServiceConfig, StateReducer, MapperToState,} from "./service.model";


export const CreateService = (config: ServiceConfig): Service => {
  const {searchConfig, suggestionConfig} = config;
  const searchApi = CreateAzApi(searchConfig.apiConfig, searchConfig.responseConfig);
  const suggestionApi = CreateAzApi(suggestionConfig.apiConfig, suggestionConfig.responseConfig);
  const throwInvalidPayload = () => {throw "Invalid Payload";}
  
  return {
    config,
    
    search: async <S>(state: S): Promise<StateReducer> => {
      try {
        const payload = config.searchConfig.mapStateToPayload(state, config);
        if (!payload) throwInvalidPayload();
        const response = await searchApi.runQuery(payload);
        
        // Return a state reducer.
        return <S>(updatedState: S): S => {
          return config.searchConfig.mapResponseToState(updatedState, response, config);
        };
      } catch (e) {
        throw e;
      }
    },

    suggest: async <S>(state: S): Promise<StateReducer> => {
      try {
        const payload = config.suggestionConfig.mapStateToPayload(state, config);
        if (!payload) throwInvalidPayload();
        const response = await suggestionApi.runQuery(payload);
        
        // Return a state reducer.
        return <S>(updatedState: S): S => {
          return config.suggestionConfig.mapResponseToState(updatedState, response, config);
        };
      } catch (e) {
        throw e;
      }
    },
  };
};
