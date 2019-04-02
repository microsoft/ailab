import * as React from 'react';
import paginator from 'paginator';
import { Page } from './page';

// based on: https://github.com/vayser/react-js-pagination
const style = require('./pagination.scss');

interface Props {
  totalItemsCount: number;
  onChange: (pageNumber: number) => void;
  activePage?: number;
  itemsCountPerPage?: number;
  pageRangeDisplayed?: number;
  prevPageText?: (string | React.ReactNode);  // Not sure if element makes sense here
  nextPageText?: (string | React.ReactNode);
  lastPageText?: (string | React.ReactNode);
  firstPageText?: (string | React.ReactNode);  
  hideDisabled?: boolean;
  hideNavigation?: boolean,
    
  hideFirstLastPages?: boolean;  
}

export class Pagination extends React.Component<Props, {}> {
  static defaultProps = {
    itemsCountPerPage: 10,
    pageRangeDisplayed: 5,
    activePage: 1,
    prevPageText: "⟨",
    firstPageText: "⟨⟨",
    nextPageText: "⟩",
    lastPageText: "⟩⟩",
    hideFirstLastPages: false,
  };

  isFirstPageVisible(has_previous_page) {
    const { hideDisabled, hideNavigation, hideFirstLastPages } = this.props;
    return !hideNavigation && !hideFirstLastPages && !(hideDisabled && !has_previous_page);
  }

  isPrevPageVisible(has_previous_page) {
    const { hideDisabled, hideNavigation } = this.props;
    return !hideNavigation && !(hideDisabled && !has_previous_page);
  }

  isNextPageVisible(has_next_page) {
    const { hideDisabled, hideNavigation } = this.props;
    return !hideNavigation && !(hideDisabled && !has_next_page);
  }

  isLastPageVisible(has_next_page) {
    const { hideDisabled, hideNavigation, hideFirstLastPages } = this.props;
    return !hideNavigation && !hideFirstLastPages && !(hideDisabled && !has_next_page);
  }

  buildPages() {
    const pages = [];
    const {
      itemsCountPerPage,
      pageRangeDisplayed,
      activePage,
      prevPageText,
      nextPageText,
      firstPageText,
      lastPageText,
      totalItemsCount,
      onChange,
      hideFirstLastPages,
    } = this.props;

    const paginationInfo : any = new paginator(
      itemsCountPerPage,
      pageRangeDisplayed
    ).build(totalItemsCount, activePage);

    const buttonStyles = {
      root: style.button,
      raisedPrimary: style.primary,
    };

    for (
      let i = paginationInfo.first_page;
      i <= paginationInfo.last_page;
      i++
    ) {
      pages.push(
        <Page
          classes={buttonStyles}
          isActive={i === activePage}
          key={i}          
          pageNumber={i}
          pageText={i + ""}
          onClick={onChange}                                        
        />
      );
    }

    this.isPrevPageVisible(paginationInfo.has_previous_page) &&
      pages.unshift(
        <Page
          classes={buttonStyles}
          key={"prev" + paginationInfo.previous_page}
          pageNumber={paginationInfo.previous_page}
          onClick={onChange}
          pageText={prevPageText}
          isDisabled={!paginationInfo.has_previous_page}                    
        />
      );

    this.isFirstPageVisible(paginationInfo.has_previous_page) &&
      pages.unshift(
        <Page
          classes={buttonStyles}
          key={"first"}
          pageNumber={1}
          onClick={onChange}
          pageText={firstPageText}
          isDisabled={!paginationInfo.has_previous_page}                    
        />
      );

    this.isNextPageVisible(paginationInfo.has_next_page) &&
      pages.push(
        <Page
          classes={buttonStyles}
          key={"next" + paginationInfo.next_page}
          pageNumber={paginationInfo.next_page}
          onClick={onChange}
          pageText={nextPageText}
          isDisabled={!paginationInfo.has_next_page}                    
        />
      );

    this.isLastPageVisible(paginationInfo.has_next_page) &&
      pages.push(
        <Page
          classes={buttonStyles}
          key={"last"}
          pageNumber={paginationInfo.total_pages}
          onClick={onChange}
          pageText={lastPageText}
          isDisabled={
            paginationInfo.current_page === paginationInfo.total_pages
          }                              
        />
      );

    return pages;
  }

  render() {
    const pages = this.buildPages();
    return (
      <div className={style.pagination}>
        {pages}
      </div>
    );
  }
}
