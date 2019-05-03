'use strict';
const { activityTypes, activityAttachmentTypes } = require('../model/Activity');
const { Instruction, instructionTypes } = require('../model/Instruction')
const TextHelper = require("../global/TextHelper");

class InstructionBuilder {
    static getFromText(text, speech = null) {
        let instruction = new Instruction();
        instruction.appendText(text);
        instruction.appendSpeech(speech);

        return instruction;
    }

    static getFromActivity(activity) {

        let type = (activity.type === activityTypes.endOfConversation) ?
            instructionTypes.endOfConversation : instructionTypes.text;

        let instruction = new Instruction(type);

        instruction.appendText(activity.text);
        instruction.appendSpeech(activity.speak);
        instruction.inputHint = activity.inputHint;

        if (activity.attachments && activity.attachments.length > 0) {
            if (activity.attachments.length == 1)
                this._processAttachment(instruction, activity.attachments[0]);
            else
                this._processCarouselAttachments(instruction, activity.attachments);
        }
        else if (activity.suggestedActions)
            instruction.suggestedActions = activity.suggestedActions.actions;

        return instruction;
    }

    static _processAttachment(instruction, attachment) {
        let content = attachment.content;
        switch (attachment.contentType) {
            case activityAttachmentTypes.SigninCard:
                this._fillSignIn(instruction, content); break;
            case activityAttachmentTypes.HeroCard:
            case activityAttachmentTypes.ThumbnailCard:
                let type = (content.images || content.image) ?
                    instructionTypes.basic :
                    instructionTypes.list;
                this._fillCard(instruction, content, type);
                break;
            case activityAttachmentTypes.AudioCard:
            case activityAttachmentTypes.VideoCard:
            case activityAttachmentTypes.AnimationCard:
                this._fillMediaCard(instruction, content); break;
            default:
                this._fillOtherCard(instruction, content); break;
        }
    }

    static _processCarouselAttachments(instruction, attachments) {
        attachments.forEach(attachment => {
            let content = attachment.content;
            switch (attachment.contentType) {
                case activityAttachmentTypes.HeroCard:
                case activityAttachmentTypes.ThumbnailCard:
                    if (content.images || content.image)
                        this._fillCard(instruction, content, instructionTypes.basic);
                    break;
                case activityAttachmentTypes.AudioCard:
                case activityAttachmentTypes.VideoCard:
                case activityAttachmentTypes.AnimationCard:
                    this._fillMediaCard(instruction, content); break;
                default:
            }
        });
        instruction.type = instructionTypes.linksCarousel;
    }

    static _fillSignIn(instruction, content) {
        instruction.signInRequired ^= true;
        this._fillCard(instruction, content, instructionTypes.basic);
    }

    static _fillCard(instruction, content, type) {

        let title = content.title;
        let subtitle = content.subtitle;
        let text = (content.text) ? TextHelper.htmlToMarkdown(content.text) : undefined;
        let images = content.images;

        if (!images && content.image)
            images = [content.image];
        let buttons = content.buttons;

        if (!instruction.text) {
            instruction.appendText(title || TextHelper.cleanMarkdown(text));
            instruction.appendSpeech(content.speak);
        }

        instruction.type = type;
        instruction.appendCard({ title, subtitle, text, images, buttons });
    }

    static _fillMediaCard(instruction, content) {
        let title = content.title;
        let subtitle = content.subtitle;
        let text = (content.text) ? TextHelper.htmlToMarkdown(content.text) : undefined;
        let images = content.images;

        if (!images && content.image)
            images = [content.image];
        let buttons = content.buttons;

        if (!instruction.text) {
            instruction.appendText(title);
            instruction.appendSpeech(content.speak);
        }

        let media = content.media;

        instruction.type = instructionTypes.media;
        instruction.appendCard({ title, subtitle, text, images, media, buttons });
    }

    static _fillOtherCard(instruction, content) {
        if (content.title)
            instruction.appendText(content.title);
        if (content.speak)
            instruction.appendSpeech(content.speak);
    }

 
};

module.exports = InstructionBuilder;