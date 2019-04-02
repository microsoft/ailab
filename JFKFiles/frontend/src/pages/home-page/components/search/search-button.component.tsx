import * as React from "react"
import Button from "material-ui/Button";

const style = require("./search-button.style.scss");

export const SearchButton = ({ onClick }) => (
  <Button classes={{root: style.button}}
    variant="raised"
    size="small"
    color="secondary"
    onClick={onClick}
  >
    SEARCH
  </Button>
);