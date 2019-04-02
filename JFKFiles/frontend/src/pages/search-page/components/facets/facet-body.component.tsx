import * as React from "react";
import { Facet, Filter } from "../../view-model";
import Collapse from "material-ui/transitions/Collapse";
import { CreateSelectionControl } from "../selection-controls";

const style = require("./facet-body.style.scss");


interface FacetBodyProps {
  facet: Facet;
  expanded: boolean;
  filter: Filter;
  onFilterUpdate: (newFilter: Filter) => void;
}

export const FacetBodyComponent: React.StatelessComponent<FacetBodyProps> = (props) => {
  return (
    <Collapse in={props.expanded} timeout="auto">
      <div className={style.body}>
        {CreateSelectionControl(props.facet, props.filter, props.onFilterUpdate)}
      </div>          
    </Collapse>
  );
};