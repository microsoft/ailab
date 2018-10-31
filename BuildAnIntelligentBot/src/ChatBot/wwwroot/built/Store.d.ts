import { Activity, ConnectionStatus, IBotConnection, Message, User } from 'botframework-directlinejs';
import { Strings } from './Strings';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { ActivityOrID } from './Types';
import { HostConfig } from 'adaptivecards';
import { Reducer } from 'redux';
export declare enum ListeningState {
    STOPPED = 0,
    STARTING = 1,
    STARTED = 2,
    STOPPING = 3,
}
export declare const sendMessage: (text: string, from: User, locale: string) => ChatActions;
export declare const sendFiles: (files: FileList, from: User, locale: string) => ChatActions;
export interface ShellState {
    sendTyping: boolean;
    input: string;
    listeningState: ListeningState;
    lastInputViaSpeech: boolean;
}
export declare type ShellAction = {
    type: 'Update_Input';
    input: string;
    source: "text" | "speech";
} | {
    type: 'Listening_Starting';
} | {
    type: 'Listening_Start';
} | {
    type: 'Listening_Stopping';
} | {
    type: 'Listening_Stop';
} | {
    type: 'Stop_Speaking';
} | {
    type: 'Card_Action_Clicked';
} | {
    type: 'Set_Send_Typing';
    sendTyping: boolean;
} | {
    type: 'Send_Message';
    activity: Activity;
} | {
    type: 'Speak_SSML';
    ssml: string;
    locale: string;
    autoListenAfterSpeak: boolean;
};
export declare const shell: Reducer<ShellState>;
export interface FormatState {
    chatTitle: boolean | string;
    locale: string;
    showUploadButton: boolean;
    strings: Strings;
    carouselMargin: number;
}
export declare type FormatAction = {
    type: 'Set_Chat_Title';
    chatTitle: boolean | string;
} | {
    type: 'Set_Locale';
    locale: string;
} | {
    type: 'Set_Measurements';
    carouselMargin: number;
} | {
    type: 'Toggle_Upload_Button';
    showUploadButton: boolean;
};
export declare const format: Reducer<FormatState>;
export interface SizeState {
    height: number;
    width: number;
}
export declare type SizeAction = {
    type: 'Set_Size';
    width: number;
    height: number;
};
export declare const size: Reducer<SizeState>;
export interface ConnectionState {
    connectionStatus: ConnectionStatus;
    botConnection: IBotConnection;
    selectedActivity: BehaviorSubject<ActivityOrID>;
    user: User;
    bot: User;
}
export declare type ConnectionAction = {
    type: 'Start_Connection';
    botConnection: IBotConnection;
    user: User;
    bot: User;
    selectedActivity: BehaviorSubject<ActivityOrID>;
} | {
    type: 'Connection_Change';
    connectionStatus: ConnectionStatus;
};
export declare const connection: Reducer<ConnectionState>;
export interface HistoryState {
    activities: Activity[];
    clientActivityBase: string;
    clientActivityCounter: number;
    selectedActivity: Activity;
}
export declare type HistoryAction = {
    type: 'Receive_Message' | 'Send_Message' | 'Show_Typing' | 'Receive_Sent_Message';
    activity: Activity;
} | {
    type: 'Send_Message_Try' | 'Send_Message_Fail' | 'Send_Message_Retry';
    clientActivityId: string;
} | {
    type: 'Send_Message_Succeed';
    clientActivityId: string;
    id: string;
} | {
    type: 'Select_Activity';
    selectedActivity: Activity;
} | {
    type: 'Take_SuggestedAction';
    message: Message;
} | {
    type: 'Clear_Typing';
    id: string;
};
export declare const history: Reducer<HistoryState>;
export interface AdaptiveCardsState {
    hostConfig: HostConfig;
}
export declare type AdaptiveCardsAction = {
    type: 'Set_AdaptiveCardsHostConfig';
    payload: any;
};
export declare const adaptiveCards: Reducer<AdaptiveCardsState>;
export declare type ChatActions = ShellAction | FormatAction | SizeAction | ConnectionAction | HistoryAction | AdaptiveCardsAction;
export interface ChatState {
    adaptiveCards: AdaptiveCardsState;
    connection: ConnectionState;
    format: FormatState;
    history: HistoryState;
    shell: ShellState;
    size: SizeState;
}
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/delay';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/filter';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/merge';
import 'rxjs/add/operator/mergeMap';
import 'rxjs/add/operator/throttleTime';
import 'rxjs/add/operator/takeUntil';
import 'rxjs/add/observable/bindCallback';
import 'rxjs/add/observable/empty';
import 'rxjs/add/observable/of';
import { Store } from 'redux';
export declare const createStore: () => Store<ChatState>;
export declare type ChatStore = Store<ChatState>;
