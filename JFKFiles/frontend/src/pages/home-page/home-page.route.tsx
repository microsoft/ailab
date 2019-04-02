import * as React from 'react';
import { Route } from 'react-router';
import { HomePageContainer } from './home-page.container';

export const homePath = "/";

export const HomeRoute = (
  <Route exact={true} path={homePath} component={HomePageContainer} />
);