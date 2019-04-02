import * as React from "react";
import Divider from "material-ui/Divider";
import { cnc } from "../../../util";

const style = require("./horizontal-separator.style.scss");

interface HorizontalSeparatorProps {
  className?: string;
}

export const HorizontalSeparator = (props: HorizontalSeparatorProps) => (
  <Divider classes={{root: cnc(props.className, style.separator)}}/>
);

