const TextHelper = require('../global/TextHelper');
const Trace = require('../global/Trace');
const { activityButtonTypes } = require('../model/Activity');

const {
    BasicCard,
    Button,
    Image,
    Suggestions,
    LinkOutSuggestion,
} = require('actions-on-google');

const cslPrefix = '[MAPPING]';

class BasicCardMapper {

    static map(item, isScreen) {

        var responses = new Array();

        if (isScreen)
            responses = responses.concat(this._buildBasicCard(item.cards[0]));

        return responses;
    }

    static _buildBasicCard(card) {
        let responses = new Array();

        let text = card.text;
        let title = card.title;
        let subtitle = card.subtitle;

        let image;
        if (card.images && card.images[0].url) {
            image = new Image({
                url: TextHelper.secureUrl(card.images[0].url),
                alt: card.images[0].alt || title || 'image'
            });
            if (!title)
                title = card.images[0].alt; 
        }

        let suggestionsObj = {};
        let links = new Array();
        this._processButtons(card.buttons, suggestionsObj, links, true);

        let buttons;

        if (links.length > 0) {
            // Process external links
            buttons = new Button({ title: links[0].title, url: links[0].url });

            if (links.length > 1) {
                responses.push(new LinkOutSuggestion({
                    name: links[1].title,
                    url: links[1].url
                }));
            }

            if (links.length > 2) {
                for (var i = 2; i < links.length; i++)
                    new Trace(null, cslPrefix).warning(`openUrl Buttons '${links[i].title}' not show (only can show 2).`);
            }
        }

        responses.push(new BasicCard({ text, subtitle, title, buttons, image, display: 'CROPPED' }));

        let suggestionKeys = Object.keys(suggestionsObj);
        if (suggestionKeys.length > 0)
            responses.push(new Suggestions(suggestionKeys));

        return responses;
    }

    static _processButtons(buttons, itemsObj, links) {
        if (!buttons)
            return;
        buttons.forEach(button => {
            switch (button.type) {
                case activityButtonTypes.imBack:
                case activityButtonTypes.postBack:
                    if (button.value.length > 20)
                        new Trace(null, cslPrefix).warning(`${button.type} button '${button.value}' too long!!`);
                    itemsObj[button.value] = ({ title: button.title });
                    break;
                case activityButtonTypes.openUrl:
                case activityButtonTypes.signin:
                    if (links)
                        links.push({ title: button.title, url: button.value });
                    break;
                default:
            }
        });
    }
}

module.exports = BasicCardMapper;

