import * as React from "react";
import Divider from "material-ui/Divider";
import Hidden from "material-ui/Hidden";
import { PageBarComponent } from "./components/page-bar";
import { DrawerComponent } from "./components/drawer";
import { SearchComponent } from "./components/search";
import { ItemCollectionViewComponent } from "./components/item";
import { FacetViewComponent } from "./components/facets";
import { HorizontalSeparator } from "./../../common/components/horizontal-separator";
import { GraphViewComponent } from "./components/graph";
import { SpacerComponent } from "./components/spacer";
import {
  ItemCollection,
  FacetCollection,
  FilterCollection,
  Filter,
  SuggestionCollection,
  Item,
  ResultViewMode,
} from "./view-model";
import { Service } from "./service";
import { Pagination } from "../../common/components/pagination/pagination";
import { PlaceholderComponent } from "./components/placeholder";
import { FooterComponent } from "../../common/components/footer";

const style = require("./search-page.style.scss");


interface SearchPageProps {
  activeService: Service;
  showDrawer: boolean;
  resultViewMode: ResultViewMode;
  searchValue: string;
  itemCollection: ItemCollection;
  activeSearch?: string;
  targetWords: string[];
  facetCollection: FacetCollection;
  filterCollection: FilterCollection;
  suggestionCollection?: SuggestionCollection;
  resultCount: number;
  resultsPerPage: number;
  pageIndex: number;
  pulseToggle?: ResultViewMode;
  onSearchSubmit: () => void;
  onSearchUpdate: (value: string) => void;
  onFilterUpdate: (newFilter: Filter) => void;
  onItemClick: (item: Item) => void;
  onDrawerClose: () => void;
  onMenuClick: () => void;
  onLoadMore: (pageIndex: number) => void;
  onChangeResultViewMode: (newMode: ResultViewMode) => void;
  onGraphNodeDblClick: (searchValue: string) => void;
}

const DrawerAreaComponent = (props: SearchPageProps) => (
  <DrawerComponent
    className={style.drawerContainer}
    activeService={props.activeService}
    show={props.showDrawer}
    onMenuClick={props.onMenuClick}
    onClose={props.onDrawerClose}
  >
    <SearchComponent
      value={props.searchValue}
      onSearchSubmit={props.onSearchSubmit}
      onSearchUpdate={props.onSearchUpdate}
      suggestionCollection={props.suggestionCollection}
      resultCount={props.resultCount}
    />
    <FacetViewComponent
      facets={props.facetCollection}
      filters={props.filterCollection}
      onFilterUpdate={props.onFilterUpdate}
    />
  </DrawerComponent>
);

const handlePageChange = callback => pageNum => callback(pageNum - 1);

const Paginator = (props: Partial<SearchPageProps>) => (
  <>
    <Hidden smUp>
      {/* Mobile */}
      <Pagination
        activePage={props.pageIndex + 1}
        itemsCountPerPage={props.resultsPerPage}
        totalItemsCount={props.resultCount}
        pageRangeDisplayed={1}
        onChange={handlePageChange(props.onLoadMore)}
      />
    </Hidden>
    <Hidden xsDown>
      {/* Desktop */}
      <Pagination
        activePage={props.pageIndex + 1}
        itemsCountPerPage={props.resultsPerPage}
        totalItemsCount={props.resultCount}
        pageRangeDisplayed={5}
        onChange={handlePageChange(props.onLoadMore)}
      />
    </Hidden>
  </>
);

class ResultAreaComponent extends React.PureComponent<Partial<SearchPageProps>> {


  render() {
    return (
      <>
        <PlaceholderComponent />
        <SpacerComponent>
          {
            this.props.resultViewMode === "graph" ?
              <GraphViewComponent
                searchValue={this.props.activeSearch}
                onGraphNodeDblClick={this.props.onGraphNodeDblClick}
              /> :
              <div>
                <ItemCollectionViewComponent
                  items={this.props.itemCollection}
                  listMode={this.props.resultViewMode === "list"}
                  activeSearch={this.props.activeSearch}
                  targetWords={this.props.targetWords}
                  onClick={this.props.onItemClick}
                />
                <Paginator
                  pageIndex={this.props.pageIndex}
                  resultsPerPage={this.props.resultsPerPage}
                  resultCount={this.props.resultCount}
                  onLoadMore={this.props.onLoadMore}
                />
              </div>
          }
        </SpacerComponent>
      </>
    );
  }
}

const SearchPageComponent = (props: SearchPageProps) => (
  <div className={style.pageContainer}>
    <DrawerAreaComponent {...props} />
    <main className={style.mainContainer}>
      <PageBarComponent
        resultViewMode={props.resultViewMode}
        pulseToggle={props.pulseToggle}
        onChangeResultViewMode={props.onChangeResultViewMode}
        onMenuClick={props.onMenuClick}
      />
      <HorizontalSeparator />
      <ResultAreaComponent
        itemCollection={props.itemCollection}
        activeSearch={props.activeSearch}
        targetWords={props.targetWords}
        pageIndex={props.pageIndex}
        resultsPerPage={props.resultsPerPage}
        resultCount={props.resultCount}
        onItemClick={props.onItemClick}
        onLoadMore={props.onLoadMore}
        onGraphNodeDblClick={props.onGraphNodeDblClick}
        resultViewMode={props.resultViewMode}
      />
      <HorizontalSeparator />
      <FooterComponent />
    </main>
  </div>
)


export { SearchPageComponent };
