import * as React from "react";
import { JFKSvg } from "./svg.component";


export const LogoJFKComponent = ({ classes }) => (
  <div className={classes.container}>
    <JFKSvg
      className={classes.svg}
    />
  </div>
);
