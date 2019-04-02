import * as React from "react";
import Hidden from "material-ui/Hidden";
import Drawer from "material-ui/Drawer";
import { DrawerBarComponent } from "./drawer-bar.component";
import { MenuButton } from "../../../../common/components/menu-button";
import { cnc } from "../../../../util";
import { Service } from "../../service";

const style = require("./drawer.style.scss");


interface DrawerProps {
  activeService: Service;
  show: boolean;
  onClose: () => void;
  onMenuClick: () => void;
  className?: string;
}

const DrawerContent: React.StatelessComponent<DrawerProps> = (props) => (
  <>
    <DrawerBarComponent
      viewMode={props.show ? "open" : "closed"}
      activeService={props.activeService}
      onClose={props.onClose}
      onMenuClick={props.onMenuClick}
    />
    { props.show ? props.children : null }
  </>
);

const DrawerForMobileComponent: React.StatelessComponent<DrawerProps> = (props) => {
  return (
    <Hidden smUp>
      <Drawer classes={{
          paper: style.drawerPaperMobile 
        }}
        variant="temporary"
        color="inherit"
        anchor="left"
        open={props.show}
        onClose={props.onClose}
        ModalProps={{
          keepMounted: true, // Better open performance on mobile.
        }}
      >
        <DrawerContent {...{...props, show: true}} />
      </Drawer>
    </Hidden>
  );
};

const DrawerForDesktopComponent: React.StatelessComponent<DrawerProps> = (props) => {
  return (
    <Hidden xsDown>
      <Drawer classes={{
          docked: style.drawerDock,
          paper: props.show ? style.drawerPaperDesktop : style.drawerPaperDesktopHidden,
        }}
        variant="permanent"
        color="inherit"
        anchor="left"
        open={props.show}
        onClose={props.onClose}
        elevation={8}
      >
        <DrawerContent {...props} />
      </Drawer>
    </Hidden>
  );
};

const DrawerComponent: React.StatelessComponent<DrawerProps> = (props) => {
  return (
    <div className={props.className}>
      <DrawerForMobileComponent {...props}>
        {props.children}
      </DrawerForMobileComponent>
      <DrawerForDesktopComponent {...props}>
        {props.children}
      </DrawerForDesktopComponent>
    </div>
  );
};

export { DrawerComponent };
