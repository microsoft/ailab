"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var adaptivecards_1 = require("adaptivecards");
var AdaptiveCardBuilder = (function () {
    function AdaptiveCardBuilder() {
        this.card = new adaptivecards_1.AdaptiveCard();
        this.container = new adaptivecards_1.Container();
        this.card.addItem(this.container);
    }
    AdaptiveCardBuilder.prototype.addColumnSet = function (sizes, container) {
        container = container || this.container;
        var columnSet = new adaptivecards_1.ColumnSet();
        container.addItem(columnSet);
        var columns = sizes.map(function (size) {
            var column = new adaptivecards_1.Column();
            column.width = size;
            columnSet.addColumn(column);
            return column;
        });
        return columns;
    };
    AdaptiveCardBuilder.prototype.addItems = function (cardElements, container) {
        container = container || this.container;
        cardElements.forEach(function (cardElement) { return container.addItem(cardElement); });
    };
    AdaptiveCardBuilder.prototype.addTextBlock = function (text, template, container) {
        container = container || this.container;
        if (typeof text !== 'undefined') {
            var textblock = new adaptivecards_1.TextBlock();
            for (var prop in template) {
                textblock[prop] = template[prop];
            }
            textblock.text = text;
            container.addItem(textblock);
        }
    };
    AdaptiveCardBuilder.prototype.addButtons = function (cardActions, includesOAuthButtons) {
        var _this = this;
        if (cardActions) {
            cardActions.forEach(function (cardAction) {
                _this.card.addAction(AdaptiveCardBuilder.addCardAction(cardAction, includesOAuthButtons));
            });
        }
    };
    AdaptiveCardBuilder.addCardAction = function (cardAction, includesOAuthButtons) {
        if (cardAction.type === 'imBack' || cardAction.type === 'postBack') {
            var action = new adaptivecards_1.SubmitAction();
            var botFrameworkCardAction = tslib_1.__assign({ __isBotFrameworkCardAction: true }, cardAction);
            action.data = botFrameworkCardAction;
            action.title = cardAction.title;
            return action;
        }
        else if (cardAction.type === 'signin' && includesOAuthButtons) {
            // Create a button specific for OAuthCard 'signin' actions (cardAction.type == signin and button action is Action.Submit)
            var action = new adaptivecards_1.SubmitAction();
            var botFrameworkCardAction = tslib_1.__assign({ __isBotFrameworkCardAction: true }, cardAction);
            action.data = botFrameworkCardAction;
            action.title = cardAction.title;
            return action;
        }
        else {
            var action = new adaptivecards_1.OpenUrlAction();
            var botFrameworkCardAction = tslib_1.__assign({ __isBotFrameworkCardAction: true }, cardAction);
            action.title = cardAction.title;
            action.url = cardAction.type === 'call' ? 'tel:' + cardAction.value : cardAction.value;
            return action;
        }
    };
    AdaptiveCardBuilder.prototype.addCommonHeaders = function (content) {
        this.addTextBlock(content.title, { size: adaptivecards_1.TextSize.Medium, weight: adaptivecards_1.TextWeight.Bolder });
        this.addTextBlock(content.subtitle, { isSubtle: true, wrap: true });
        this.addTextBlock(content.text, { wrap: true });
    };
    AdaptiveCardBuilder.prototype.addCommon = function (content) {
        this.addCommonHeaders(content);
        this.addButtons(content.buttons);
    };
    AdaptiveCardBuilder.prototype.addImage = function (url, container, selectAction) {
        container = container || this.container;
        var image = new adaptivecards_1.Image();
        image.url = url;
        image.size = adaptivecards_1.Size.Stretch;
        if (selectAction) {
            image.selectAction = AdaptiveCardBuilder.addCardAction(selectAction);
        }
        container.addItem(image);
    };
    return AdaptiveCardBuilder;
}());
exports.AdaptiveCardBuilder = AdaptiveCardBuilder;
exports.buildCommonCard = function (content) {
    if (!content)
        return null;
    var cardBuilder = new AdaptiveCardBuilder();
    cardBuilder.addCommon(content);
    return cardBuilder.card;
};
exports.buildOAuthCard = function (content) {
    if (!content)
        return null;
    var cardBuilder = new AdaptiveCardBuilder();
    cardBuilder.addCommonHeaders(content);
    cardBuilder.addButtons(content.buttons, true);
    return cardBuilder.card;
};
//# sourceMappingURL=CardBuilder.js.map