"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var React = require("react");
var ReactDOM = require("react-dom");
var Chat_1 = require("./Chat");
var konsole = require("./Konsole");
exports.App = function (props, container) {
    konsole.log("BotChat.App props", props);
    ReactDOM.render(React.createElement(AppContainer, props), container);
};
var AppContainer = function (props) {
    return React.createElement("div", { className: "wc-app" },
        React.createElement(Chat_1.Chat, tslib_1.__assign({}, props)));
};
//# sourceMappingURL=App.js.map