const { activityButtonTypes } = require('../model/Activity');
const { List } = require('actions-on-google');

const cslPrefix = '[MAPPING]';

class ListCardMapper {
    static map(item, isScreen) {

        var responses = new Array();

        if (isScreen)
            responses = responses.concat(this._buildListCard(item.cards[0]));
        return responses;
    }

    static _buildListCard(card) {
        var responses = new Array();

        var items = {};
        this._processButtons(card.buttons, items);

        if (Object.keys(items).length > 0)
            responses.push(new List({ title: card.text, items }));

        return responses
    }

    static _processButtons(buttons, itemsObj) {
        if (!buttons)
            return;
        buttons.forEach(button => {
            switch (button.type) {
                case activityButtonTypes.imBack:
                case activityButtonTypes.postBack:
                    itemsObj[button.value] = ({ title: button.title });
                    break;
                default:
            }
        });
    }
}

module.exports = ListCardMapper;