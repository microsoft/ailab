"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
function activityWithSuggestedActions(activities) {
    if (!activities || activities.length === 0) {
        return;
    }
    var lastActivity = activities[activities.length - 1];
    if (lastActivity.type === 'message'
        && lastActivity.suggestedActions
        && lastActivity.suggestedActions.actions.length > 0) {
        return lastActivity;
    }
}
exports.activityWithSuggestedActions = activityWithSuggestedActions;
//# sourceMappingURL=activityWithSuggestedActions.js.map