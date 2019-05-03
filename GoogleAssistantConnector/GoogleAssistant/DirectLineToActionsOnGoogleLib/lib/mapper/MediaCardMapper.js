const TextHelper = require('../global/TextHelper');
const Trace = require('../global/Trace');
const { activityButtonTypes } = require('../model/Activity');

const {
    BasicCard,
    Button,
    Image,
    Suggestions,
    LinkOutSuggestion,
    MediaObject
} = require('actions-on-google');

const cslPrefix = '[MAPPING]';

class MediaCardMapper {

    static map(item, isScreen) {

        var responses = new Array();

        if (isScreen && item.cards.length > 0 && item.cards[0].media) {
            let card = item.cards[0];
            let mediaUrl = card.media[0].url;

            let newResponses = (mediaUrl.slice(0, 5) === 'https' && mediaUrl.slice(-3) === 'mp3') ?
                this._buildBasicCard(card) :
                this._buildBasicCard(card);

            responses = responses.concat(newResponses);
        }

        return responses;
    }

    static _buildMediaCard(card) {
        let responses = new Array();

        let name = card.title;
        let description = card.subtitle || TextHelper.cleanMarkdown(card.text);
        let icon;
        if (card.images && card.images[0].url)
            icon = new Image({
                url: TextHelper.secureUrl(card.images[0].url),
                alt: card.images[0].alt || name || ' '
            });

        let suggestionsObj = {};
        let links = new Array();
        this._processButtons(card.buttons, suggestionsObj, null, true);

        let url = card.media[0].url;

        if (links.length > 0) {
            responses.push(new LinkOutSuggestion({
                name: links[0].title,
                url: links[0].url
            }));

            if (links.length > 1) {
                for (var i = 1; i < links.length; i++)
                    new Trace(null, cslPrefix).warning(`openUrl Buttons '${links[i].title}' not show (only can show 2).`);
            }
        }

        responses.push(new MediaObject({ name, url, description, icon }));

        let suggestionKeys = Object.keys(suggestionsObj);
        if (suggestionKeys.length > 0)
            responses.push(new Suggestions(suggestionKeys));

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
        if (card.media)
            buttons = new Button({ title, url: card.media[0].url });

        if (links.length > 0) {
            responses.push(new LinkOutSuggestion({
                name: links[0].title,
                url: links[0].url
            }));

            if (links.length > 1) {
                for (var i = 1; i < links.length; i++)
                    new Trace(null, cslPrefix).warning(`openUrl Buttons '${links[i].title}' not show (only can show 2).`);
            }
        }

        responses.push(new BasicCard({ text, subtitle, title, buttons, image, display: 'CROPPED' }));

        let suggestionKeys = Object.keys(suggestionsObj);
        if (suggestionKeys.length > 0)
            responses.push(new Suggestions(suggestionKeys));

        return responses;
    }

    static _processButtons(buttons, itemsObj, links, isSugestion = false) {
        if (!buttons)
            return;
        buttons.forEach(button => {
            switch (button.type) {
                case activityButtonTypes.imBack:
                case activityButtonTypes.postBack:
                    if (isSugestion && button.value.length > 20)
                        new Trace(null, cslPrefix).warning(`${button.type} button '${button.value}' too long!!`);
                    //   else
                    itemsObj[button.value] = ({ title: button.title });
                    break;
                case activityButtonTypes.openUrl:
                    if (links)
                        links.push({ title: button.title, url: button.value });
                    break;
                default:
            }
        });
    }
}

module.exports = MediaCardMapper;

