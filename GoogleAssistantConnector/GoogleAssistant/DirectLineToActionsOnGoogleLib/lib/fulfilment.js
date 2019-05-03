'use strict';
require('./config/config');
//=========================================================
// Import modules
//=========================================================
const DirectLineManager = require('./directLine/DirectLineManager');
const ConversationWrapper = require('./googleActions/ConversationWrapper');
const InstructionManager = require('./googleActions/InstructionManager');
const ConversationQueue = require('./global/ConversationQueue');
const MessagesManager = require('./global/MessagesManager');
const Trace = require('./global/Trace');
const { Instruction, instructionTypes } = require('./model/Instruction');

const { actionssdk } = require('actions-on-google');
const appInsights = require('applicationinsights');

// Constants
const MAIN_INTENT = 'actions.intent.MAIN';
const TEXT_INTENT = 'actions.intent.TEXT';
const OPTION_INTENT = 'actions.intent.OPTION';
const CANCEL_INTENT = 'actions.intent.CANCEL';

const cslPrefix = '[ACTIONS ON GOOGLE]';

// Local variables
let directLineManager;      // Direct line
let trace;

// Initialize instances
const app = actionssdk();
if (process.env.APPINSIGHTS_INSTRUMENTATIONKEY)
    appInsights.setup().start();

//=========================================================
// Functions
//=========================================================
let fulfilment = (directlineSecret, messagesObj, router) => {
    console.log('------------------------------------------------------------------------------');
    console.log ('                     DirectLineToActionsOnGoogle');
    console.log('------------------------------------------------------------------------------');
    // Initialize messages manager
    MessagesManager.initialize(messagesObj);

    // Initialize Direct line
    directLineManager = new DirectLineManager(directlineSecret);
    ConversationQueue.globalJob = () => directLineManager.CheckTimeout();

    app.intent([MAIN_INTENT, TEXT_INTENT, OPTION_INTENT, CANCEL_INTENT],
        (conv, input, argument) => {
            trace= new Trace(conv, cslPrefix);
            return intentHandler(conv, input, argument);
        });

    app.catch((conv, e) => new Trace(conv, cslPrefix).error(e.message, e));

    router.use(app);
    console.log('------------------------------------------------------------------------------');
};

let intentHandler = async (conv, input, option) => {
    trace.log(`START ${conv.intent} ==========================`);
    let conversation = new ConversationWrapper(conv);
    let cancelConv = false;

    let dlClient = await directLineManager.getClient(conversation);

    switch (conversation.intent) {
        case MAIN_INTENT:
            let welcomeMessage = MessagesManager.getWelcomeMessage(conversation.userLocale);
            if (welcomeMessage) {
                conversation.ask(welcomeMessage);
            }
            else {
                input = MessagesManager.getStartTrigger(conversation.userLocale);
                await continueConversation(dlClient, conversation, input);
            }
            break;
        case OPTION_INTENT:
            input = option;
            await continueConversation(dlClient, conversation, input);
            break;
        case CANCEL_INTENT:
            cancelConv = true;
        case TEXT_INTENT:
            cancelConv ^= isCancelMessage(input, conversation.userLocale);
            if (!cancelConv)
                await continueConversation(dlClient, conversation, input);
            else
                cancelConversation(dlClient, conversation);
            break;
        default:
    };

    trace.log(`END ${conv.intent} ==========================`);
};

let continueConversation = (dlClient, conversation, input) => {
    return new Promise((resolve, reject) => {
        let responder = (activity) =>
            // Queue Bot Framework response (activity)
            ConversationQueue.addActivity(conversation.id, activity, activities => {
                // Job to process responses (activities) from Bot Framework

                // Build instructions from processed activities
                const instructions = InstructionManager.buildInstructions(activities);
                // Response to Google Action executing instructions
                var conversationClosed = InstructionManager.executeInstructions(instructions, conversation);

                if (conversationClosed) {
                    // close Bot Framework conversation
                    directLineManager.closeClient(dlClient);
                }
                else {
                    // continue conversation
                    dlClient.removeEventListener(responder);
                }

                // Required to end intentHandler
                resolve();
            });

        // Add event listener to Bot Framework responses process
        dlClient.addEventListener(responder);
        // Send user input to Bot Framework
        dlClient.sendMessage(input);
    });
};

let cancelConversation = (dlClient, conversation) => {
    // Close Bot Framework conversation
    directLineManager.closeClient(dlClient);

    // Build instructions to close Google Action conversation
    let exitMessage = MessagesManager.getExitMessage(conversation.userLocale);
    let exitInstructions = [new Instruction(), new Instruction(instructionTypes.endOfConversation)];
    exitInstructions[0].appendText(exitMessage);
    exitInstructions[0].appendSpeech(exitMessage);
    // Response to Google Action executing instructions
    InstructionManager.executeInstructions(exitInstructions, conversation);
};

let isCancelMessage = (input, locale) => {
    let exitTriggers = MessagesManager.getExitTriggers(locale);
    return (exitTriggers) ? exitTriggers.includes(input.toLowerCase()) : false;
}

module.exports = fulfilment;