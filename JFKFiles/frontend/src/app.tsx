import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { Reboot } from 'material-ui';
import { AppRouter } from './app.router';
import { MuiThemeProvider } from 'material-ui/styles';
import { theme } from "./theme";


ReactDOM.render(
  <MuiThemeProvider theme={theme}>
    <Reboot/>
    <AppRouter/>
  </MuiThemeProvider>
  , document.getElementById('app')
);
