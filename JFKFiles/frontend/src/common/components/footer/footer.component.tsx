import * as React from "react";
import { LogoMicrosoftComponent } from "../logo-microsoft";
import { cnc } from "../../../util";

const style = require("./footer.style.scss");

const Links = () => (
  <div className={style.linkArea}>
    <a className={style.link} href="https://technet.microsoft.com/en-US/cc300389.aspx" target="__blank">Terms of Use</a>
    <a className={style.link} href="https://go.microsoft.com/fwlink/?LinkId=248681" target="__blank">Privacy</a>    
  </div>
);

const Statement = () => (
  <div className={style.statementArea}>
    © Microsoft 2018
  </div>
);

export const FooterComponent = ({className = null}) => {
  return (
    <footer className={cnc(style.footer, className)}>
      <Statement />
      <a href="https:\\www.microsoft.com" target="__blank">
        <LogoMicrosoftComponent colorful={false} classes={{container: style.logoContainer, svg: style.logoSvg }} />
      </a> 
      <Links />
    </footer>
  );
}