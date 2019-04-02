import * as React from "react";
import { ResultViewMode } from "../../view-model";
import { MenuButton } from "../../../../common/components/menu-button";
import { LogoJFKComponent } from "./../../../../common/components/logo-jfk";
import { ResultViewModeToggler } from "./view-mode-toggler.component";
import AppBar from "material-ui/AppBar";
import Toolbar from "material-ui/Toolbar";

const style = require("./page-bar.style.scss");


interface BarProps{
  resultViewMode: ResultViewMode;
  pulseToggle?: ResultViewMode;
  onChangeResultViewMode: (newMode: ResultViewMode) => void;
  onMenuClick: () => void;
}

export const PageBarComponent = (props) => {
  return (
    <AppBar 
      classes={{root: style.appbar}}
      color="inherit"
      position="static"
    >
      <Toolbar classes={{root: style.toolbar}} disableGutters={true}>
        <MenuButton
          className={style.menuButton}
          onClick={props.onMenuClick}
        />
        <LogoJFKComponent classes={{container: style.logoContainer, svg: style.logoSvg}}/>
        <ResultViewModeToggler {...props} />
      </Toolbar>
    </AppBar>
  )
}
