/// <reference types="react" />
import * as React from 'react';
import 'rxjs/add/observable/fromEvent';
import 'rxjs/add/observable/merge';
export interface HScrollProps {
    scrollUnit?: 'page' | 'item';
    prevSvgPathData: string;
    nextSvgPathData: string;
}
export declare class HScroll extends React.Component<HScrollProps, {}> {
    private prevButton;
    private nextButton;
    private scrollDiv;
    private animateDiv;
    private scrollStartTimer;
    private scrollSyncTimer;
    private scrollDurationTimer;
    private scrollSubscription;
    private clickSubscription;
    constructor(props: HScrollProps);
    private clearScrollTimers();
    updateScrollButtons(): void;
    componentDidMount(): void;
    componentDidUpdate(): void;
    componentWillUnmount(): void;
    private scrollAmount(direction);
    private scrollBy(direction);
    render(): JSX.Element;
}
