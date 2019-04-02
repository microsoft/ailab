import * as React from "react"
import { SelectionProps } from "./selection-control";
import { FormControlLabel } from "material-ui/Form";
import { Facet, FacetValue, Filter } from "../../view-model";
import Checkbox from "material-ui/Checkbox";
import { isValueInArray, addValueToArray, removeValueFromArray } from "../../../../util";

const style = require("./checkbox-list.style.scss");


class CheckboxListComponent extends React.Component<SelectionProps, {}> {
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

  private handleChange = (facetValue) => (event) => {    
    const currentFilter = this.getFilter();
    const newCheckedList = (event.target.checked) ? 
      addValueToArray(currentFilter.store, facetValue) : 
      removeValueFromArray(currentFilter.store, facetValue);
    this.props.onFilterUpdate({
      ...currentFilter,
      store: newCheckedList.length ? newCheckedList : null,
    });
  }
  
  private isValueInFilterList = (facetValue): boolean => {
    if (this.props.filter && this.props.filter.store){
      return isValueInArray(this.props.filter.store, facetValue);
    } else {
      return false;
    }    
  }

  private getCheckboxLabel = (labelValue: string, labelCount: number) => {
    return (
      <>
        {labelValue}
        <span className={style.highlight}>{` (${labelCount.toString()})`}</span>
      </>
    )
  }

  private getCheckbox = (facetValue) => (
    <Checkbox
      classes={{
        default: style.checkbox,
        checked: style.checkboxChecked,
      }}
      color="primary"
      value={facetValue.toString()}
      checked={this.isValueInFilterList(facetValue)}
      onChange={this.handleChange(facetValue)}
    />
  ); 
  
  private getCheckboxList = () => (
   this.props.facet.values.map((facetValue, index) => 
    <FormControlLabel
      classes={{label: style.label}}
      control={this.getCheckbox(facetValue.value)}
      label={this.getCheckboxLabel(facetValue.value, facetValue.count)}
      key={index}
    />
  ));

  public render() {
    return (
      <div className={style.container}>
        {this.getCheckboxList()}
      </div>
    );    
  }
};

export { CheckboxListComponent };