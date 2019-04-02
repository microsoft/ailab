import * as React from "react";
import Icon from "material-ui/Icon";

const style = require("./search-input.style.scss");

interface SearchInputProps {
  searchValue: string;
  onSearchUpdate: (newValue: string) => void;
  onSearchSubmit: () => void;
}

const handleOnChange = (onSearchUpdate) => (e) => {
  onSearchUpdate(e.target.value);
}

const captureEnter = (onSearchSubmit) => (e => {
  if (e.key === "Enter") {
    onSearchSubmit();
  }
});

export const SearchInput = (props: SearchInputProps) => (
  <div className={style.container}>
    <Icon classes={{root: style.icon}}>&#xe900;</Icon>
    <input
      className={style.input}
      type="text"
      placeholder="Search here..."
      value={props.searchValue}
      onChange={handleOnChange(props.onSearchUpdate)}
      onKeyPress={captureEnter(props.onSearchSubmit)}
      spellCheck={false}
      autoFocus
    />
  </div>
);