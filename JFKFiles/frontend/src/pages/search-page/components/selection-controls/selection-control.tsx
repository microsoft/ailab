import * as React from "react";
import { Facet, Filter } from "../../view-model";
import { CheckboxListComponent } from "./checkbox-list.component";
import { YearPickerComponent } from "./year-picker.component";

export interface SelectionProps {
  facet: Facet;
  filter: Filter;
  onFilterUpdate: (newFilter: Filter) => void;
}

export const CreateSelectionControl = (facet: Facet, filter: Filter, onFilterUpdate) => {
  switch (facet.selectionControl) {
    case "checkboxList":
      return <CheckboxListComponent facet={facet} filter={filter} onFilterUpdate={onFilterUpdate} />
    case "yearPicker":
      return <YearPickerComponent facet={facet} filter={filter} onFilterUpdate={onFilterUpdate} />
    default:
      return JSON.stringify(facet.values);
  }
}