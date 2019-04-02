import {State} from './state.model';

let mementoState : State = null;

export const storeState = (state : State) => {
  mementoState = state;
}

export const isLastStateAvailable = () : boolean => (mementoState !== null);

export const restoreLastState = () : State => {
  return mementoState;
}