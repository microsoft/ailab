"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var React = require("react");
var react_dom_1 = require("react-dom");
var botframework_directlinejs_1 = require("botframework-directlinejs");
var Store_1 = require("./Store");
var react_redux_1 = require("react-redux");
var SpeechModule_1 = require("./SpeechModule");
var konsole = require("./Konsole");
var getTabIndex_1 = require("./getTabIndex");
var History_1 = require("./History");
var MessagePane_1 = require("./MessagePane");
var Shell_1 = require("./Shell");
var Chat = (function (_super) {
    tslib_1.__extends(Chat, _super);
    function Chat(props) {
        var _this = _super.call(this, props) || this;
        _this.store = Store_1.createStore();
        _this.resizeListener = function () { return _this.setSize(); };
        _this._handleCardAction = _this.handleCardAction.bind(_this);
        _this._handleKeyDownCapture = _this.handleKeyDownCapture.bind(_this);
        _this._saveChatviewPanelRef = _this.saveChatviewPanelRef.bind(_this);
        _this._saveHistoryRef = _this.saveHistoryRef.bind(_this);
        _this._saveShellRef = _this.saveShellRef.bind(_this);
        konsole.log("BotChat.Chat props", props);
        _this.store.dispatch({
            type: 'Set_Locale',
            locale: props.locale || window.navigator["userLanguage"] || window.navigator.language || 'en'
        });
        if (props.adaptiveCardsHostConfig) {
            _this.store.dispatch({
                type: 'Set_AdaptiveCardsHostConfig',
                payload: props.adaptiveCardsHostConfig
            });
        }
        var chatTitle = props.chatTitle;
        if (props.formatOptions) {
            console.warn('DEPRECATED: "formatOptions.showHeader" is deprecated, use "chatTitle" instead. See https://github.com/Microsoft/BotFramework-WebChat/blob/master/CHANGELOG.md#formatoptionsshowheader-is-deprecated-use-chattitle-instead.');
            if (typeof props.formatOptions.showHeader !== 'undefined' && typeof props.chatTitle === 'undefined') {
                chatTitle = props.formatOptions.showHeader;
            }
        }
        if (typeof chatTitle !== 'undefined') {
            _this.store.dispatch({ type: 'Set_Chat_Title', chatTitle: chatTitle });
        }
        _this.store.dispatch({ type: 'Toggle_Upload_Button', showUploadButton: props.showUploadButton !== false });
        if (props.sendTyping) {
            _this.store.dispatch({ type: 'Set_Send_Typing', sendTyping: props.sendTyping });
        }
        if (props.speechOptions) {
            SpeechModule_1.Speech.SpeechRecognizer.setSpeechRecognizer(props.speechOptions.speechRecognizer);
            SpeechModule_1.Speech.SpeechSynthesizer.setSpeechSynthesizer(props.speechOptions.speechSynthesizer);
        }
        return _this;
    }
    Chat.prototype.handleIncomingActivity = function (activity) {
        var state = this.store.getState();
        switch (activity.type) {
            case "message":
                this.store.dispatch({ type: activity.from.id === state.connection.user.id ? 'Receive_Sent_Message' : 'Receive_Message', activity: activity });
                break;
            case "typing":
                if (activity.from.id !== state.connection.user.id)
                    this.store.dispatch({ type: 'Show_Typing', activity: activity });
                break;
        }
    };
    Chat.prototype.setSize = function () {
        this.store.dispatch({
            type: 'Set_Size',
            width: this.chatviewPanelRef.offsetWidth,
            height: this.chatviewPanelRef.offsetHeight
        });
    };
    Chat.prototype.handleCardAction = function () {
        // After the user click on any card action, we will "blur" the focus, by setting focus on message pane
        // This is for after click on card action, the user press "A", it should go into the chat box
        var historyDOM = react_dom_1.findDOMNode(this.historyRef);
        if (historyDOM) {
            historyDOM.focus();
        }
    };
    Chat.prototype.handleKeyDownCapture = function (evt) {
        var target = evt.target;
        var tabIndex = getTabIndex_1.getTabIndex(target);
        if (evt.altKey
            || evt.ctrlKey
            || evt.metaKey
            || (!inputtableKey(evt.key) && evt.key !== 'Backspace')) {
            // Ignore if one of the utility key (except SHIFT) is pressed
            // E.g. CTRL-C on a link in one of the message should not jump to chat box
            // E.g. "A" or "Backspace" should jump to chat box
            return;
        }
        if (target === react_dom_1.findDOMNode(this.historyRef)
            || typeof tabIndex !== 'number'
            || tabIndex < 0) {
            evt.stopPropagation();
            var key = void 0;
            // Quirks: onKeyDown we re-focus, but the newly focused element does not receive the subsequent onKeyPress event
            //         It is working in Chrome/Firefox/IE, confirmed not working in Edge/16
            //         So we are manually appending the key if they can be inputted in the box
            if (/(^|\s)Edge\/16\./.test(navigator.userAgent)) {
                key = inputtableKey(evt.key);
            }
            this.shellRef.focus(key);
        }
    };
    Chat.prototype.saveChatviewPanelRef = function (chatviewPanelRef) {
        this.chatviewPanelRef = chatviewPanelRef;
    };
    Chat.prototype.saveHistoryRef = function (historyWrapper) {
        this.historyRef = historyWrapper && historyWrapper.getWrappedInstance();
    };
    Chat.prototype.saveShellRef = function (shellWrapper) {
        this.shellRef = shellWrapper && shellWrapper.getWrappedInstance();
    };
    Chat.prototype.componentDidMount = function () {
        var _this = this;
        // Now that we're mounted, we know our dimensions. Put them in the store (this will force a re-render)
        this.setSize();
        var botConnection = this.props.directLine
            ? (this.botConnection = new botframework_directlinejs_1.DirectLine(this.props.directLine))
            : this.props.botConnection;
        if (this.props.resize === 'window')
            window.addEventListener('resize', this.resizeListener);
        this.store.dispatch({ type: 'Start_Connection', user: this.props.user, bot: this.props.bot, botConnection: botConnection, selectedActivity: this.props.selectedActivity });
        this.connectionStatusSubscription = botConnection.connectionStatus$.subscribe(function (connectionStatus) {
            if (_this.props.speechOptions && _this.props.speechOptions.speechRecognizer) {
                var refGrammarId = botConnection.referenceGrammarId;
                if (refGrammarId)
                    _this.props.speechOptions.speechRecognizer.referenceGrammarId = refGrammarId;
            }
            _this.store.dispatch({ type: 'Connection_Change', connectionStatus: connectionStatus });
        });
        this.activitySubscription = botConnection.activity$.subscribe(function (activity) { return _this.handleIncomingActivity(activity); }, function (error) { return konsole.log("activity$ error", error); });
        if (this.props.selectedActivity) {
            this.selectedActivitySubscription = this.props.selectedActivity.subscribe(function (activityOrID) {
                _this.store.dispatch({
                    type: 'Select_Activity',
                    selectedActivity: activityOrID.activity || _this.store.getState().history.activities.find(function (activity) { return activity.id === activityOrID.id; })
                });
            });
        }
    };
    Chat.prototype.componentWillUnmount = function () {
        this.connectionStatusSubscription.unsubscribe();
        this.activitySubscription.unsubscribe();
        if (this.selectedActivitySubscription)
            this.selectedActivitySubscription.unsubscribe();
        if (this.botConnection)
            this.botConnection.end();
        window.removeEventListener('resize', this.resizeListener);
    };
    Chat.prototype.componentWillReceiveProps = function (nextProps) {
        if (this.props.adaptiveCardsHostConfig !== nextProps.adaptiveCardsHostConfig) {
            this.store.dispatch({
                type: 'Set_AdaptiveCardsHostConfig',
                payload: nextProps.adaptiveCardsHostConfig
            });
        }
        if (this.props.showUploadButton !== nextProps.showUploadButton) {
            this.store.dispatch({
                type: 'Toggle_Upload_Button',
                showUploadButton: nextProps.showUploadButton
            });
        }
        if (this.props.chatTitle !== nextProps.chatTitle) {
            this.store.dispatch({
                type: 'Set_Chat_Title',
                chatTitle: nextProps.chatTitle
            });
        }
    };
    // At startup we do three render passes:
    // 1. To determine the dimensions of the chat panel (nothing needs to actually render here, so we don't)
    // 2. To determine the margins of any given carousel (we just render one mock activity so that we can measure it)
    // 3. (this is also the normal re-render case) To render without the mock activity
    Chat.prototype.render = function () {
        var state = this.store.getState();
        konsole.log("BotChat.Chat state", state);
        // only render real stuff after we know our dimensions
        return (React.createElement(react_redux_1.Provider, { store: this.store },
            React.createElement("div", { className: "wc-chatview-panel", onKeyDownCapture: this._handleKeyDownCapture, ref: this._saveChatviewPanelRef },
                !!state.format.chatTitle &&
                    React.createElement("div", { className: "wc-header" },
                        React.createElement("span", null, typeof state.format.chatTitle === 'string' ? state.format.chatTitle : state.format.strings.title)),
                React.createElement(MessagePane_1.MessagePane, null,
                    React.createElement(History_1.History, { onCardAction: this._handleCardAction, ref: this._saveHistoryRef })),
                React.createElement(Shell_1.Shell, { ref: this._saveShellRef }),
                this.props.resize === 'detect' &&
                    React.createElement(ResizeDetector, { onresize: this.resizeListener }))));
    };
    return Chat;
}(React.Component));
exports.Chat = Chat;
exports.doCardAction = function (botConnection, from, locale, sendMessage) { return function (type, actionValue) {
    var text = (typeof actionValue === 'string') ? actionValue : undefined;
    var value = (typeof actionValue === 'object') ? actionValue : undefined;
    switch (type) {
        case "imBack":
            if (typeof text === 'string')
                sendMessage(text, from, locale);
            break;
        case "postBack":
            exports.sendPostBack(botConnection, text, value, from, locale);
            break;
        case "call":
        case "openUrl":
        case "playAudio":
        case "playVideo":
        case "showImage":
        case "downloadFile":
            window.open(text);
            break;
        case "signin":
            var loginWindow_1 = window.open();
            if (botConnection.getSessionId) {
                botConnection.getSessionId().subscribe(function (sessionId) {
                    konsole.log("received sessionId: " + sessionId);
                    loginWindow_1.location.href = text + encodeURIComponent('&code_challenge=' + sessionId);
                }, function (error) {
                    konsole.log("failed to get sessionId", error);
                });
            }
            else {
                loginWindow_1.location.href = text;
            }
            break;
        default:
            konsole.log("unknown button type", type);
    }
}; };
exports.sendPostBack = function (botConnection, text, value, from, locale) {
    botConnection.postActivity({
        type: "message",
        text: text,
        value: value,
        from: from,
        locale: locale
    })
        .subscribe(function (id) {
        konsole.log("success sending postBack", id);
    }, function (error) {
        konsole.log("failed to send postBack", error);
    });
};
exports.renderIfNonempty = function (value, renderer) {
    if (value !== undefined && value !== null && (typeof value !== 'string' || value.length > 0))
        return renderer(value);
};
exports.classList = function () {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        args[_i] = arguments[_i];
    }
    return args.filter(Boolean).join(' ');
};
// note: container of this element must have CSS position of either absolute or relative
var ResizeDetector = function (props) {
    // adapted to React from https://github.com/developit/simple-element-resize-detector
    return React.createElement("iframe", { style: { position: 'absolute', left: '0', top: '-100%', width: '100%', height: '100%', margin: '1px 0 0', border: 'none', opacity: 0, visibility: 'hidden', pointerEvents: 'none' }, ref: function (frame) {
            if (frame)
                frame.contentWindow.onresize = props.onresize;
        } });
};
// For auto-focus in some browsers, we synthetically insert keys into the chatbox.
// By default, we insert keys when:
// 1. evt.key.length === 1 (e.g. "1", "A", "=" keys), or
// 2. evt.key is one of the map keys below (e.g. "Add" will insert "+", "Decimal" will insert ".")
var INPUTTABLE_KEY = {
    Add: '+',
    Decimal: '.',
    Divide: '/',
    Multiply: '*',
    Subtract: '-' // Numpad subtract key
};
function inputtableKey(key) {
    return key.length === 1 ? key : INPUTTABLE_KEY[key];
}
//# sourceMappingURL=Chat.js.map