"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var React = require("react");
var react_dom_1 = require("react-dom");
var react_redux_1 = require("react-redux");
var adaptivecards_1 = require("adaptivecards");
var Chat_1 = require("./Chat");
var adaptivecardsHostConfig = require("../adaptivecards-hostconfig.json");
var defaultHostConfig = new adaptivecards_1.HostConfig(adaptivecardsHostConfig);
function cardWithoutHttpActions(card) {
    if (!card.actions) {
        return card;
    }
    var nextActions = card.actions.reduce(function (nextActions, action) {
        // Filter out HTTP action buttons
        switch (action.type) {
            case 'Action.Submit':
                break;
            case 'Action.ShowCard':
                nextActions.push(tslib_1.__assign({}, action, { card: cardWithoutHttpActions(action.card) }));
                break;
            default:
                nextActions.push(action);
                break;
        }
        return nextActions;
    }, []);
    return tslib_1.__assign({}, card, { nextActions: nextActions });
}
var AdaptiveCardContainer = (function (_super) {
    tslib_1.__extends(AdaptiveCardContainer, _super);
    function AdaptiveCardContainer(props) {
        var _this = _super.call(this, props) || this;
        _this.handleImageLoad = _this.handleImageLoad.bind(_this);
        _this.onClick = _this.onClick.bind(_this);
        _this.saveDiv = _this.saveDiv.bind(_this);
        return _this;
    }
    AdaptiveCardContainer.prototype.saveDiv = function (divRef) {
        this.divRef = divRef;
    };
    AdaptiveCardContainer.prototype.onClick = function (e) {
        if (!this.props.onClick) {
            return;
        }
        //do not allow form elements to trigger a parent click event
        switch (e.target.tagName) {
            case 'A':
            case 'AUDIO':
            case 'VIDEO':
            case 'BUTTON':
            case 'INPUT':
            case 'LABEL':
            case 'TEXTAREA':
            case 'SELECT':
                break;
            default:
                this.props.onClick(e);
        }
    };
    AdaptiveCardContainer.prototype.onExecuteAction = function (action) {
        if (action instanceof adaptivecards_1.OpenUrlAction) {
            window.open(action.url);
        }
        else if (action instanceof adaptivecards_1.SubmitAction) {
            if (action.data !== undefined) {
                if (typeof action.data === 'object' && action.data.__isBotFrameworkCardAction) {
                    var cardAction = action.data;
                    this.props.onCardAction(cardAction.type, cardAction.value);
                }
                else {
                    this.props.onCardAction(typeof action.data === 'string' ? 'imBack' : 'postBack', action.data);
                }
            }
        }
    };
    AdaptiveCardContainer.prototype.componentDidMount = function () {
        this.mountAdaptiveCards();
    };
    AdaptiveCardContainer.prototype.componentDidUpdate = function (prevProps) {
        if (prevProps.hostConfig !== this.props.hostConfig
            || prevProps.jsonCard !== this.props.jsonCard
            || prevProps.nativeCard !== this.props.nativeCard) {
            this.unmountAdaptiveCards();
            this.mountAdaptiveCards();
        }
    };
    AdaptiveCardContainer.prototype.handleImageLoad = function () {
        this.props.onImageLoad && this.props.onImageLoad.apply(this, arguments);
    };
    AdaptiveCardContainer.prototype.unmountAdaptiveCards = function () {
        var divElement = react_dom_1.findDOMNode(this.divRef);
        [].forEach.call(divElement.children, function (child) { return divElement.removeChild(child); });
    };
    AdaptiveCardContainer.prototype.mountAdaptiveCards = function () {
        var _this = this;
        var adaptiveCard = this.props.nativeCard || new adaptivecards_1.AdaptiveCard();
        adaptiveCard.hostConfig = this.props.hostConfig || defaultHostConfig;
        var errors = [];
        if (!this.props.nativeCard && this.props.jsonCard) {
            this.props.jsonCard.version = this.props.jsonCard.version || '0.5';
            adaptiveCard.parse(cardWithoutHttpActions(this.props.jsonCard));
            errors = adaptiveCard.validate();
        }
        adaptiveCard.onExecuteAction = function (action) { return _this.onExecuteAction(action); };
        if (errors.length === 0) {
            var renderedCard = void 0;
            try {
                renderedCard = adaptiveCard.render();
            }
            catch (e) {
                var ve = {
                    error: -1,
                    message: e
                };
                errors.push(ve);
                if (e.stack) {
                    ve.message += '\n' + e.stack;
                }
            }
            if (renderedCard) {
                if (this.props.onImageLoad) {
                    var imgs = renderedCard.querySelectorAll('img');
                    if (imgs && imgs.length > 0) {
                        Array.prototype.forEach.call(imgs, function (img) {
                            img.addEventListener('load', _this.handleImageLoad);
                        });
                    }
                }
                react_dom_1.findDOMNode(this.divRef).appendChild(renderedCard);
                return;
            }
        }
        if (errors.length > 0) {
            console.log('Error(s) rendering AdaptiveCard:');
            errors.forEach(function (e) { return console.log(e.message); });
            this.setState({ errors: errors.map(function (e) { return e.message; }) });
        }
    };
    AdaptiveCardContainer.prototype.render = function () {
        var wrappedChildren;
        var hasErrors = this.state && this.state.errors && this.state.errors.length > 0;
        if (hasErrors) {
            wrappedChildren = (React.createElement("div", null,
                React.createElement("svg", { className: "error-icon", viewBox: "0 0 15 12.01" },
                    React.createElement("path", { d: "M7.62 8.63v-.38H.94a.18.18 0 0 1-.19-.19V.94A.18.18 0 0 1 .94.75h10.12a.18.18 0 0 1 .19.19v3.73H12V.94a.91.91 0 0 0-.07-.36 1 1 0 0 0-.5-.5.91.91 0 0 0-.37-.08H.94a.91.91 0 0 0-.37.07 1 1 0 0 0-.5.5.91.91 0 0 0-.07.37v7.12a.91.91 0 0 0 .07.36 1 1 0 0 0 .5.5.91.91 0 0 0 .37.08h6.72c-.01-.12-.04-.24-.04-.37z M11.62 5.26a3.27 3.27 0 0 1 1.31.27 3.39 3.39 0 0 1 1.8 1.8 3.36 3.36 0 0 1 0 2.63 3.39 3.39 0 0 1-1.8 1.8 3.36 3.36 0 0 1-2.62 0 3.39 3.39 0 0 1-1.8-1.8 3.36 3.36 0 0 1 0-2.63 3.39 3.39 0 0 1 1.8-1.8 3.27 3.27 0 0 1 1.31-.27zm0 6a2.53 2.53 0 0 0 1-.21A2.65 2.65 0 0 0 14 9.65a2.62 2.62 0 0 0 0-2 2.65 2.65 0 0 0-1.39-1.39 2.62 2.62 0 0 0-2 0A2.65 2.65 0 0 0 9.2 7.61a2.62 2.62 0 0 0 0 2A2.65 2.65 0 0 0 10.6 11a2.53 2.53 0 0 0 1.02.26zM13 7.77l-.86.86.86.86-.53.53-.86-.86-.86.86-.53-.53.86-.86-.86-.86.53-.53.86.86.86-.86zM1.88 7.13h2.25V4.88H1.88zm.75-1.5h.75v.75h-.75zM5.63 2.63h4.5v.75h-4.5zM1.88 4.13h2.25V1.88H1.88zm.75-1.5h.75v.75h-.75zM9 5.63H5.63v.75h2.64A4 4 0 0 1 9 5.63z" })),
                React.createElement("div", { className: "error-text" }, "Can't render card")));
        }
        else if (this.props.children) {
            wrappedChildren = (React.createElement("div", { className: "non-adaptive-content" }, this.props.children));
        }
        else {
            wrappedChildren = null;
        }
        return (React.createElement("div", { className: Chat_1.classList('wc-card', 'wc-adaptive-card', this.props.className, hasErrors && 'error'), onClick: this.onClick },
            wrappedChildren,
            React.createElement("div", { ref: this.saveDiv })));
    };
    return AdaptiveCardContainer;
}(React.Component));
exports.default = react_redux_1.connect(function (state) { return ({
    hostConfig: state.adaptiveCards.hostConfig
}); }, {}, function (stateProps, dispatchProps, ownProps) { return (tslib_1.__assign({}, ownProps, stateProps)); })(AdaptiveCardContainer);
//# sourceMappingURL=AdaptiveCardContainer.js.map