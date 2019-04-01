import * as React from "react";
import Downshift from "downshift";
import { MenuItem } from "material-ui/Menu";
import Paper from "material-ui/Paper";
import TextField, { TextFieldProps } from "material-ui/TextField";
import { SuggestionCollection, Suggestion } from "../../view-model";
import { cnc } from "../../../../util";

const style = require("./autocomplete.style.scss");


interface AutocompleteInputProps {
  type: string;
  name: string;
  id: string;
  searchValue: string;
  onSearchUpdate: (newValue: string) => void;
  onKeyPress?: (event) => void;
  suggestionCollection?: SuggestionCollection;
  placeholder?: string;
  autoFocus?: boolean;
  className?: string;
}

const renderInput = (params) => {
  const { innerInputProps, ...other } = params;
  return (
    <TextField
      {...other}
      InputProps={{
        ...innerInputProps,
        classes: {
          root: style.input,
          underline: style.underline,
        }
      }}
      spellCheck={false}
    />
  );
};

const renderSuggestionItem = (params) => {
  const { suggestion, index, composedProps, highlightedIndex, selectedItem } = params;
  const isHighlighted = highlightedIndex === index;
  const isSelected = selectedItem === suggestion.text;

  return (
    <MenuItem
      {...composedProps}
      classes={{ root: style.suggestionItem }}
      key={index}
      selected={isHighlighted}
      component="div"
    >
      {suggestion.text}
    </MenuItem>
  );
};

const renderSuggestionCollection = (params) => {
  const { suggestionCollection, getItemProps, isOpen, selectedItem, highlightedIndex } = params;
  if (isOpen && suggestionCollection && suggestionCollection.length) {
    return (
      <Paper square classes={{ root: style.dropdownArea }}>
        {suggestionCollection.map((suggestion, index) =>
          renderSuggestionItem({
            suggestion,
            index,
            composedProps: getItemProps({
              item: suggestion.text,
              index: index,
            }),
            highlightedIndex,
            selectedItem,
          })
        )}
      </Paper>
    );
  } else {
    return null;
  }
};

const handleItemToString = item => (item ? item.toString() : "");

const handleInputValueChange = props => newValue => {
  if (newValue !== props.searchValue) {
    props.onSearchUpdate(newValue);
  }
}

export const AutocompleteInputComponent: React.StatelessComponent<AutocompleteInputProps> = props => {
  return (
    <Downshift
      selectedItem={props.searchValue}
      onInputValueChange={handleInputValueChange(props)}
      itemToString={handleItemToString}
    >
      {({ getInputProps, getItemProps, isOpen, inputValue, selectedItem, highlightedIndex }) => {

        return (
          <div className={cnc(props.className, style.container)}>
            {renderInput({
              autoFocus: props.autoFocus,
              fullWidth: true,
              placeholder: props.placeholder,
              innerInputProps: getInputProps({
                type: props.type,
                name: props.name,
                id: props.id,
                onKeyDown: props.onKeyPress,
              }),
            })}
            {renderSuggestionCollection({
              suggestionCollection: props.suggestionCollection,
              getItemProps,
              isOpen,
              selectedItem,
              highlightedIndex,
            })}
          </div>
        );
      }}
    </Downshift>
  );
};
