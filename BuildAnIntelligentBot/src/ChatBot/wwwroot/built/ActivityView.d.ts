/// <reference types="react" />
import * as React from 'react';
import { Activity } from 'botframework-directlinejs';
import { FormatState, SizeState } from './Store';
import { IDoCardAction } from './Chat';
export interface ActivityViewProps {
    format: FormatState;
    size: SizeState;
    activity: Activity;
    onCardAction: IDoCardAction;
    onImageLoad: () => void;
}
export declare class ActivityView extends React.Component<ActivityViewProps, {}> {
    constructor(props: ActivityViewProps);
    shouldComponentUpdate(nextProps: ActivityViewProps): boolean;
    render(): JSX.Element;
}
