import { createMuiTheme } from 'material-ui/styles';

const defs = require("./main.scss");


export const theme = createMuiTheme({
  breakpoints: {
    values: {
      xs: parseInt(defs.breakpointXs),
      sm: parseInt(defs.breakpointSm),
      md: parseInt(defs.breakpointMd),
      lg: parseInt(defs.breakpointLg),
      xl: parseInt(defs.breakpointXl),
    },
  },
  palette: {
    common: {
      black: defs.colorBlack,
      white: defs.colorWhite,
    },
    primary: {
      main: defs.colorPrimary,
      dark: defs.colorPrimaryDark,
    },
    secondary: {
      main: defs.colorSecondary,
    },
    background: {
      default: defs.colorBackground,
      paper: defs.colorPaper,
    },
  },
  transitions: {
    duration: {
      shortest: 100,
      shorter: 125,
      short: 150,
      standard:200,
      complex: 225,
    }
  },
  typography: {
    fontFamily: '"Open Sans", sans-serif',
    fontSize: "1rem",
    fontWeightRegular: 300,
  }
});
