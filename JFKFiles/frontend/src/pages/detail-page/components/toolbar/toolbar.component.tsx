import * as React from "react";
import { Link } from "react-router-dom";
import { VerticalSeparator } from "./../../../../common/components/vertical-separator";
import { ZoomMode } from "../../../../common/components/hocr";
import AppBar from "material-ui/AppBar";
import Toolbar from "material-ui/Toolbar";
import Button from "material-ui/Button";
import IconButton from "material-ui/IconButton";

const style = require("./toolbar.style.scss");


/**
 * Main toolbar for Detail page.
 */

interface ToolbarProps {
  zoomMode: ZoomMode;
  onToggleTextClick: () => void;
  onZoomChange: (zoomMode: ZoomMode) => void;
  onCloseClick: () => void;
}

export class ToolbarComponent extends React.Component<ToolbarProps, {}> {
  constructor(props) {
    super(props);
  }

  private handleZoomClick = (zoomMode: ZoomMode) => () => {
    this.props.onZoomChange(zoomMode);
  }

  public render() {
    return (
      <Toolbar classes={{ root: style.toolbar }} disableGutters={true}>
        <div className={style.group}>
          <ToggleViewButton onClick={this.props.onToggleTextClick} />
          <VerticalSeparator />
          <OriginalSizeButton
            zoomMode={this.props.zoomMode}
            onClick={this.handleZoomClick("original")}
          />
          <PageWidthButton
            zoomMode={this.props.zoomMode}
            onClick={this.handleZoomClick("page-width")}
          />
          <PageFullButton
            zoomMode={this.props.zoomMode}
            onClick={this.handleZoomClick("page-full")}
          />
        </div>
        <div className={style.group}>
          <CloseButton onClick={this.props.onCloseClick} />
        </div>
      </Toolbar>
    );
  }
}

const ToggleViewButton = ({ onClick }) => (
  <Button
    classes={{ root: style.toggleView }}
    color="inherit"
    onClick={onClick}
  >
    Toggle View
  </Button>
);

const toggleColor = (targetMode: ZoomMode, zoomMode: ZoomMode) => {
  return targetMode === zoomMode ? "primary" : "inherit";
}

const OriginalSizeButton = ({ zoomMode, onClick }) => (
  <IconButton
    classes={{ label: style.toggleIcon }}
    color={toggleColor("original", zoomMode)}
    onClick={onClick}
  >
    &#xe911;
  </IconButton>
);

const PageWidthButton = ({ zoomMode, onClick }) => (
  <IconButton
    classes={{ label: style.toggleIcon }}
    color={toggleColor("page-width", zoomMode)}
    onClick={onClick}
  >
    &#xe907;
  </IconButton>
);

const PageFullButton = ({ zoomMode, onClick }) => (
  <IconButton
    classes={{ label: style.toggleIcon }}
    color={toggleColor("page-full", zoomMode)}
    onClick={onClick}
  >
    &#xe908;
  </IconButton>
);

const CloseButton = ({ onClick }) => (
  <IconButton
    classes={{ label: style.closeIcon, root: style.closeButton }}
    color="inherit"
    onClick={onClick}
  >
    &#xe905;
  </IconButton>
);
