import { HocrPageStyleMap, injectDefaultPageStyle } from "./hocr-page.style";
import { HocrNodeStyleMap, injectDefaultNodeStyle } from "./hocr-node.style";

export interface HocrPreviewStyleMap extends HocrPageStyleMap, HocrNodeStyleMap {};

export const injectDefaultPreviewStyle = (userStyle: HocrPreviewStyleMap): HocrPreviewStyleMap => {
  return {
    ...injectDefaultPageStyle(userStyle),
    ...injectDefaultNodeStyle(userStyle),
  };
}