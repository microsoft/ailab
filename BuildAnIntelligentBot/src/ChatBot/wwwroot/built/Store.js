"use strict";
var _this = this;
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var botframework_directlinejs_1 = require("botframework-directlinejs");
var Strings_1 = require("./Strings");
var SpeechModule_1 = require("./SpeechModule");
var adaptivecards_1 = require("adaptivecards");
var konsole = require("./Konsole");
var ListeningState;
(function (ListeningState) {
    ListeningState[ListeningState["STOPPED"] = 0] = "STOPPED";
    ListeningState[ListeningState["STARTING"] = 1] = "STARTING";
    ListeningState[ListeningState["STARTED"] = 2] = "STARTED";
    ListeningState[ListeningState["STOPPING"] = 3] = "STOPPING";
})(ListeningState = exports.ListeningState || (exports.ListeningState = {}));
exports.sendMessage = function (text, from, locale) { return ({
    type: 'Send_Message',
    activity: {
        type: "message",
        text: text,
        from: from,
        locale: locale,
        textFormat: 'plain',
        timestamp: (new Date()).toISOString()
    }
}); };
exports.sendFiles = function (files, from, locale) { return ({
    type: 'Send_Message',
    activity: {
        type: "message",
        attachments: attachmentsFromFiles(files),
        from: from,
        locale: locale
    }
}); };
var attachmentsFromFiles = function (files) {
    var attachments = [];
    for (var i = 0, numFiles = files.length; i < numFiles; i++) {
        var file = files[i];
        attachments.push({
            contentType: file.type,
            contentUrl: window.URL.createObjectURL(file),
            name: file.name
        });
    }
    return attachments;
};
exports.shell = function (state, action) {
    if (state === void 0) { state = {
        input: '',
        sendTyping: false,
        listeningState: ListeningState.STOPPED,
        lastInputViaSpeech: false
    }; }
    switch (action.type) {
        case 'Update_Input':
            return tslib_1.__assign({}, state, { input: action.input, lastInputViaSpeech: action.source == "speech" });
        case 'Listening_Start':
            return tslib_1.__assign({}, state, { listeningState: ListeningState.STARTED });
        case 'Listening_Stop':
            return tslib_1.__assign({}, state, { listeningState: ListeningState.STOPPED });
        case 'Listening_Starting':
            return tslib_1.__assign({}, state, { listeningState: ListeningState.STARTING });
        case 'Listening_Stopping':
            return tslib_1.__assign({}, state, { listeningState: ListeningState.STOPPING });
        case 'Send_Message':
            return tslib_1.__assign({}, state, { input: '' });
        case 'Set_Send_Typing':
            return tslib_1.__assign({}, state, { sendTyping: action.sendTyping });
        case 'Card_Action_Clicked':
            return tslib_1.__assign({}, state, { lastInputViaSpeech: false });
        default:
            return state;
    }
};
exports.format = function (state, action) {
    if (state === void 0) { state = {
        chatTitle: true,
        locale: 'en-us',
        showUploadButton: true,
        strings: Strings_1.defaultStrings,
        carouselMargin: undefined
    }; }
    switch (action.type) {
        case 'Set_Chat_Title':
            return tslib_1.__assign({}, state, { chatTitle: typeof action.chatTitle === 'undefined' ? true : action.chatTitle });
        case 'Set_Locale':
            return tslib_1.__assign({}, state, { locale: action.locale, strings: Strings_1.strings(action.locale) });
        case 'Set_Measurements':
            return tslib_1.__assign({}, state, { carouselMargin: action.carouselMargin });
        case 'Toggle_Upload_Button':
            return tslib_1.__assign({}, state, { showUploadButton: action.showUploadButton });
        default:
            return state;
    }
};
exports.size = function (state, action) {
    if (state === void 0) { state = {
        width: undefined,
        height: undefined
    }; }
    switch (action.type) {
        case 'Set_Size':
            return tslib_1.__assign({}, state, { width: action.width, height: action.height });
        default:
            return state;
    }
};
exports.connection = function (state, action) {
    if (state === void 0) { state = {
        connectionStatus: botframework_directlinejs_1.ConnectionStatus.Uninitialized,
        botConnection: undefined,
        selectedActivity: undefined,
        user: undefined,
        bot: undefined
    }; }
    switch (action.type) {
        case 'Start_Connection':
            return tslib_1.__assign({}, state, { botConnection: action.botConnection, user: action.user, bot: action.bot, selectedActivity: action.selectedActivity });
        case 'Connection_Change':
            return tslib_1.__assign({}, state, { connectionStatus: action.connectionStatus });
        default:
            return state;
    }
};
var copyArrayWithUpdatedItem = function (array, i, item) { return array.slice(0, i).concat([
    item
], array.slice(i + 1)); };
exports.history = function (state, action) {
    if (state === void 0) { state = {
        activities: [],
        clientActivityBase: Date.now().toString() + Math.random().toString().substr(1) + '.',
        clientActivityCounter: 0,
        selectedActivity: null
    }; }
    konsole.log("history action", action);
    switch (action.type) {
        case 'Receive_Sent_Message': {
            if (!action.activity.channelData || !action.activity.channelData.clientActivityId) {
                // only postBack messages don't have clientActivityId, and these shouldn't be added to the history
                return state;
            }
            var i_1 = state.activities.findIndex(function (activity) {
                return activity.channelData && activity.channelData.clientActivityId === action.activity.channelData.clientActivityId;
            });
            if (i_1 !== -1) {
                var activity_1 = state.activities[i_1];
                return tslib_1.__assign({}, state, { activities: copyArrayWithUpdatedItem(state.activities, i_1, activity_1), selectedActivity: state.selectedActivity === activity_1 ? action.activity : state.selectedActivity });
            }
            // else fall through and treat this as a new message
        }
        case 'Receive_Message':
            if (state.activities.find(function (a) { return a.id === action.activity.id; }))
                return state; // don't allow duplicate messages
            return tslib_1.__assign({}, state, { activities: state.activities.filter(function (activity) { return activity.type !== "typing"; }).concat([
                    action.activity
                ], state.activities.filter(function (activity) { return activity.from.id !== action.activity.from.id && activity.type === "typing"; })) });
        case 'Send_Message':
            return tslib_1.__assign({}, state, { activities: state.activities.filter(function (activity) { return activity.type !== "typing"; }).concat([
                    tslib_1.__assign({}, action.activity, { timestamp: (new Date()).toISOString(), channelData: { clientActivityId: state.clientActivityBase + state.clientActivityCounter } })
                ], state.activities.filter(function (activity) { return activity.type === "typing"; })), clientActivityCounter: state.clientActivityCounter + 1 });
        case 'Send_Message_Retry': {
            var activity_2 = state.activities.find(function (activity) {
                return activity.channelData && activity.channelData.clientActivityId === action.clientActivityId;
            });
            var newActivity_1 = activity_2.id === undefined ? activity_2 : tslib_1.__assign({}, activity_2, { id: undefined });
            return tslib_1.__assign({}, state, { activities: state.activities.filter(function (activityT) { return activityT.type !== "typing" && activityT !== activity_2; }).concat([
                    newActivity_1
                ], state.activities.filter(function (activity) { return activity.type === "typing"; })), selectedActivity: state.selectedActivity === activity_2 ? newActivity_1 : state.selectedActivity });
        }
        case 'Send_Message_Succeed':
        case 'Send_Message_Fail': {
            var i_2 = state.activities.findIndex(function (activity) {
                return activity.channelData && activity.channelData.clientActivityId === action.clientActivityId;
            });
            if (i_2 === -1)
                return state;
            var activity_3 = state.activities[i_2];
            if (activity_3.id && activity_3.id != "retry")
                return state;
            var newActivity_2 = tslib_1.__assign({}, activity_3, { id: action.type === 'Send_Message_Succeed' ? action.id : null });
            return tslib_1.__assign({}, state, { activities: copyArrayWithUpdatedItem(state.activities, i_2, newActivity_2), clientActivityCounter: state.clientActivityCounter + 1, selectedActivity: state.selectedActivity === activity_3 ? newActivity_2 : state.selectedActivity });
        }
        case 'Show_Typing':
            return tslib_1.__assign({}, state, { activities: state.activities.filter(function (activity) { return activity.type !== "typing"; }).concat(state.activities.filter(function (activity) { return activity.from.id !== action.activity.from.id && activity.type === "typing"; }), [
                    action.activity
                ]) });
        case 'Clear_Typing':
            return tslib_1.__assign({}, state, { activities: state.activities.filter(function (activity) { return activity.id !== action.id; }), selectedActivity: state.selectedActivity && state.selectedActivity.id === action.id ? null : state.selectedActivity });
        case 'Select_Activity':
            if (action.selectedActivity === state.selectedActivity)
                return state;
            return tslib_1.__assign({}, state, { selectedActivity: action.selectedActivity });
        case 'Take_SuggestedAction':
            var i = state.activities.findIndex(function (activity) { return activity === action.message; });
            var activity = state.activities[i];
            var newActivity = tslib_1.__assign({}, activity, { suggestedActions: undefined });
            return tslib_1.__assign({}, state, { activities: copyArrayWithUpdatedItem(state.activities, i, newActivity), selectedActivity: state.selectedActivity === activity ? newActivity : state.selectedActivity });
        default:
            return state;
    }
};
exports.adaptiveCards = function (state, action) {
    if (state === void 0) { state = {
        hostConfig: null
    }; }
    switch (action.type) {
        case 'Set_AdaptiveCardsHostConfig':
            return tslib_1.__assign({}, state, { hostConfig: action.payload && (action.payload instanceof adaptivecards_1.HostConfig ? action.payload : new adaptivecards_1.HostConfig(action.payload)) });
        default:
            return state;
    }
};
var nullAction = { type: null };
var speakFromMsg = function (msg, fallbackLocale) {
    var speak = msg.speak;
    if (!speak && msg.textFormat == null || msg.textFormat == "plain")
        speak = msg.text;
    if (!speak && msg.channelData && msg.channelData.speechOutput && msg.channelData.speechOutput.speakText)
        speak = msg.channelData.speechOutput.speakText;
    if (!speak && msg.attachments && msg.attachments.length > 0)
        for (var i = 0; i < msg.attachments.length; i++) {
            var anymsg = msg;
            if (anymsg.attachments[i]["content"] && anymsg.attachments[i]["content"]["speak"]) {
                speak = anymsg.attachments[i]["content"]["speak"];
                break;
            }
        }
    return {
        type: 'Speak_SSML',
        ssml: speak,
        locale: msg.locale || fallbackLocale,
        autoListenAfterSpeak: (msg.inputHint == "expectingInput") || (msg.channelData && msg.channelData.botState == "WaitingForAnswerToQuestion"),
    };
};
// Epics - chain actions together with async operations
var redux_1 = require("redux");
var Observable_1 = require("rxjs/Observable");
require("rxjs/add/operator/catch");
require("rxjs/add/operator/delay");
require("rxjs/add/operator/do");
require("rxjs/add/operator/filter");
require("rxjs/add/operator/map");
require("rxjs/add/operator/merge");
require("rxjs/add/operator/mergeMap");
require("rxjs/add/operator/throttleTime");
require("rxjs/add/operator/takeUntil");
require("rxjs/add/observable/bindCallback");
require("rxjs/add/observable/empty");
require("rxjs/add/observable/of");
var sendMessageEpic = function (action$, store) {
    return action$.ofType('Send_Message')
        .map(function (action) {
        var state = store.getState();
        var clientActivityId = state.history.clientActivityBase + (state.history.clientActivityCounter - 1);
        return { type: 'Send_Message_Try', clientActivityId: clientActivityId };
    });
};
var trySendMessageEpic = function (action$, store) {
    return action$.ofType('Send_Message_Try')
        .flatMap(function (action) {
        var state = store.getState();
        var clientActivityId = action.clientActivityId;
        var activity = state.history.activities.find(function (activity) { return activity.channelData && activity.channelData.clientActivityId === clientActivityId; });
        if (!activity) {
            konsole.log("trySendMessage: activity not found");
            return Observable_1.Observable.empty();
        }
        if (state.history.clientActivityCounter == 1) {
            var capabilities = {
                type: 'ClientCapabilities',
                requiresBotState: true,
                supportsTts: true,
                supportsListening: true,
            };
            activity.entities = activity.entities == null ? [capabilities] : activity.entities.concat([capabilities]);
        }
        return state.connection.botConnection.postActivity(activity)
            .map(function (id) { return ({ type: 'Send_Message_Succeed', clientActivityId: clientActivityId, id: id }); })
            .catch(function (error) { return Observable_1.Observable.of({ type: 'Send_Message_Fail', clientActivityId: clientActivityId }); });
    });
};
var speakObservable = Observable_1.Observable.bindCallback(SpeechModule_1.Speech.SpeechSynthesizer.speak);
var speakSSMLEpic = function (action$, store) {
    return action$.ofType('Speak_SSML')
        .filter(function (action) { return action.ssml; })
        .mergeMap(function (action) {
        var onSpeakingStarted = null;
        var onSpeakingFinished = function () { return nullAction; };
        if (action.autoListenAfterSpeak) {
            onSpeakingStarted = function () { return SpeechModule_1.Speech.SpeechRecognizer.warmup(); };
            onSpeakingFinished = function () { return ({ type: 'Listening_Starting' }); };
        }
        var call$ = speakObservable(action.ssml, action.locale, onSpeakingStarted);
        return call$.map(onSpeakingFinished)
            .catch(function (error) { return Observable_1.Observable.of(nullAction); });
    })
        .merge(action$.ofType('Speak_SSML').map(function (_) { return ({ type: 'Listening_Stopping' }); }));
};
var speakOnMessageReceivedEpic = function (action$, store) {
    return action$.ofType('Receive_Message')
        .filter(function (action) { return action.activity && store.getState().shell.lastInputViaSpeech; })
        .map(function (action) { return speakFromMsg(action.activity, store.getState().format.locale); });
};
var stopSpeakingEpic = function (action$) {
    return action$.ofType('Update_Input', 'Listening_Starting', 'Send_Message', 'Card_Action_Clicked', 'Stop_Speaking')
        .do(SpeechModule_1.Speech.SpeechSynthesizer.stopSpeaking)
        .map(function (_) { return nullAction; });
};
var stopListeningEpic = function (action$, store) {
    return action$.ofType('Listening_Stopping', 'Card_Action_Clicked')
        .do(function () { return tslib_1.__awaiter(_this, void 0, void 0, function () {
        return tslib_1.__generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, SpeechModule_1.Speech.SpeechRecognizer.stopRecognizing()];
                case 1:
                    _a.sent();
                    store.dispatch({ type: 'Listening_Stop' });
                    return [2 /*return*/];
            }
        });
    }); })
        .map(function (_) { return nullAction; });
};
var startListeningEpic = function (action$, store) {
    return action$.ofType('Listening_Starting')
        .do(function (action) { return tslib_1.__awaiter(_this, void 0, void 0, function () {
        var _a, activities, locale, lastMessageActivity, grammars, onIntermediateResult, onFinalResult, onAudioStreamStart, onRecognitionFailed;
        return tslib_1.__generator(this, function (_b) {
            switch (_b.label) {
                case 0:
                    _a = store.getState(), activities = _a.history.activities, locale = _a.format.locale;
                    lastMessageActivity = activities.slice().reverse().find(function (activity) { return activity.type === 'message'; });
                    grammars = lastMessageActivity && lastMessageActivity.listenFor;
                    onIntermediateResult = function (srText) { store.dispatch({ type: 'Update_Input', input: srText, source: 'speech' }); };
                    onFinalResult = function (srText) {
                        srText = srText.replace(/^[.\s]+|[.\s]+$/g, "");
                        onIntermediateResult(srText);
                        store.dispatch({ type: 'Listening_Stopping' });
                        store.dispatch(exports.sendMessage(srText, store.getState().connection.user, locale));
                    };
                    onAudioStreamStart = function () { store.dispatch({ type: 'Listening_Start' }); };
                    onRecognitionFailed = function () { store.dispatch({ type: 'Listening_Stopping' }); };
                    return [4 /*yield*/, SpeechModule_1.Speech.SpeechRecognizer.startRecognizing(locale, grammars, onIntermediateResult, onFinalResult, onAudioStreamStart, onRecognitionFailed)];
                case 1:
                    _b.sent();
                    return [2 /*return*/];
            }
        });
    }); })
        .map(function (_) { return nullAction; });
};
var listeningSilenceTimeoutEpic = function (action$, store) {
    var cancelMessages$ = action$.ofType('Update_Input', 'Listening_Stopping');
    return action$.ofType('Listening_Start')
        .mergeMap(function (action) {
        return Observable_1.Observable.of(({ type: 'Listening_Stopping' }))
            .delay(5000)
            .takeUntil(cancelMessages$);
    });
};
var retrySendMessageEpic = function (action$) {
    return action$.ofType('Send_Message_Retry')
        .map(function (action) { return ({ type: 'Send_Message_Try', clientActivityId: action.clientActivityId }); });
};
var updateSelectedActivityEpic = function (action$, store) {
    return action$.ofType('Send_Message_Succeed', 'Send_Message_Fail', 'Show_Typing', 'Clear_Typing')
        .map(function (action) {
        var state = store.getState();
        if (state.connection.selectedActivity)
            state.connection.selectedActivity.next({ activity: state.history.selectedActivity });
        return nullAction;
    });
};
var showTypingEpic = function (action$) {
    return action$.ofType('Show_Typing')
        .delay(3000)
        .map(function (action) { return ({ type: 'Clear_Typing', id: action.activity.id }); });
};
var sendTypingEpic = function (action$, store) {
    return action$.ofType('Update_Input')
        .map(function (_) { return store.getState(); })
        .filter(function (state) { return state.shell.sendTyping; })
        .throttleTime(3000)
        .do(function (_) { return konsole.log("sending typing"); })
        .flatMap(function (state) {
        return state.connection.botConnection.postActivity({
            type: 'typing',
            from: state.connection.user
        })
            .map(function (_) { return nullAction; })
            .catch(function (error) { return Observable_1.Observable.of(nullAction); });
    });
};
// Now we put it all together into a store with middleware
var redux_2 = require("redux");
var redux_observable_1 = require("redux-observable");
exports.createStore = function () {
    return redux_2.createStore(redux_2.combineReducers({
        adaptiveCards: exports.adaptiveCards,
        connection: exports.connection,
        format: exports.format,
        history: exports.history,
        shell: exports.shell,
        size: exports.size
    }), redux_1.applyMiddleware(redux_observable_1.createEpicMiddleware(redux_observable_1.combineEpics(updateSelectedActivityEpic, sendMessageEpic, trySendMessageEpic, retrySendMessageEpic, showTypingEpic, sendTypingEpic, speakSSMLEpic, speakOnMessageReceivedEpic, startListeningEpic, stopListeningEpic, stopSpeakingEpic, listeningSilenceTimeoutEpic))));
};
//# sourceMappingURL=Store.js.map