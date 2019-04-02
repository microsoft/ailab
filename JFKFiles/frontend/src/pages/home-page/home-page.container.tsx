import * as React from "react";
import { RouteComponentProps } from "react-router";
import { HomePageComponent } from "./home-page.component";
import { searchPath } from "../search-page";
var qs= require('qs');

interface HomePageState {
  searchValue: string;
}

export class HomePageContainer extends React.Component<RouteComponentProps<any>, HomePageState> {
  constructor(props) {
    super(props);

    this.state = {
      searchValue: "oswald",
    }
  }
  
  private handleSearchSubmit = () => {        
    const params = qs.stringify({term: this.state.searchValue});

    this.props.history.push({
      pathname: searchPath,
      search: `?${params}`
    });
    
  };

  private handleSearchUpdate = (newSearch: string) => {
    this.setState({...this.state, searchValue: newSearch});
  };

  public render() {
    return (
      <HomePageComponent
        searchValue={this.state.searchValue}
        onSearchSubmit={this.handleSearchSubmit}
        onSearchUpdate={this.handleSearchUpdate}
      />
    )
  }
};
