import * as React from "react";

const style = require("./spacer.style.scss");

export const SpacerComponent = (props) => {
  return (
    <div className={style.spacer}>
      {props.children}
    </div>
  );
}
