import * as React from "react";
import { MicrosoftSvg } from "./svg.component";


export const LogoMicrosoftComponent = ({ classes, colorful }) => (
  <div className={classes.container}>
    <MicrosoftSvg
      className={classes.svg}
      colorful={colorful}
    />
  </div>
);
