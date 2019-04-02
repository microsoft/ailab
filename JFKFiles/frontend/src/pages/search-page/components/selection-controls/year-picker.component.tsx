import * as React from "react"
import { SelectionProps } from "./selection-control";
import { Facet, FacetValue, Filter } from "../../view-model";
import { DatePicker } from 'material-ui-pickers';
import { Moment } from "moment";

const style = require("./year-picker.style.scss");


class YearPickerComponent extends React.Component<SelectionProps, {}> {
  constructor(props) {
    super(props);
  }
  
  private getFilter = (): Filter => {
    if (this.props.filter) {
      return this.props.filter;
    } else {
      const newFilter = {
        fieldId: this.props.facet.fieldId,
        store: null,        
      };
      return newFilter;
    }
  }

  private handleChange = (newDate: Moment) => {
    const currentFilter = this.getFilter();
    this.props.onFilterUpdate({
      ...currentFilter,
      store: newDate,
    });  
  }
  
  public render() {
    return (
      <div className={style.container}>
        <DatePicker          
          clearable
          keyboard
          openToYearSelection
          disableFuture
          format="YYYY"
          // label="Choose a Year"
          invalidLabel="All"
          value={this.props.filter && this.props.filter.store ? this.props.filter.store : ""}
          onChange={this.handleChange}
          animateYearScrolling={true}
        />
      </div>
    );    
  }
};

export { YearPickerComponent };
