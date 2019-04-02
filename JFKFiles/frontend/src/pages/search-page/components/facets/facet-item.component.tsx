import * as React from "react";
import Card from "material-ui/Card";
import { FacetHeaderComponent } from "./facet-header.component";
import { FacetBodyComponent } from "./facet-body.component";
import { Facet, Filter } from "../../view-model";

const style = require("./facet-item.style.scss");


interface FacetItemProps {
  facet: Facet;
  filter: Filter;
  onFilterUpdate: (newFilter: Filter) => void;
}

interface State {
  expanded: boolean;
}

export class FacetItemComponent extends React.Component<FacetItemProps, State> {
  constructor(props) {
    super(props);

    this.state = {
      expanded: true,
    }
  }

  private toggleExpand = () => {
    this.setState({
      ...this.state,
      expanded: !this.state.expanded,
    });
  }
    
  public render() {
    const { facet, filter, onFilterUpdate } = this.props;
    const { expanded } = this.state;

    if (!facet.values) { return null }

    return (
      <Card
        className={style.card}
        color="inherit"
        elevation={0}
      >
        <FacetHeaderComponent
          facet={facet}
          expanded={expanded}
          onToggleExpanded={this.toggleExpand}
        />
        <FacetBodyComponent
          facet={facet}
          expanded={expanded}
          filter={filter}
          onFilterUpdate={onFilterUpdate}
        />
      </Card>
    );
  }  
}
