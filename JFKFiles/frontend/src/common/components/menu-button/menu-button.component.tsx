import * as React from "react";
import IconButton from "material-ui/IconButton";
import { cnc } from "../../../util";

const style = require("./menu-button.style.scss");

export const MenuButton = ({ onClick, className = "" }) => (
  <IconButton
    className={className}
    classes={{label: style.icon}}
    color="inherit"
    aria-label="Menu"
    onClick={onClick ? onClick : () => {}}
  >
    &#xe901;
  </IconButton>
);