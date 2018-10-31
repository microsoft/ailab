"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var React = require("react");
var react_redux_1 = require("react-redux");
var ActivityView_1 = require("./ActivityView");
var Chat_1 = require("./Chat");
var konsole = require("./Konsole");
var Store_1 = require("./Store");
var activityWithSuggestedActions_1 = require("./activityWithSuggestedActions");
var HistoryView = (function (_super) {
    tslib_1.__extends(HistoryView, _super);
    function HistoryView(props) {
        var _this = _super.call(this, props) || this;
        _this.scrollToBottom = true;
        // In order to do their cool horizontal scrolling thing, Carousels need to know how wide they can be.
        // So, at startup, we create this mock Carousel activity and measure it.
        _this.measurableCarousel = function () {
            // find the largest possible message size by forcing a width larger than the chat itself
            return React.createElement(WrappedActivity, { ref: function (x) { return _this.carouselActivity = x; }, activity: {
                    type: 'message',
                    id: '',
                    from: { id: '' },
                    attachmentLayout: 'carousel'
                }, format: null, fromMe: false, onClickActivity: null, onClickRetry: null, selected: false, showTimestamp: false },
                React.createElement("div", { style: { width: _this.largeWidth } }, "\u00A0"));
        };
        return _this;
    }
    HistoryView.prototype.componentWillUpdate = function (nextProps) {
        var scrollToBottomDetectionTolerance = 1;
        if (!this.props.hasActivityWithSuggestedActions && nextProps.hasActivityWithSuggestedActions) {
            scrollToBottomDetectionTolerance = 40; // this should be in-sync with $actionsHeight scss var
        }
        this.scrollToBottom = (Math.abs(this.scrollMe.scrollHeight - this.scrollMe.scrollTop - this.scrollMe.offsetHeight) <= scrollToBottomDetectionTolerance);
    };
    HistoryView.prototype.componentDidUpdate = function () {
        if (this.props.format.carouselMargin == undefined) {
            // After our initial render we need to measure the carousel width
            // Measure the message padding by subtracting the known large width
            var paddedWidth = measurePaddedWidth(this.carouselActivity.messageDiv) - this.largeWidth;
            // offsetParent could be null if we start initially hidden
            var offsetParent = this.carouselActivity.messageDiv.offsetParent;
            if (offsetParent) {
                // Subtract the padding from the offsetParent's width to get the width of the content
                var maxContentWidth = offsetParent.offsetWidth - paddedWidth;
                // Subtract the content width from the chat width to get the margin.
                // Next time we need to get the content width (on a resize) we can use this margin to get the maximum content width
                var carouselMargin = this.props.size.width - maxContentWidth;
                konsole.log('history measureMessage ' + carouselMargin);
                // Finally, save it away in the Store, which will force another re-render
                this.props.setMeasurements(carouselMargin);
                this.carouselActivity = null; // After the re-render this activity doesn't exist
            }
        }
        this.autoscroll();
    };
    HistoryView.prototype.autoscroll = function () {
        var vAlignBottomPadding = Math.max(0, measurePaddedHeight(this.scrollMe) - this.scrollContent.offsetHeight);
        this.scrollContent.style.marginTop = vAlignBottomPadding + 'px';
        var lastActivity = this.props.activities[this.props.activities.length - 1];
        var lastActivityFromMe = lastActivity && this.props.isFromMe && this.props.isFromMe(lastActivity);
        // Validating if we are at the bottom of the list or the last activity was triggered by the user.
        if (this.scrollToBottom || lastActivityFromMe) {
            this.scrollMe.scrollTop = this.scrollMe.scrollHeight - this.scrollMe.offsetHeight;
        }
    };
    // At startup we do three render passes:
    // 1. To determine the dimensions of the chat panel (not much needs to actually render here)
    // 2. To determine the margins of any given carousel (we just render one mock activity so that we can measure it)
    // 3. (this is also the normal re-render case) To render without the mock activity
    HistoryView.prototype.doCardAction = function (type, value) {
        this.props.onClickCardAction();
        this.props.onCardAction && this.props.onCardAction();
        return this.props.doCardAction(type, value);
    };
    HistoryView.prototype.render = function () {
        var _this = this;
        konsole.log("History props", this);
        var content;
        if (this.props.size.width !== undefined) {
            if (this.props.format.carouselMargin === undefined) {
                // For measuring carousels we need a width known to be larger than the chat itself
                this.largeWidth = this.props.size.width * 2;
                content = React.createElement(this.measurableCarousel, null);
            }
            else {
                content = this.props.activities.map(function (activity, index) {
                    return (activity.type !== 'message' || activity.text || (activity.attachments && activity.attachments.length)) &&
                        React.createElement(WrappedActivity, { format: _this.props.format, key: 'message' + index, activity: activity, showTimestamp: index === _this.props.activities.length - 1 || (index + 1 < _this.props.activities.length && suitableInterval(activity, _this.props.activities[index + 1])), selected: _this.props.isSelected(activity), fromMe: _this.props.isFromMe(activity), onClickActivity: _this.props.onClickActivity(activity), onClickRetry: function (e) {
                                // Since this is a click on an anchor, we need to stop it
                                // from trying to actually follow a (nonexistant) link
                                e.preventDefault();
                                e.stopPropagation();
                                _this.props.onClickRetry(activity);
                            } },
                            React.createElement(ActivityView_1.ActivityView, { format: _this.props.format, size: _this.props.size, activity: activity, onCardAction: function (type, value) { return _this.doCardAction(type, value); }, onImageLoad: function () { return _this.autoscroll(); } }));
                });
            }
        }
        var groupsClassName = Chat_1.classList('wc-message-groups', !this.props.format.chatTitle && 'no-header');
        return (React.createElement("div", { className: groupsClassName, ref: function (div) { return _this.scrollMe = div || _this.scrollMe; }, role: "log", tabIndex: 0 },
            React.createElement("div", { className: "wc-message-group-content", ref: function (div) { if (div)
                    _this.scrollContent = div; } }, content)));
    };
    return HistoryView;
}(React.Component));
exports.HistoryView = HistoryView;
exports.History = react_redux_1.connect(function (state) { return ({
    // passed down to HistoryView
    format: state.format,
    size: state.size,
    activities: state.history.activities,
    hasActivityWithSuggestedActions: !!activityWithSuggestedActions_1.activityWithSuggestedActions(state.history.activities),
    // only used to create helper functions below
    connectionSelectedActivity: state.connection.selectedActivity,
    selectedActivity: state.history.selectedActivity,
    botConnection: state.connection.botConnection,
    user: state.connection.user
}); }, {
    setMeasurements: function (carouselMargin) { return ({ type: 'Set_Measurements', carouselMargin: carouselMargin }); },
    onClickRetry: function (activity) { return ({ type: 'Send_Message_Retry', clientActivityId: activity.channelData.clientActivityId }); },
    onClickCardAction: function () { return ({ type: 'Card_Action_Clicked' }); },
    // only used to create helper functions below
    sendMessage: Store_1.sendMessage
}, function (stateProps, dispatchProps, ownProps) { return ({
    // from stateProps
    format: stateProps.format,
    size: stateProps.size,
    activities: stateProps.activities,
    hasActivityWithSuggestedActions: stateProps.hasActivityWithSuggestedActions,
    // from dispatchProps
    setMeasurements: dispatchProps.setMeasurements,
    onClickRetry: dispatchProps.onClickRetry,
    onClickCardAction: dispatchProps.onClickCardAction,
    // helper functions
    doCardAction: Chat_1.doCardAction(stateProps.botConnection, stateProps.user, stateProps.format.locale, dispatchProps.sendMessage),
    isFromMe: function (activity) { return activity.from.id === stateProps.user.id; },
    isSelected: function (activity) { return activity === stateProps.selectedActivity; },
    onClickActivity: function (activity) { return stateProps.connectionSelectedActivity && (function () { return stateProps.connectionSelectedActivity.next({ activity: activity }); }); },
    onCardAction: ownProps.onCardAction
}); }, {
    withRef: true
})(HistoryView);
var getComputedStyleValues = function (el, stylePropertyNames) {
    var s = window.getComputedStyle(el);
    var result = {};
    stylePropertyNames.forEach(function (name) { return result[name] = parseInt(s.getPropertyValue(name)); });
    return result;
};
var measurePaddedHeight = function (el) {
    var paddingTop = 'padding-top', paddingBottom = 'padding-bottom';
    var values = getComputedStyleValues(el, [paddingTop, paddingBottom]);
    return el.offsetHeight - values[paddingTop] - values[paddingBottom];
};
var measurePaddedWidth = function (el) {
    var paddingLeft = 'padding-left', paddingRight = 'padding-right';
    var values = getComputedStyleValues(el, [paddingLeft, paddingRight]);
    return el.offsetWidth + values[paddingLeft] + values[paddingRight];
};
var suitableInterval = function (current, next) {
    return Date.parse(next.timestamp) - Date.parse(current.timestamp) > 5 * 60 * 1000;
};
var WrappedActivity = (function (_super) {
    tslib_1.__extends(WrappedActivity, _super);
    function WrappedActivity(props) {
        return _super.call(this, props) || this;
    }
    WrappedActivity.prototype.render = function () {
        var _this = this;
        var timeLine;
        switch (this.props.activity.id) {
            case undefined:
                timeLine = React.createElement("span", null, this.props.format.strings.messageSending);
                break;
            case null:
                timeLine = React.createElement("span", null, this.props.format.strings.messageFailed);
                break;
            case "retry":
                timeLine =
                    React.createElement("span", null,
                        this.props.format.strings.messageFailed,
                        ' ',
                        React.createElement("a", { href: ".", onClick: this.props.onClickRetry }, this.props.format.strings.messageRetry));
                break;
            default:
                var sent = void 0;
                if (this.props.showTimestamp)
                    sent = this.props.format.strings.timeSent.replace('%1', (new Date(this.props.activity.timestamp)).toLocaleTimeString());
                timeLine = React.createElement("span", null,
                    this.props.activity.from.name || this.props.activity.from.id,
                    sent);
                break;
        }
        var who = this.props.fromMe ? 'me' : 'bot';
        var wrapperClassName = Chat_1.classList('wc-message-wrapper', this.props.activity.attachmentLayout || 'list', this.props.onClickActivity && 'clickable');
        var contentClassName = Chat_1.classList('wc-message-content', this.props.selected && 'selected');
        return (React.createElement("div", { "data-activity-id": this.props.activity.id, className: wrapperClassName, onClick: this.props.onClickActivity },
            React.createElement("div", { className: 'wc-message wc-message-from-' + who, ref: function (div) { return _this.messageDiv = div; } },
                React.createElement("div", { className: contentClassName },
                    React.createElement("svg", { className: "wc-message-callout" },
                        React.createElement("path", { className: "point-left", d: "m0,6 l6 6 v-12 z" }),
                        React.createElement("path", { className: "point-right", d: "m6,6 l-6 6 v-12 z" })),
                    this.props.children)),
            React.createElement("div", { className: 'wc-message-from wc-message-from-' + who }, timeLine)));
    };
    return WrappedActivity;
}(React.Component));
exports.WrappedActivity = WrappedActivity;
//# sourceMappingURL=History.js.map