//=========================================================
// Import modules
//=========================================================
const { DirectLine, ConnectionStatus } = require('botframework-directlinejs');
const { activityTypes } = require('../model/Activity');
const Trace  = require('../global/Trace');

// Direct Line js requeriment
global.XMLHttpRequest = require('xhr2');
global.WebSocket = require('ws');

const directLineUrl = `https://${process.env.DIRECTLINE_ENDPOINT}/v3/directline`;
const cslPrefix = '[BOT FRAMEWORK]';

class DirectLineWrapper {
    constructor(token, conversation) {
        this._conversation = conversation;
        this._trace = new Trace(conversation, cslPrefix);

        this._messagingCallbacks = [];
        this._lastInteraction = new Date();

        this._client = new DirectLine({
            token,
            webSocket: true,
            directLineUrl,
            pollingInterval: process.env.POLLING_INTERVAL,
            botAgent: this._conversation.directLineUserId
        });

        this._client.connectionStatus$
            .subscribe(status => this._trace.verbose(`Connection Status: ${ConnectionStatus[status]} [${this._client.conversationId}]`));

        this._client.activity$
            .filter(activity => (activity.type === activityTypes.message || activity.type === activityTypes.endOfConversation)
                && activity.from.id !== this._conversation.directLineUserId)
            .subscribe(this._activityHandler.bind(this));
    }

    _activityHandler(activity) {
        this._trace.log(`Received activity [${this._client.conversationId}]`, activity);
        this._messagingCallbacks.map(callback => callback(activity));
    }

    addEventListener(callback) {
        this._messagingCallbacks.push(callback);
    }

    removeEventListener(callback) {
        this._messagingCallbacks.pop(callback);
    }

    endConversation() {
        const activity = {
            type: activityTypes.endOfConversation,
            from: { id: this._conversation.directLineUserId }
        };

        this._postActivity(activity);
    }

    sendMessage(message) {
        if (!message)
            return;

        const activity = {
            from: {
                id: this._conversation.directLineUserId,
                name: this._conversation.user.displayName,
               // summary: this._conversation.user
            },
            type: activityTypes.message,
            channelData: {
                isVoiceAssistant: !this._conversation.isScreen,
                assistantName: 'googleAssistant'
            },
            text: message
        };

        this._postActivity(activity);
        this._lastInteraction = new Date();
    }

    _postActivity(activity) {
        this._trace.log(`Sending ${activity.type} activity [${this._client.conversationId}]`, activity);

        this._client
            .postActivity(activity)
            .subscribe(id => { }
            , error => this._trace.error(`Error posting ${activity.type} activity [${this._client.conversationId}]`, error));
    }

    get googleActionsConversationId() {
        return this._conversation.id;
    }

    get lastInteraction() {
        return this._lastInteraction;
    }
}

module.exports =  DirectLineWrapper;