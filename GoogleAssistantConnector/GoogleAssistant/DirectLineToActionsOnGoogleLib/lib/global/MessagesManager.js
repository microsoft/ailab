// Constants
const DEFAULT = 'default';
const DEFAULT_EXIT_MESSAGE = 'Ok, bye!';

class MessagesManager {
    constructor() {
        this._exitTriggersMap;
        this._welcomeMessagesMap;
        this._startTriggersMap;
        this._exitMessagesMap;
        this._anythingElseMessagesMap;
    }

    initialize(messagesObj) {

        if (messagesObj) {
            if (messagesObj.welcomeMessages) {
                this._welcomeMessagesMap = new Map(messagesObj.welcomeMessages);
                console.log('welcomeMessages:');
                this._logMap(this._welcomeMessagesMap);
            }
            if (messagesObj.startTriggers) {
                this._startTriggersMap = new Map(messagesObj.startTriggers);
                console.log('startTriggers:');
                this._logMap(this._startTriggersMap);
            }
            if (messagesObj.anythingElseMessages) {
                this._anythingElseMessagesMap = new Map(messagesObj.anythingElseMessages);
                console.log('anythingElseMessages:');
                this._logMap(this._anythingElseMessagesMap);
            }
            if (messagesObj.exitMessages) {
                this._exitMessagesMap = new Map(messagesObj.exitMessages);
                console.log('exitMessages:');
                this._logMap(this._exitMessagesMap);
            }
            if (messagesObj.exitTriggers) {
                this._exitTriggersMap = new Map(messagesObj.exitTriggers);
                console.log('exitTriggers:');
                this._logMap(this._exitTriggersMap);
            }
        }
    }

    getWelcomeMessage(locale) {
        if (!this._welcomeMessagesMap)
            return undefined;

        let primaryLoc = this._getPrimaryLocale(locale);
        let messages = this._welcomeMessagesMap.get(primaryLoc);
        return this._getRandomMessage(messages);
    }

    getStartTrigger(locale) {
        if (!this._startTriggersMap)
            return undefined;

        let primaryLoc = this._getPrimaryLocale(locale);
        let messages = this._startTriggersMap.get(primaryLoc);
        return this._getRandomMessage(messages);
    }

    getAnythingElseMessage(locale) {
        if (!this._anythingElseMessagesMap)
            return undefined;

        let primaryLoc = this._getPrimaryLocale(locale);
        let messages = this._anythingElseMessagesMap.get(primaryLoc);
        return this._getRandomMessage(messages);
    }

    getExitMessage(locale) {
        if (!this._exitMessagesMap)
            return DEFAULT_EXIT_MESSAGE;

        let primaryLoc = this._getPrimaryLocale(locale);
        let messages = this._exitMessagesMap.get(primaryLoc);
        return this._getRandomMessage(messages);
    }

    getExitTriggers(locale) {
        if (!this._exitTriggersMap)
            return undefined;

        let primaryLoc = this._getPrimaryLocale(locale);
        return this._exitTriggersMap.get(primaryLoc) || this._exitTriggersMap.get(DEFAULT);
    }

    _getPrimaryLocale(locale) {
        return locale.split('-')[0];
    }

    _getRandomMessage(messages) {
        return messages[Math.floor(Math.random() * messages.length)];
    }

    _logMap(map) {
        //trace.log(`WelcomeMessages ${this._welcomeMessagesMap}`, this._welcomeMessagesMap);
        for (const [k, v] of map.entries())
            console.log(' '+ k, v);
    }
}


module.exports = new MessagesManager();