import * as React from "react";
import { cnc } from "../../../../util";

const style = require("./tooltip.style.scss");


interface TooltipProps {
  show: boolean;
  top: number;
  left: number;
  className?: string;
}

export const TooltipComponent: React.StatelessComponent<TooltipProps> = (props) => {
  return props.show ? (
    <div
      className={cnc(style.popup, props.className)}
      style={{
        top: props.top,
        left: props.left,
      }}
    >
      {props.children}
    </div>
  ) : null;
}
