import * as React from "react";
import { withRouter, RouteComponentProps } from "react-router-dom";
import * as throttle from 'lodash.throttle';
import { parse } from "qs";
import { SearchPageComponent } from "./search-page.component";
import { State, FilterCollection, Filter, Item, ResultViewMode } from "./view-model";
import { Service, StateReducer } from "./service";
import { jfkService } from "./service";
import { isArrayEmpty, getUniqueStrings } from "../../util";
import {
  CreateInitialState,
  searchValueUpdate,
  showDrawerUpdate,
  suggestionsUpdate,
  preSearchUpdate,
  postSearchSuccessUpdate,
  postSearchMoreSuccessUpdate,
  postSearchErrorReset,
  postSearchErrorKeep,
  resultViewModeUpdate,
  receivedSearchValueUpdate,
} from "./search-page.container.state";
import { detailPath, DetailRouteState } from "../detail-page";
import { storeState, restoreLastState, isLastStateAvailable } from './view-model/state.memento';
import { setDetailState } from "../detail-page/detail-page.memento";
import { buildRoute } from "../../common/helpers/routes";

class SearchPageInnerContainer extends React.Component<RouteComponentProps<any>, State> {
  constructor(props) {
    super(props);

    this.state = CreateInitialState();
  }

  componentDidMount() {
    if (isLastStateAvailable()) {
      this.setState(restoreLastState());
    } else if (this.props.location.search) {
      const receivedSearchValue = parse(this.props.location.search.substring(1));
      this.handleReceivedSearchValue(receivedSearchValue.term);
    }
  }

  // *** Search Value received through query string ***

  private handleReceivedSearchValue = (searchValue: string) => {
    this.setState(
      receivedSearchValueUpdate(searchValue, true, "list", "graph"),
      this.handleSearchSubmit
    );
    this.schedulePulseOff();
  }

  private onGraphNodeDblClick = (value: string) => {
    const searchValue = `${this.state.searchValue} ${value}`;
    this.handleReceivedSearchValue(searchValue);
  }

  private schedulePulseOff = () => {
    setTimeout(() => {
      this.setState({
        ...this.state,
        pulseToggle: null,
      })
    }, 5100);
  } 

  // *** DRAWER LOGIC ***

  private handleDrawerClose = () => {
    this.setState(showDrawerUpdate(false));
  };

  private handleDrawerToggle = () => {
    this.setState(showDrawerUpdate(!this.state.showDrawer));
  };

  private handleMenuClick = () => {
    this.handleDrawerToggle();
  };

  // *** VIEW MODE LOGIC ***

  private handleResultViewMode = (resultViewMode: ResultViewMode) => {
    this.setState(resultViewModeUpdate(resultViewMode));
  }


  // *** SEARCH LOGIC ***

  private handleSearchSubmit = () => {
    this.setState(
      preSearchUpdate(null),
      this.runSearch(postSearchSuccessUpdate, postSearchErrorReset)
    );
  };

  private runSearch = (
    successCallback: (stateReducer: StateReducer) => (prevState: State) => State,
    errorCallback: (rejectValue) => (prevState: State) => State
  ) => () => {
    jfkService
      .search(this.state)
      .then(stateReducer => this.setState(successCallback(stateReducer)))
      .catch(rejectValue => this.setState(errorCallback(rejectValue)));
  }


  // *** FILTER LOGIC ***

  private updateFilterCollection = (newFilter: Filter) => {
    return (
      this.state.filterCollection ?
        [...this.state.filterCollection.filter(f => f.fieldId !== newFilter.fieldId), newFilter]
        : [newFilter])
      .filter(f => f.store);
  }

  private handleFilterUpdate = (newFilter: Filter) => {
    const newFilterCollection = this.updateFilterCollection(newFilter);
    this.setState(
      preSearchUpdate(newFilterCollection),
      this.runSearch(postSearchSuccessUpdate, postSearchErrorReset)
    );
  };


  // *** PAGINATION LOGIC ***

  private handleLoadMore = (pageIndex: number) => {
    this.setState(
      preSearchUpdate(this.state.filterCollection, pageIndex),
      this.runSearch(postSearchMoreSuccessUpdate, postSearchErrorKeep)
    );
  }


  // *** SUGGESTIONS LOGIC ***

  private handleSearchUpdate = (newValue: string) => {
    this.setState(searchValueUpdate(newValue), this.runSuggestions);
  };

  private runSuggestions = throttle(() => {
    jfkService
      .suggest(this.state)
      .then(stateReducer => this.setState(stateReducer<State>(this.state)))
      .catch(rejectValue => {
        console.debug(`Suggestions halted: ${rejectValue}`);
        this.setState(suggestionsUpdate(null));
      });
  }, 500, { leading: true, trailing: true });


  // *** MISC ***

  private handleOnItemClick = (item: Item) => {
    storeState(this.state);

    setDetailState({
      hocr: item.metadata,
      targetWords: getUniqueStrings([...this.state.targetWords, ...item.highlightWords]),
    } as DetailRouteState);

    const route = buildRoute(detailPath, { pageIndex: item.demoInitialPage });
    this.props.history.push(route);
  }

  // TODO: Snackbar implementation.
  private informMessage = (message: string) => {
    console.log(message);
  }

  // *** REACT LIFECYCLE ***

  public render() {
    return (
      <div>
        <SearchPageComponent
          activeService={jfkService}
          searchValue={this.state.searchValue}
          suggestionCollection={this.state.suggestionCollection}
          onSearchUpdate={this.handleSearchUpdate}
          onSearchSubmit={this.handleSearchSubmit}
          filterCollection={this.state.filterCollection}
          onFilterUpdate={this.handleFilterUpdate}
          itemCollection={this.state.itemCollection}
          activeSearch={this.state.activeSearch}
          targetWords={this.state.targetWords}
          onItemClick={this.handleOnItemClick}
          resultCount={this.state.resultCount}
          resultsPerPage={this.state.pageSize}
          pageIndex={this.state.pageIndex}
          pulseToggle={this.state.pulseToggle}
          facetCollection={this.state.facetCollection}
          onMenuClick={this.handleMenuClick}
          showDrawer={this.state.showDrawer}
          onDrawerClose={this.handleDrawerClose}
          onLoadMore={this.handleLoadMore}
          resultViewMode={this.state.resultViewMode}
          onChangeResultViewMode={this.handleResultViewMode}
          onGraphNodeDblClick={this.onGraphNodeDblClick}
        />
      </div>
    );
  }
}

export const SearchPageContainer = withRouter(SearchPageInnerContainer);
