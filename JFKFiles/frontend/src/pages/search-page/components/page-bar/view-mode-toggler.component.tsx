import * as React from "react";
import { ResultViewMode } from "../../view-model";
import IconButton from "material-ui/IconButton";
import { cnc } from "../../../../util";

const style = require("./view-mode-toggler.style.scss");


interface ViewModeTogglerProps {
  resultViewMode: ResultViewMode;
  onChangeResultViewMode: (newMode: ResultViewMode) => void;
  pulseToggle?: ResultViewMode;
}

const toggleColor = (props: ViewModeTogglerProps) => (viewMode: ResultViewMode) => {
  return props.resultViewMode === viewMode ? "primary" : "inherit";
}

const pulseToggle = (props: ViewModeTogglerProps, toggle: ResultViewMode) => {
  return Boolean((toggle === props.pulseToggle) && (toggle !== props.resultViewMode));
}

const notifyModeChanged = (props: ViewModeTogglerProps) => (newMode: ResultViewMode) => () =>{
  return props.onChangeResultViewMode(newMode);
}

export const ResultViewModeToggler = (props: ViewModeTogglerProps) => {
  const toggleColorFunc = toggleColor(props);
  const notifyModeChangedFunc = notifyModeChanged(props);
  const iconStyle = (toggle: ResultViewMode) => ({
    label: cnc(style.icon, pulseToggle(props, toggle) && style.pulse),
  });

  return (
    <>
      <IconButton
        classes={iconStyle("list")}
        color={toggleColorFunc("list")}
        onClick={notifyModeChangedFunc("list")}
      >
        &#xe903;
      </IconButton>
      {
        false &&
        <IconButton
          classes={iconStyle("grid")}
          color={toggleColorFunc("grid")}
          onClick={notifyModeChangedFunc("grid")}
        >
          &#xe902;
        </IconButton>
      }
      <IconButton
        classes={iconStyle("graph")}
        color={toggleColorFunc("graph")}
        onClick={notifyModeChangedFunc("graph")}
      >
        &#xe904;
      </IconButton>
    </>
  );
}
