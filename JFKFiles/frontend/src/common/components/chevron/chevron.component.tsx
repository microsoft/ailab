import * as React from "react"
import IconButton from "material-ui/IconButton";
import { cnc } from "../../../util";

const style = require("./chevron.style.scss");


interface ChevronProps {
  expanded: boolean;
  onClick: () => void;
  className?: string;
}

export const Chevron: React.StatelessComponent<ChevronProps> = (props) => {
  return (
    <IconButton
      classes={{
        root: props.className,
        label: cnc(props.className, style.chevron, props.expanded && style.chevronUp),
      }}
      color={props.expanded ? "primary" : "inherit"}
      onClick={props.onClick}
      aria-expanded={props.expanded}
      aria-label="Show more"      
    >
      &#xe906;
    </IconButton>
  )
}
