import * as React from "react";
import Toolbar from "material-ui/Toolbar";
import Icon from "material-ui/Icon";
import IconButton from "material-ui/IconButton";
import Typography from "material-ui/Typography";
import { cnc } from "../../../../util";
import { Service } from "../../service";
import { MenuButton } from "../../../../common/components/menu-button";

const style = require("./drawer-bar.style.scss");


interface DrawerBarProps {
  viewMode: "open" | "closed";
  activeService: Service;
  onClose: () => void;
  onMenuClick: () => void;
  className?: string;
}

const DrawerBarCaption = () => (
  <div className={style.caption}>
    <p className={style.title} color="inherit">
      Documents revealed.
    </p>
    <p className={style.subtitle} color="inherit">
      Let's find out what happened that day.
    </p>
  </div>
);

const DrawerBarOpenContent = ({activeService, onClose}) => (
  <>
    <DrawerBarCaption />
    <IconButton
      classes={{label: style.closeIcon}}
      color="inherit"
      aria-label="Close"
      onClick={onClose}
    >
      &#xe905;
    </IconButton>
  </>
);

const DrawerBarClosedContent = ({onMenuClick}) => (
  <MenuButton onClick={onMenuClick}/>
);

export const DrawerBarComponent: React.StatelessComponent<DrawerBarProps> = (props) => {
  const containerStyle = props.viewMode === "open" ? style.container : style.containerClosed;
  return (
    <Toolbar 
      classes={{root: cnc(props.className, containerStyle)}}
      disableGutters
    >
      {
        props.viewMode === "open" ?
        <DrawerBarOpenContent
          activeService={props.activeService}
          onClose={props.onClose}
        />
        : <DrawerBarClosedContent onMenuClick={props.onMenuClick}/>
      }
    </Toolbar>
  );
};
