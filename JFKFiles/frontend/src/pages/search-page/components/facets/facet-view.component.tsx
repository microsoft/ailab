import * as React from "react"
import { FacetCollection, FilterCollection, Filter } from "../../view-model";
import { FacetItemComponent } from "./facet-item.component";

const style = require("./facet-view.style.scss");


interface FacetViewProps {
  facets: FacetCollection;
  filters: FilterCollection;
  onFilterUpdate: (newFilter: Filter) => void;
}

class FacetViewComponent extends React.PureComponent<FacetViewProps> {
  render() {
    return this.props.facets ? (
      <div className={style.container}>
        {this.props.facets.map((facet, index) => {
          const filter = this.props.filters ?
            this.props.filters.find(f => f.fieldId === facet.fieldId) : null;
          return (
            <FacetItemComponent
              facet={facet}
              filter={filter}
              onFilterUpdate={this.props.onFilterUpdate}
              key={index}
            />
          )
        })}
      </div>
    ) : null;
  }
}

export { FacetViewComponent };
