import * as React from "react";
import { getDocNodeChildrenComponents } from "./hocr-docnode.component";
import { injectDefaultDocumentStyle, HocrDocumentStyleMap } from "./hocr-document.style";
import {
  parseHocr,
  CreateWordComparator,
  WordComparator,
  resolveNodeEntity,
  getNodeById,
} from "../util/common-util";
import { cnc } from "../../../../util";

const style = require("./hocr-document.style.scss");


/**
 * HOCR Document.
 * Given an HOCR input string, it parses it and represents the document in text format.
 * It allows user to navigate through document pages, highlighting the hovered items, and
 * also provides the necessary wiring and events to be connected to a HocrPreviewComponent
 * to create a whole proofreader.
 */

export interface HocrDocumentProps {
  hocr: string;
  targetWords?: string[];
  caseSensitiveComparison?: boolean;
  autoFocusId?: string;
  hightlightPageIndex?: number;
  userStyle?: HocrDocumentStyleMap;
  onWordHover?: (wordId: string) => void;
  onPageHover?: (pageIndex: number) => void;
  className?: string;
};

interface HocrDocumentState {
  docBody: Element;
  wordCompare: WordComparator;
  autoFocusNode: Element;
  hightlightedPageNode: Element;
  safeStyle: HocrDocumentStyleMap;
}

export class HocrDocumentComponent extends React.Component<HocrDocumentProps, HocrDocumentState> {
  constructor(props) {
    super(props);

    this.state = {
      docBody: getDocumentBody(props.hocr),
      wordCompare: CreateWordComparator(props.targetWords, props.caseSensitiveComparison),
      safeStyle: injectDefaultDocumentStyle(props.userStyle),
      autoFocusNode: null,
      hightlightedPageNode: null,
    }
  }

  private viewportRef = null;

  private saveViewportRef = (node) => {
    this.viewportRef = node;
  }

  private scrollTo = (node: Element) => {
    if (node) {
      const verticalShift = node.getBoundingClientRect().top - this.viewportRef.getBoundingClientRect().top;
      const scrollTop = this.viewportRef.scrollTop + verticalShift - (this.viewportRef.clientHeight / 2);
      this.viewportRef.scrollTop = scrollTop;
    }
  }
  
  private resetHighlight = (node: Element) => {
    if (node) node.classList.remove(this.state.safeStyle["highlight"]);
  }

  private setHighlight = (node: Element) => {
    if (node) node.classList.add(this.state.safeStyle["highlight"]);
  }

  private autoFocusToNode = (nodeId: string) => {
    this.resetHighlight(this.state.autoFocusNode);
    if (nodeId) {      
      const focusNode = getNodeById(this.viewportRef, nodeId);
      this.setHighlight(focusNode);
      this.scrollTo(focusNode);
      this.setState({
        ...this.state,
        autoFocusNode: focusNode,
      });
    }
  }

  private hightlightPage = (pageIndex: number) => {
    this.resetHighlight(this.state.hightlightedPageNode);
    if (pageIndex) {
      const pageNode = getNodeById(this.viewportRef, `page_${pageIndex}`);
      this.setHighlight(pageNode);
      this.setState({
        ...this.state,
        hightlightedPageNode: pageNode,
      });
    }
  }

  // *** Lifecycle ***

  public componentDidMount() {
    if ( this.viewportRef) {
      this.props.autoFocusId && this.scrollTo(getNodeById(this.viewportRef, this.props.autoFocusId));
      this.props.hightlightPageIndex && this.hightlightPage(this.props.hightlightPageIndex);
    }
  }

  public componentWillReceiveProps(nextProps: HocrDocumentProps) {
    if( this.props.hocr !== nextProps.hocr) {
      this.setState({
        ...this.state,
        docBody: getDocumentBody(nextProps.hocr)
      })
    } else if (
        this.props.targetWords !== nextProps.targetWords ||
        this.props.caseSensitiveComparison !== nextProps.caseSensitiveComparison
    ) {
      this.setState({
        ...this.state,
        wordCompare: CreateWordComparator(nextProps.targetWords, nextProps.caseSensitiveComparison),
      });
    } else if ( this.props.userStyle != nextProps.userStyle ) {
      this.setState({
        ...this.state,
        safeStyle: injectDefaultDocumentStyle(nextProps.userStyle),
      });
    } 
    
    if (this.props.autoFocusId !== nextProps.autoFocusId) {
      this.autoFocusToNode(nextProps.autoFocusId);
    }
    if (this.props.hightlightPageIndex !== nextProps.hightlightPageIndex) {
      this.hightlightPage(nextProps.hightlightPageIndex);
    }
  }

  public shouldComponentUpdate(nextProps: HocrDocumentProps, nextState: HocrDocumentState) {
    return ( 
      this.props.hocr !== nextProps.hocr ||
      this.props.targetWords !== nextProps.targetWords ||
      this.props.caseSensitiveComparison !== nextProps.caseSensitiveComparison ||
      this.props.userStyle !== nextProps.userStyle ||
      this.props.onWordHover !== nextProps.onWordHover ||
      this.props.onPageHover !== nextProps.onPageHover ||
      this.props.className !== nextProps.className ||
      this.state.docBody !== nextState.docBody ||
      this.state.wordCompare !== nextState.wordCompare
    );
  }

  public render() {
    if (!this.state.docBody || !this.state.docBody.children) return null;
    return (
      <div className={cnc(style.container, this.props.className)}>
        <div className={style.viewport} ref={this.saveViewportRef}>
         { getDocNodeChildrenComponents({
            node: this.state.docBody,
            index: 0,
            wordCompare: this.state.wordCompare,
            userStyle: this.state.safeStyle,
            onWordHover: this.props.onWordHover,
            onPageHover: this.props.onPageHover,
         })}
        </div>
      </div>
    );
  }
};

const getDocumentBody = (hocr: string) => {
  if (!hocr) return null;
  const doc = parseHocr(hocr);
  return doc.body
}
