//=========================================================
// Import modules
//=========================================================
const DirectLineWrapper = require('./DirectLineWrapper');
const Trace = require('../global/Trace');
const fetch = require('node-fetch');

const generateTokenUrl = `https://${process.env.DIRECTLINE_ENDPOINT}/v3/directline/tokens/generate`;

const cslPrefix = '[BOT FRAMEWORK]';

class DirectLineManager {
    constructor(directlineSecret) {
        this._secret = directlineSecret;
        this._clients = new Map();
    }

    async getClient(conversation) {
        // Get an instance of the directline client
        let client = this._clients.get(conversation.id);

        if (!client) {
            // Create a conversation
            client = await this._createConversation(conversation);
            // Store in client storage
            this._clients.set(client.googleActionsConversationId, client);
        }
        return client;
    }

    closeClient(client) {
        client.endConversation();

        // Remove the directline client from the list
        this._clients.delete(client.googleActionsConversationId);
        new Trace(null, cslPrefix).log(`Remove client (${this._clients.size} continue to be active) `);
    }

    CheckTimeout() {
        let mapIter = this._clients.values();
        let client = mapIter.next().value; // Only check the first item (older item )
        if (client) {
            // Get time delta from state's last interaction (ms)
            const delta = new Date() - client.lastInteraction;

            if (delta > process.env.CONVERSATION_TIMEOUT * 1000) {
                // End the conversation
                this.closeClient(client);
            }
        }
    }
    
    async  _createConversation(conversation) {
        let token = await this._getToken(this._secret);
        return new DirectLineWrapper(token, conversation);
        new Trace(null, cslPrefix).log(`Added client (${_clients.size} continue to be active) `);
    }

    async _getToken(directlineSecret) {
        let response = await fetch(generateTokenUrl, {
            method: 'POST',
            headers: { Authorization: `Bearer ${directlineSecret}` }
        });
        let json = await response.json();
        return json.token;
    }
}

module.exports = DirectLineManager;