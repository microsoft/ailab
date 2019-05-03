const { activityButtonTypes } = require('../model/Activity');
const {
    List,
    Suggestions,
} = require('actions-on-google');

const cslPrefix = '[MAPPING]';

class SuggestedActionsMapper {

    static map(item, isScreen) {

        var responses = new Array();

        if (isScreen)
            responses.push(this._buildListCard(item.suggestedActions));
        return responses;
    }

    static _buildListCard(actions) {

        var suggestionsObj = {};
        this._processActions(actions, suggestionsObj);
        let suggestionKeys = Object.keys(suggestionsObj);

        return (suggestionKeys.length < 8) ?
            new Suggestions(suggestionKeys) :
            new List({ items: suggestionsObj });
    }

    static _processActions(actions, itemsObj) {
        if (!actions)
            return;
        actions.forEach(action => {
            switch (action.type) {
                case activityButtonTypes.imBack:
                case activityButtonTypes.postBack:
                    if (action.value.length > 20)
                        new Trace(null, cslPrefix).warning(`${action.type} button '${action.value}' too long!!`);

                    itemsObj[action.value] = ({ title: action.title });
                    break;
                default:
            }
        });
    }
}

module.exports = SuggestedActionsMapper;

