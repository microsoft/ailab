import * as React from "react";
import { Link } from 'react-router-dom';
import { HocrPreviewComponent, HocrPreviewStyleMap } from "../hocr-preview";
import { HocrDocumentComponent, HocrDocumentStyleMap } from "../hocr-document";
import { PageIndex } from "../util/common-util";
import { ZoomMode } from "../hocr-preview";
import { cnc } from "../../../../util";

const style = require("./hocr-proofreader.style.scss");


/**
 * HOCR Proofreader
 */

interface HocrProofreaderProps {
  hocr: string;
  targetWords: string[];
  zoomMode?: ZoomMode;
  showText?: boolean;
  previewStyle?: HocrPreviewStyleMap;
  documentStyle?: HocrDocumentStyleMap;
  className?: string;
  pageIndex: PageIndex;
}

interface HocrProofreaderState {
  docIdHighlighted: string;
  previewIdHightlighted: string;
}

export class HocrProofreaderComponent extends React.PureComponent<HocrProofreaderProps, HocrProofreaderState> {
  constructor(props) {
    super(props);

    this.state = {
      docIdHighlighted: `page_${this.props.pageIndex}`, // Start focusing initial page also in document view.
      previewIdHightlighted: null,
    };
    this.fixIndex = this.props.pageIndex;
  }

  private fixIndex: PageIndex; // **FIX. Read below.

  private handleDocumentWordHover = (id: string) => {
    this.setState({
      ...this.state,
      previewIdHightlighted: id,
    });
  }

  private handleDocumentPageHover = (index: number) => {
    this.fixIndex = index;  // **FIX. Read below.
  }

  private handlePreviewWordHover = (id: string) => {
    this.setState({
      ...this.state,
      docIdHighlighted: id,
    });
  }

  public render() {
    return (
      <div className={cnc(style.container, this.props.className)}>
        <HocrPreviewComponent
          className={style.hocrPreview}
          hocr={this.props.hocr}
          zoomMode={this.props.zoomMode}
          pageIndex={this.fixIndex}
          autoFocusId={this.state.previewIdHightlighted}
          targetWords={this.props.targetWords}
          onWordHover={this.handlePreviewWordHover}
          userStyle={this.props.previewStyle}
        />
        <HocrDocumentComponent
          className={this.props.showText ? style.hocrDocument : style.hocrDocumentHidden}
          hocr={this.props.hocr}
          targetWords={this.props.targetWords}
          hightlightPageIndex={Number(this.fixIndex)}
          autoFocusId={this.state.docIdHighlighted}
          onWordHover={this.handleDocumentWordHover}
          onPageHover={this.handleDocumentPageHover}
          userStyle={this.props.documentStyle}
        />
      </div>
    );
  }
}


// **FIX. Apparently, there should be a bug in React that makes setState not finish
// when scrolling fast through document pages. PageIndex does not get updated
// in the state eventhough a setState is called with the new index. If we store the
// index in a private variable, it works. For some reason, it seems that setState
// got interrupted.
// Although not exactly the same, some related issues:
// https://github.com/facebook/react/issues/10906
// https://github.com/facebook/react/issues/11164
// https://github.com/facebook/react/issues/11152
// setState inside an event handler may lead to some issues. I read somewhere that
// React has to wait for the event to finish, so maybe the setState is skipped.
