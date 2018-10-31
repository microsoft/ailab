import { Activity } from 'botframework-directlinejs';
export interface FormatOptions {
    showHeader?: boolean;
}
export declare type ActivityOrID = {
    activity?: Activity;
    id?: string;
};
