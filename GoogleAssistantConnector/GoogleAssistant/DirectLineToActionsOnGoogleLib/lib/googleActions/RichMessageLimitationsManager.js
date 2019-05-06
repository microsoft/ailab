const InstructionBuilder = require('./InstructionBuilder');
const MessagesManager = require('../global/MessagesManager');
const { instructionTypes } = require('../model/Instruction');
const { activityInputHintTypes } = require('../model/Activity');
const Trace = require('../global/Trace');

const cslPrefix = '[MAPPING]';

 class RichMessageLimitationsManager {

     static compact(instructions, userLocale) {

         let compactInstructions = new Array();

         let textItemsCount = 0;
         let isComplexCard = false;

         var tryToShowAnythingElseMessage = true;

         instructions.forEach(instruction => {
             let addInstruction = true;
             switch (instruction.type) {
                 case instructionTypes.text:
                     textItemsCount++;

                     if (instruction.inputHint === activityInputHintTypes.expectingInput ||
                         (instruction.text && (instruction.text.slice(-1) === '?' || instruction.text.includes('?'))))
                         tryToShowAnythingElseMessage = false;
                     break;
                 case instructionTypes.basic:
                 case instructionTypes.media:
                     if (isComplexCard) {
                         new Trace(null, cslPrefix).warning('Only one rich response per turn');
                         addInstruction = false;
                     }
                     else {
                         isComplexCard = true;
                         if (textItemsCount == 0 && instruction.text) {
                             textItemsCount++;
                             compactInstructions.push(InstructionBuilder.getFromText(instruction.text, instruction.speech));
                         }
                     }
                     break;
                 case instructionTypes.linksCarousel:
                 case instructionTypes.list:
                     const previousInstruction = compactInstructions.find(processedInst => processedInst.type === instruction.type);
                     if (previousInstruction) {
                         new Trace(null, cslPrefix).warning(`Only one rich response of type ${instruction.type} per turn. Mergin instructions`);
                         previousInstruction.merge(instruction);
                         addInstruction = false;
                     }
                     else {
                         if (instruction.type == instructionTypes.linksCarousel)
                             isComplexCard = true;
                         else
                             tryToShowAnythingElseMessage = false;

                         if (textItemsCount == 0 && instruction.text) {
                             textItemsCount++;
                             compactInstructions.push(InstructionBuilder.getFromText(instruction.text, instruction.speech));
                         }
                     }
                     break;
                 case instructionTypes.endOfConversation:
                     tryToShowAnythingElseMessage = false;
                     break;
                 default:
             }

             if (instruction.suggestedActions)
                 tryToShowAnythingElseMessage = false;

             if (addInstruction)
                 compactInstructions.push(instruction);
         });

         let maxTextItems = 2;

         if (tryToShowAnythingElseMessage) {
             let anythingElseMessage = MessagesManager.getAnythingElseMessage(userLocale);
             if (anythingElseMessage) {
                 compactInstructions.push(InstructionBuilder.getFromText(anythingElseMessage));
                 maxTextItems--;
             }
         }

         if (textItemsCount > maxTextItems) {
             let previousInstruction;
             for (var i = 0; i < compactInstructions.length; i++) {
                 if (textItemsCount == 0)
                     break;

                 let instruction = compactInstructions[i];
                 if (instruction.type === instructionTypes.text) {
                     textItemsCount--;
                     if (!previousInstruction) {
                         maxTextItems--;
                         if (maxTextItems == 0) previousInstruction = instruction;
                     }
                     else {
                         new Trace(null, cslPrefix).warning(`Too much text responses. Mergin text`);
                         previousInstruction.merge(instruction);
                         compactInstructions.splice(i, 1);
                         i--;
                     }
                 }
             }
         }

         return compactInstructions;
     }
};

module.exports = RichMessageLimitationsManager;
