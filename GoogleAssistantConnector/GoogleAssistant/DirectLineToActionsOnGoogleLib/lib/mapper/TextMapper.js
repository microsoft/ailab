const {
    SimpleResponse,
} = require('actions-on-google');

class TextMapper {
    static map(instruction) {
        return new SimpleResponse({
            speech: instruction.speech,
            text: instruction.text
        });
    }

    static mapText(text, speech = null) {
        return new SimpleResponse({
            speech: speech || text,
            text: text
        });
    }
}


module.exports = TextMapper;