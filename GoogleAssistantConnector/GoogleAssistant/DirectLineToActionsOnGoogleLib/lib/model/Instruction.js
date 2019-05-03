const TextHelper = require("../global/TextHelper");

const instructionTypes = {
    'text': 'text',
    'endOfConversation': 'endOfConversation',
    'basic': 'basic',
    'list': 'list',
    'linksCarousel': 'linksCarousel',
    'media': 'media'
};

class Instruction {

    constructor(type = instructionTypes.text) {
        this.type = type;
        this.inputHint;
        this.signInRequired = false;
        this._text;
        this._speech;
        this.speak;
        this.cards = new Array();
        this._suggestedActions;
    }
    get text() {
        return this._text || this._speech;
    }

    get speech() {
        return (this._speech && this._speech.includes('<')) ?
            `<speak>${this._speech}</speak>` :
            this._speech || this._text;
    }

    get suggestedActions() {
        return this._suggestedActions;
    }

    set suggestedActions(actions) {
        this._suggestedActions = actions;
    }

    appendText(newText) {
        if (!newText || newText === '')
            return;

        newText = TextHelper.cleanText(newText, false);

        if (this._text) {
            let requiresBreak = this._text.slice(-1) != '.';
            this._text = (requiresBreak) ?
                `${this._text}. ${newText}` :
                `${this._text} ${newText}`;
        } else {
            this._text = newText;
        }
    }

    appendSpeech(newSpeech) {
        if (!newSpeech || newSpeech === '')
            return;

        newSpeech = TextHelper.cleanText(newSpeech, false);

        this._speech = (this._speech) ?
            `${this._speech}<break time=\"2\" />${newSpeech}` : newSpeech;
    }

    appendCard(newCard) {
        this.cards.push(newCard);
    }

    appendCards(newCards) {
        this.cards = this.cards.concat(newCards);
    }

    merge(instruction) {
        this.type = instruction.type;
        this.appendText(instruction._text);
        this.appendSpeech(instruction._speech);
        this.signInRequired ^= instruction.signInRequired;
        this.appendCards(instruction.cards);
    }
}

module.exports = { Instruction, instructionTypes };