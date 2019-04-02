import * as React from 'react';
import { HashRouter, Switch, Route } from 'react-router-dom';
import { HomeRoute } from './pages/home-page';
import { SearchRoute } from './pages/search-page';
import { DetailRoute } from './pages/detail-page';

export class AppRouter extends React.Component {

  public componentDidMount() {
    // We just want to display the background image once all the app is ready
    // if not it just doesn't display.
    document.body.style.backgroundImage = 'url("../assets/img/bg.jpg")';
  }

  public render() {
    return (
      <HashRouter>
        <Switch>
          {HomeRoute}
          {SearchRoute}
          {DetailRoute}
        </Switch>
      </HashRouter>
    );
  }
}
