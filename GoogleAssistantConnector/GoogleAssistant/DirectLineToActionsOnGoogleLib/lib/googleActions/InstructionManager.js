const InstructionBuilder = require('./InstructionBuilder');
const RichMessageLimitationsManager = require('./RichMessageLimitationsManager');
const TextMapper = require('../mapper/TextMapper');
const BasicCardMapper = require('../mapper/BasicCardMapper');
const ListCardMapper = require('../mapper/ListCardMapper');
const CarouselCardMapper = require('../mapper/CarouselCardMapper');
const SuggestedActionsMapper = require('../mapper/SuggestedActionsMapper');
const MediaCardMapper = require('../mapper/MediaCardMapper');
const { instructionTypes } = require('../model/Instruction');

class InstructionManager {

    static buildInstructions(activities) {
        return activities.map(activity => InstructionBuilder.getFromActivity(activity));
    }

    static executeInstructions(instructions, conv) {
        let compactInstructions = RichMessageLimitationsManager.compact(instructions, conv.userLocale);

        var responses = new Array();
        var endConversation = false;

        compactInstructions.forEach(instruction => {
            switch (instruction.type) {
                case instructionTypes.basic:
                    responses = responses.concat(BasicCardMapper.map(instruction, conv.isScreen));
                    break;
                case instructionTypes.list:
                    responses = responses.concat(ListCardMapper.map(instruction, conv.isScreen));
                    break;
                case instructionTypes.linksCarousel:
                    responses = responses.concat(CarouselCardMapper.map(instruction, conv.isScreen));
                    break;
                case instructionTypes.media:
                    responses = responses.concat(MediaCardMapper.map(instruction, conv.isScreen));
                    break;
                case instructionTypes.endOfConversation:
                    endConversation = true;
                    break;
                case instructionTypes.text:
                    responses.push(TextMapper.map(instruction));
                    break;
                default:
            }

            if (instruction.suggestedActions)
                responses = responses.concat(SuggestedActionsMapper.map(instruction, conv.isScreen));
        });

        responses.forEach((response, i) => {
            if (endConversation && i === responses.length - 1)
                conv.close(response);
            else
                conv.ask(response);
        });

        return endConversation;
    }
};

module.exports = InstructionManager;