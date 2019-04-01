import * as React from 'react';
import { Route } from 'react-router';
import { SearchPageContainer } from './search-page.container';

export const searchPath = "/search";

export const SearchRoute = (
  <Route path={searchPath} component={SearchPageContainer} />
);