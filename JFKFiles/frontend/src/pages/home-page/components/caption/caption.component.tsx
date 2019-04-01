import * as React from "react"

const style = require("./caption.style.scss");


export const CaptionComponent = () => (
  <div className={style.caption}>
    <p className={style.title}>Documents revealed.</p>
    <p className={style.subtitle}>Let's find out what happened that day.</p>
  </div>
);