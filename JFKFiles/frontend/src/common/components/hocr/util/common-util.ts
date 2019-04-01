import { hocrClasses, hocrAttributes } from "../constants";

export type PageIndex = number | "auto";
export type WordComparator = (word: string) => boolean;

export interface WordPosition {
  pageIndex: number;
  firstOcurrenceNode: Element;
}


const hocrEntityMap = {
  page: hocrClasses.page,
  area: hocrClasses.area,
  paragraph: hocrClasses.paragraph,
  line: hocrClasses.line,
  word: hocrClasses.word,
}

export const resolveNodeEntity = (node: Element): string => {
  let entity = null;
  Object.keys(hocrEntityMap).some(key => {
    const match = node.classList.contains(hocrEntityMap[key]);
    if (match) entity = key;
    return match;
  });

  return entity;
};

export const CreateWordComparator = (targetWords: string[], caseSensitive: boolean = false) => {
  const parsedTargetWords = caseSensitive ? targetWords : targetWords.map(w => w.toLowerCase());
  return (word: string): boolean => {
    return parsedTargetWords.indexOf(caseSensitive ? word : word.toLowerCase()) >= 0;
  }
};

export const parseHocr = (hocr: string): Document => {
  const domParser = new DOMParser();
  return domParser.parseFromString(hocr, "text/html");
}

export const parseWordPosition = (doc: Document, pageIndex: PageIndex,
  wordComparator: WordComparator): WordPosition => {
  if (typeof pageIndex === "number") {
    const validatedPageIndex = (pageIndex < 0 || !checkPageIndexInRange(doc, pageIndex)) ? 0 : pageIndex;
    const firstOcurrenceNode = wordIdInPage(doc.body.children[validatedPageIndex], wordComparator);
    return {
      pageIndex: validatedPageIndex,
      firstOcurrenceNode,
    };
  } else {  // Auto page index based on the first ocurrence of a target word.
    return findFirstOcurrencePosition(doc, wordComparator);
  }
};

const checkPageIndexInRange = (doc: Document, pageIndex: number) => {
  return doc.body && doc.body.children && (pageIndex < doc.body.children.length);
};

const findFirstOcurrencePosition = (doc: Document, wordComparator: WordComparator): WordPosition => {
  let pos: WordPosition = { pageIndex: 0, firstOcurrenceNode: null };
  if (wordComparator) {
    Array.from(doc.body.children).some((page, index) => {
      const foundNode = wordIdInPage(page, wordComparator);
      if (foundNode) {
        pos.pageIndex = index;
        pos.firstOcurrenceNode = foundNode;
      }
      return Boolean(foundNode);
    });
  }
  return pos;
};

const wordIdInPage = (page: Element, wordComparator: WordComparator): Element => {
  const pageWords = page.getElementsByClassName(hocrEntityMap.word);
  let wordNode = null;
  Array.from(pageWords).some((word, index) => {
    const comparison = wordComparator(word.textContent);
    if (comparison) wordNode = word;
    return comparison;
  })
  return wordNode;
};

export const getNodeId = (node: Element, suffix: string = ""): string => {
  return composeId(node.getAttribute("id"), suffix);
};

export const composeId = (id: string, suffix: string = ""): string => {
  return [id, suffix].filter(s => s).join("-");
};

export const getNodeById = (parentNode: Element, id: string) => {
  return parentNode.querySelector(`#${id}`);
}

const optionArrayFields = ['bbox', 'baseline', 'scan_res'];

export const getNodeOptions = (node: Element): any => {
  const optionsStr = node["title"] ? node["title"] : "";
  const regex = /(?:^|;)\s*(\w+)\s+(?:([^;"']+?)|"((?:\\"|[^"])+?)"|'((?:\\'|[^'])+?)')\s*(?=;|$)/g;
  let match;

  let options = {};
  while (match = regex.exec(optionsStr)) {
    const name = match[1];
    let value = match[4] || match[3] || match[2];
    if (optionArrayFields.indexOf(name) !== -1) {
      value = value.split(/\s+/);
    }
    options[name] = value;
  }
  return options;
};


export interface PosSize {
  x: number,
  y: number,
  width: number,
  height: number,
}

export const bboxToPosSize = (bbox: number[]): PosSize => ({
  x: bbox[0],
  y: bbox[1],
  width: bbox[2] - bbox[0],
  height: bbox[3] - bbox[1]
})

export const getPosSizeFromDOMNode = (node: Element): PosSize => {
  if (node) {
    return {
      x: parseInt(node.getAttribute("x")),
      y: parseInt(node.getAttribute("y")),
      width: parseInt(node.getAttribute("width")),
      height: parseInt(node.getAttribute("height")),
    };
  }
  return null;
}

export const getPosSizeFromBBoxNode = (node: Element): PosSize => {
  if (!node) return null;
  const nodeOptions = getNodeOptions(node);
  if (!nodeOptions || !nodeOptions.bbox) return null;
  return bboxToPosSize(nodeOptions.bbox);
}

export const calculateNodeShiftInContainer = (target: PosSize, container: PosSize) => {
  const shiftX = target.x - container.x;
  const shiftY = target.y - container.y;
  const shiftCentroidX = shiftX + (target.width / 2);
  const shiftCentroidY = shiftY + (target.height / 2);
  const shiftCentroidXPercentage = shiftCentroidX / container.width;
  const shiftCentroidYPercentage = shiftCentroidY / container.height;

  return { x: shiftCentroidXPercentage, y: shiftCentroidYPercentage };
}

export const getAnnotationMessage = (node: Element): string => (
  Boolean(node.attributes[hocrAttributes.dataAnnotation]) ?
    node.attributes[hocrAttributes.dataAnnotation].value :
    null
);
