import * as React from "react";
import { ZoomMode, HocrPageComponent } from "./hocr-page.component";
import { HocrPreviewStyleMap, injectDefaultPreviewStyle } from "./hocr-preview.style";
import {
  calculateNodeShiftInContainer,
  PageIndex,
  parseHocr,
  CreateWordComparator,
  WordComparator,
  parseWordPosition,
  getNodeById,
  PosSize,
  getPosSizeFromBBoxNode,
  composeId,
  getPosSizeFromDOMNode,
} from "../util/common-util";
import { cnc } from "../../../../util";

const style = require("./hocr-preview.style.scss");

const idSuffix = "preview";


/**
 * HOCR-Preview
 * Given an HOCR input string, it parses it and represents the document in graphic format, 
 * showing the source image the text was extracted from. It shows a placeholder for each
 * recognised word and provides the necessary wiring and events to be connected to a 
 * HocrDocumentComponent to create a whole proofreader.
 */

export interface HocrPreviewProps {
  hocr: string;
  pageIndex: PageIndex;
  zoomMode: ZoomMode;
  targetWords?: string[];
  caseSensitiveComparison?: boolean;
  renderOnlyTargetWords?: boolean;
  autoFocusId?: string;
  highlightId?: string;
  disabelScroll?: boolean;
  userStyle?: HocrPreviewStyleMap;
  onWordHover?: (wordId: string) => void;
  className?: string;
};

interface HocrPreviewState {
  pageNode: Element;
  pagePosSize: PosSize;
  wordCompare: WordComparator;
  autoFocusNode: Element;
  autoFocusPosSize: PosSize;
  safeStyle?: HocrPreviewStyleMap;
}

export class HocrPreviewComponent extends React.Component<HocrPreviewProps, HocrPreviewState> {
  constructor(props) {
    super(props);

    this.state = {
      ...this.calculateStateFromProps(props),
      safeStyle: injectDefaultPreviewStyle(props.userStyle),
    }
  }

  public static defaultProps: Partial<HocrPreviewProps> = {
    pageIndex: 'auto',
  };

  private viewportRef = null;

  private saveViewportRef = (node) => {
    this.viewportRef = node;
  }

  private scrollTo = (targetPosSize: PosSize) => {    
    if (!this.viewportRef || !targetPosSize || !this.state.pagePosSize) return;
    
    const shift = calculateNodeShiftInContainer(targetPosSize, this.state.pagePosSize);
    if (!shift) return;

    const {x, y} = shift;
    const scrollLeft = this.viewportRef.scrollWidth * x - (this.viewportRef.clientWidth * 0.5); 
    const scrollTop = this.viewportRef.scrollHeight * y - (this.viewportRef.clientHeight * 0.3);
    
    // Workd around Edge Bug
    // https://developer.microsoft.com/en-us/microsoft-edge/platform/issues/15534521/
    if(this.viewportRef.scrollTo) {
      this.viewportRef.scrollTo({left: scrollLeft, top: scrollTop});
    } else {
      this.viewportRef.scrollTop = scrollTop;
      this.viewportRef.scrollLeft = scrollLeft;
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
      const focusNode = getNodeById(this.viewportRef, composeId(nodeId, idSuffix));
      this.setHighlight(focusNode);
      this.scrollTo(getPosSizeFromDOMNode(focusNode));
      this.setState({
        ...this.state,
        autoFocusNode: focusNode,
      });
    }
  }

  private calculateStateFromProps = (newProps: HocrPreviewProps): HocrPreviewState => {    
    if (newProps.hocr) {
      const doc = parseHocr(newProps.hocr);
      const wordCompare = CreateWordComparator(newProps.targetWords, newProps.caseSensitiveComparison);
      let pageIndex = newProps.pageIndex;
      let autoFocusNode = null;
      if (pageIndex) {
        const wordPosition = parseWordPosition(doc, newProps.pageIndex, wordCompare);
        pageIndex = wordPosition.pageIndex;
        autoFocusNode = wordPosition.firstOcurrenceNode;
      }

      if (pageIndex !== null) {
        const pageNode = doc.body.children[pageIndex];
        const pagePosSize = getPosSizeFromBBoxNode(pageNode);
        const autoFocusPosSize = getPosSizeFromBBoxNode(autoFocusNode);
        return {pageNode, pagePosSize, wordCompare, autoFocusNode, autoFocusPosSize};
      }
    }
  };

  private onStateUpdated = () => {
    this.scrollTo(this.state.autoFocusPosSize);
  }

  // *** Lifecycle ***

  public componentDidMount() {
    this.onStateUpdated(); // Initial scroll on mount.
  }

  public componentWillReceiveProps(nextProps: HocrPreviewProps) {
    if( this.props.hocr !== nextProps.hocr ||
        this.props.pageIndex !== nextProps.pageIndex ||
        this.props.targetWords !== nextProps.targetWords ||
        this.props.caseSensitiveComparison !== nextProps.caseSensitiveComparison
    ) {
      this.setState({
        ...this.state,
        ...this.calculateStateFromProps(nextProps),
      }, this.onStateUpdated);
    } else if ( this.props.userStyle != nextProps.userStyle ) {
      this.setState({
        ...this.state,
        safeStyle: injectDefaultPreviewStyle(nextProps.userStyle),
      });
    } else if ( this.props.autoFocusId != nextProps.autoFocusId ) {
      this.autoFocusToNode(nextProps.autoFocusId);
    };
  }

  public shouldComponentUpdate(nextProps: HocrPreviewProps, nextState: HocrPreviewState) {
    const shouldUpdate = ( 
      this.props.hocr !== nextProps.hocr ||
      this.props.pageIndex !== nextProps.pageIndex ||
      this.props.zoomMode !== nextProps.zoomMode ||
      this.props.targetWords !== nextProps.targetWords ||
      this.props.caseSensitiveComparison !== nextProps.caseSensitiveComparison ||
      this.props.renderOnlyTargetWords !== nextProps.renderOnlyTargetWords ||
      this.props.disabelScroll !== nextProps.disabelScroll ||
      this.props.userStyle !== nextProps.userStyle ||
      this.props.onWordHover !== nextProps.onWordHover ||
      this.props.className !== nextProps.className ||
      this.state.pageNode !== nextState.pageNode ||
      this.state.wordCompare !== nextState.wordCompare
    );
    return shouldUpdate;
  }

  public render() {
    return (
      <div className={cnc(style.container, this.props.className)}>
        <div className={cnc(style.viewport, this.props.disabelScroll && style.noScrollable )}
        ref={this.saveViewportRef}
        >
          <HocrPageComponent
            node={this.state.pageNode}
            wordCompare={this.state.wordCompare}
            idSuffix={idSuffix}
            zoomMode={this.props.zoomMode}
            renderOnlyTargetWords={this.props.renderOnlyTargetWords}
            userStyle={this.state.safeStyle}
            onWordHover={this.props.onWordHover}
          />
        </div>
      </div>
    );
  }
};
