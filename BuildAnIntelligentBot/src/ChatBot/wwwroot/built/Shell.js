"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var React = require("react");
var Chat_1 = require("./Chat");
var react_redux_1 = require("react-redux");
var SpeechModule_1 = require("./SpeechModule");
var Store_1 = require("./Store");
var ShellContainer = (function (_super) {
    tslib_1.__extends(ShellContainer, _super);
    function ShellContainer() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    ShellContainer.prototype.sendMessage = function () {
        if (this.props.inputText.trim().length > 0) {
            this.props.sendMessage(this.props.inputText);
        }
    };
    ShellContainer.prototype.handleSendButtonKeyPress = function (evt) {
        if (evt.key === 'Enter' || evt.key === ' ') {
            evt.preventDefault();
            this.sendMessage();
            this.textInput.focus();
        }
    };
    ShellContainer.prototype.handleUploadButtonKeyPress = function (evt) {
        if (evt.key === 'Enter' || evt.key === ' ') {
            evt.preventDefault();
            this.fileInput.click();
        }
    };
    ShellContainer.prototype.onKeyPress = function (e) {
        if (e.key === 'Enter') {
            this.sendMessage();
        }
    };
    ShellContainer.prototype.onClickSend = function () {
        this.sendMessage();
    };
    ShellContainer.prototype.onChangeFile = function () {
        this.props.sendFiles(this.fileInput.files);
        this.fileInput.value = null;
        this.textInput.focus();
    };
    ShellContainer.prototype.onTextInputFocus = function () {
        if (this.props.listeningState === Store_1.ListeningState.STARTED) {
            this.props.stopListening();
        }
    };
    ShellContainer.prototype.onClickMic = function () {
        if (this.props.listeningState === Store_1.ListeningState.STARTED) {
            this.props.stopListening();
        }
        else if (this.props.listeningState === Store_1.ListeningState.STOPPED) {
            this.props.startListening();
        }
    };
    ShellContainer.prototype.focus = function (appendKey) {
        this.textInput.focus();
        if (appendKey) {
            this.props.onChangeText(this.props.inputText + appendKey);
        }
    };
    ShellContainer.prototype.render = function () {
        var _this = this;
        var className = Chat_1.classList('wc-console', this.props.inputText.length > 0 && 'has-text', this.props.showUploadButton && 'has-upload-button');
        var showMicButton = this.props.listeningState !== Store_1.ListeningState.STOPPED || (SpeechModule_1.Speech.SpeechRecognizer.speechIsAvailable() && !this.props.inputText.length);
        var sendButtonClassName = Chat_1.classList('wc-send', showMicButton && 'hidden');
        var micButtonClassName = Chat_1.classList('wc-mic', !showMicButton && 'hidden', this.props.listeningState === Store_1.ListeningState.STARTED && 'active', this.props.listeningState !== Store_1.ListeningState.STARTED && 'inactive');
        var placeholder = this.props.listeningState === Store_1.ListeningState.STARTED ? this.props.strings.listeningIndicator : this.props.strings.consolePlaceholder;
        return (React.createElement("div", { className: className },
            this.props.showUploadButton &&
                React.createElement("label", { className: "wc-upload", htmlFor: "wc-upload-input", onKeyPress: function (evt) { return _this.handleUploadButtonKeyPress(evt); }, tabIndex: 0 },
                    React.createElement("svg", null,
                        React.createElement("path", { d: "M19.96 4.79m-2 0a2 2 0 0 1 4 0 2 2 0 0 1-4 0zM8.32 4.19L2.5 15.53 22.45 15.53 17.46 8.56 14.42 11.18 8.32 4.19ZM1.04 1L1.04 17 24.96 17 24.96 1 1.04 1ZM1.03 0L24.96 0C25.54 0 26 0.45 26 0.99L26 17.01C26 17.55 25.53 18 24.96 18L1.03 18C0.46 18 0 17.55 0 17.01L0 0.99C0 0.45 0.47 0 1.03 0Z" }))),
            this.props.showUploadButton &&
                React.createElement("input", { id: "wc-upload-input", tabIndex: -1, type: "file", ref: function (input) { return _this.fileInput = input; }, multiple: true, onChange: function () { return _this.onChangeFile(); }, "aria-label": this.props.strings.uploadFile, role: "button" }),
            React.createElement("div", { className: "wc-textbox" },
                React.createElement("input", { type: "text", className: "wc-shellinput", ref: function (input) { return _this.textInput = input; }, autoFocus: true, value: this.props.inputText, onChange: function (_) { return _this.props.onChangeText(_this.textInput.value); }, onKeyPress: function (e) { return _this.onKeyPress(e); }, onFocus: function () { return _this.onTextInputFocus(); }, placeholder: placeholder, "aria-label": this.props.inputText ? null : placeholder, "aria-live": "polite" })),
            React.createElement("button", { className: sendButtonClassName, onClick: function () { return _this.onClickSend(); }, "aria-label": this.props.strings.send, role: "button", onKeyPress: function (evt) { return _this.handleSendButtonKeyPress(evt); }, tabIndex: 0, type: "button" },
                React.createElement("svg", null,
                    React.createElement("path", { d: "M26.79 9.38A0.31 0.31 0 0 0 26.79 8.79L0.41 0.02C0.36 0 0.34 0 0.32 0 0.14 0 0 0.13 0 0.29 0 0.33 0.01 0.37 0.03 0.41L3.44 9.08 0.03 17.76A0.29 0.29 0 0 0 0.01 17.8 0.28 0.28 0 0 0 0.01 17.86C0.01 18.02 0.14 18.16 0.3 18.16A0.3 0.3 0 0 0 0.41 18.14L26.79 9.38ZM0.81 0.79L24.84 8.79 3.98 8.79 0.81 0.79ZM3.98 9.37L24.84 9.37 0.81 17.37 3.98 9.37Z" }))),
            React.createElement("button", { className: micButtonClassName, onClick: function () { return _this.onClickMic(); }, "aria-label": this.props.strings.speak, role: "button", tabIndex: 0, type: "button" },
                React.createElement("svg", { width: "28", height: "22", viewBox: "0 0 58 58" },
                    React.createElement("path", { d: "M 44 28 C 43.448 28 43 28.447 43 29 L 43 35 C 43 42.72 36.72 49 29 49 C 21.28 49 15 42.72 15 35 L 15 29 C 15 28.447 14.552 28 14 28 C 13.448 28 13 28.447 13 29 L 13 35 C 13 43.485 19.644 50.429 28 50.949 L 28 56 L 23 56 C 22.448 56 22 56.447 22 57 C 22 57.553 22.448 58 23 58 L 35 58 C 35.552 58 36 57.553 36 57 C 36 56.447 35.552 56 35 56 L 30 56 L 30 50.949 C 38.356 50.429 45 43.484 45 35 L 45 29 C 45 28.447 44.552 28 44 28 Z" }),
                    React.createElement("path", { id: "micFilling", d: "M 28.97 44.438 L 28.97 44.438 C 23.773 44.438 19.521 40.033 19.521 34.649 L 19.521 11.156 C 19.521 5.772 23.773 1.368 28.97 1.368 L 28.97 1.368 C 34.166 1.368 38.418 5.772 38.418 11.156 L 38.418 34.649 C 38.418 40.033 34.166 44.438 28.97 44.438 Z" }),
                    React.createElement("path", { d: "M 29 46 C 35.065 46 40 41.065 40 35 L 40 11 C 40 4.935 35.065 0 29 0 C 22.935 0 18 4.935 18 11 L 18 35 C 18 41.065 22.935 46 29 46 Z M 20 11 C 20 6.037 24.038 2 29 2 C 33.962 2 38 6.037 38 11 L 38 35 C 38 39.963 33.962 44 29 44 C 24.038 44 20 39.963 20 35 L 20 11 Z" })))));
    };
    return ShellContainer;
}(React.Component));
exports.Shell = react_redux_1.connect(function (state) { return ({
    // passed down to ShellContainer
    inputText: state.shell.input,
    showUploadButton: state.format.showUploadButton,
    strings: state.format.strings,
    // only used to create helper functions below
    locale: state.format.locale,
    user: state.connection.user,
    listeningState: state.shell.listeningState
}); }, {
    // passed down to ShellContainer
    onChangeText: function (input) { return ({ type: 'Update_Input', input: input, source: "text" }); },
    stopListening: function () { return ({ type: 'Listening_Stopping' }); },
    startListening: function () { return ({ type: 'Listening_Starting' }); },
    // only used to create helper functions below
    sendMessage: Store_1.sendMessage,
    sendFiles: Store_1.sendFiles
}, function (stateProps, dispatchProps, ownProps) { return ({
    // from stateProps
    inputText: stateProps.inputText,
    showUploadButton: stateProps.showUploadButton,
    strings: stateProps.strings,
    listeningState: stateProps.listeningState,
    // from dispatchProps
    onChangeText: dispatchProps.onChangeText,
    // helper functions
    sendMessage: function (text) { return dispatchProps.sendMessage(text, stateProps.user, stateProps.locale); },
    sendFiles: function (files) { return dispatchProps.sendFiles(files, stateProps.user, stateProps.locale); },
    startListening: function () { return dispatchProps.startListening(); },
    stopListening: function () { return dispatchProps.stopListening(); }
}); }, {
    withRef: true
})(ShellContainer);
//# sourceMappingURL=Shell.js.map