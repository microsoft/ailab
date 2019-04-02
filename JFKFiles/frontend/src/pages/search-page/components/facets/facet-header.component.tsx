import * as React from "react";
import { Facet } from "../../view-model";
import { Chevron } from "../../../../common/components/chevron";
import { CardActions } from "material-ui/Card";
import { Icon } from "material-ui";
import Typography from "material-ui/Typography";

const style = require("./facet-header.style.scss");


interface FacetHeaderProps {
  facet: Facet;
  expanded: boolean;
  onToggleExpanded: () => void;
}

export const FacetHeaderComponent: React.StatelessComponent<FacetHeaderProps> = (props) => {
  return (
    <CardActions classes={{root: style.actions}} color="inherit">
      <div className={style.title}>
        { props.facet.iconName ? 
          <Icon classes={{root: style.icon}} color="action">
            {props.facet.iconName}
          </Icon>
          : null
        }            
        <span>{props.facet.displayName.toUpperCase()}</span>
      </div>          
      <Chevron onClick={props.onToggleExpanded} expanded={props.expanded}/>
    </CardActions>
  );
};