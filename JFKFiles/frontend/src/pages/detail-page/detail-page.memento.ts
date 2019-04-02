import { DetailRouteState } from ".";


let detailState : DetailRouteState = {hocr: "", targetWords: []};

export const setDetailState = (state : DetailRouteState) => {
  detailState = state;
}

export const getDetailState = () : DetailRouteState => {
  return detailState;
}
