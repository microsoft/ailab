const style = require("./hocr-document.style.scss");

export interface HocrDocumentStyleMap {
  page?: string;
  area?: string;
  paragraph?: string;
  line?: string;
  word?: string;
  target?: string;
  highlight?: string;  
};

export const defaultDocumentStyle: HocrDocumentStyleMap = {
  page: style.page,
  area: style.area,
  paragraph: style.par,
  line: style.line,
  word: style.word,
  target: style.target,
  highlight: style.highlight,  
}

export const injectDefaultDocumentStyle = (userStyle: HocrDocumentStyleMap): HocrDocumentStyleMap => {
  return {
    ...defaultDocumentStyle,
    ...userStyle,
  };
}
