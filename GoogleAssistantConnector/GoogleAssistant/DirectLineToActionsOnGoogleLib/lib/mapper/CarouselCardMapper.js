const TextHelper = require('../global/TextHelper');
const Trace = require('../global/Trace');
const { activityButtonTypes } = require('../model/Activity');

const {
    Image,
    Suggestions,
    BrowseCarousel,
    BrowseCarouselItem
} = require('actions-on-google');

const cslPrefix = '[MAPPING]';

class CarouselCardMapper {

    static map(item, isScreen) {

        var responses = new Array();

        if (isScreen)
            responses = responses.concat(this._buildCarouselCard(item.cards));

        return responses;
    }

    static _buildCarouselCard(cards) {
        let responses = new Array();

        var items = new Array();
        let suggestionsObj = {};

        for (var i = 0; i < cards.length; i++) {
            if (i > 9) {
                new Trace(null, cslPrefix).warning('Too much attackments. Only can show 10');
                break;
            }
            let card = cards[i];

            let title = card.title;
            let description = TextHelper.cleanMarkdown(card.text);
           
            let image;
            if (card.images && card.images[0].url) {
                image = new Image({
                    url: TextHelper.secureUrl(card.images[0].url),
                    alt: card.images[0].alt || title || 'image'
                });
                if (!title)
                    title = card.images[0].alt;
            }

            let links = new Array();
            this._processButtons(card.buttons, suggestionsObj, links);

            let url = (!card.media) ?
                (links.length > 0) ?
                    links[0].url :
                    image.url :
                card.media[0].url;

            items.push(new BrowseCarouselItem({ title, description, url, image }));
        }

        responses.push(new BrowseCarousel({
            items: items,
            display: 'CROPPED'
        }));

        if (Object.keys(suggestionsObj).length > 0)
            responses.push(new Suggestions(Object.keys(suggestionsObj)));

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

module.exports = CarouselCardMapper;

