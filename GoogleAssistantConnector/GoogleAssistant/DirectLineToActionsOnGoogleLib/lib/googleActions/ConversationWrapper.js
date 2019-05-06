//=========================================================
// Import modules
//=========================================================
const uuidv4 = require('uuid/v4');
const Trace = require('../global/Trace');

const cslPrefix = '[ACTIONS ON GOOGLE]';

class ConversationWrapper {
    constructor(conversation) {
        this._conv = conversation;
        this._console = new Trace(conversation, cslPrefix);

        this._console.log('Received request', this._conv.request);
    }

    ask(message) {
        this._console.log('Sending ask', message);
        return this._conv.ask(message);
    }

    close(message) {
        this._console.log('Sending close', message);
        return this._conv.close(message);
    }

    get id() {
        return this._conv.id;
    }

    get user() {
        return this._conv.user;
    }

    get userLocale() {
        return this._conv.user.locale;
    }

    get isScreen() {
        return this._conv.screen;
    }

    get directLineUserId() {
        let id = this._conv.user.storage.directLineUserId;
        if (!id) {
            id = uuidv4();
            this._conv.user.storage.directLineUserId = id;
        }
        return id;
    }

    get intent() {
        return this._conv.intent;
    }
}

module.exports = ConversationWrapper;
