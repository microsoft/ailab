/// <reference types="react" />
import * as React from 'react';
import { Activity } from 'botframework-directlinejs';
import { FormatState, SizeState } from './Store';
import { IDoCardAction } from './Chat';
export interface HistoryProps {
    format: FormatState;
    size: SizeState;
    activities: Activity[];
    hasActivityWithSuggestedActions: Activity;
    setMeasurements: (carouselMargin: number) => void;
    onClickRetry: (activity: Activity) => void;
    onClickCardAction: () => void;
    isFromMe: (activity: Activity) => boolean;
    isSelected: (activity: Activity) => boolean;
    onClickActivity: (activity: Activity) => React.MouseEventHandler<HTMLDivElement>;
    onCardAction: () => void;
    doCardAction: IDoCardAction;
}
export declare class HistoryView extends React.Component<HistoryProps, {}> {
    private scrollMe;
    private scrollContent;
    private scrollToBottom;
    private carouselActivity;
    private largeWidth;
    constructor(props: HistoryProps);
    componentWillUpdate(nextProps: HistoryProps): void;
    componentDidUpdate(): void;
    private autoscroll();
    private measurableCarousel;
    private doCardAction(type, value);
    render(): JSX.Element;
}
export declare const History: React.ComponentClass<any>;
export interface WrappedActivityProps {
    activity: Activity;
    showTimestamp: boolean;
    selected: boolean;
    fromMe: boolean;
    format: FormatState;
    onClickActivity: React.MouseEventHandler<HTMLDivElement>;
    onClickRetry: React.MouseEventHandler<HTMLAnchorElement>;
}
export declare class WrappedActivity extends React.Component<WrappedActivityProps, {}> {
    messageDiv: HTMLDivElement;
    constructor(props: WrappedActivityProps);
    render(): JSX.Element;
}
