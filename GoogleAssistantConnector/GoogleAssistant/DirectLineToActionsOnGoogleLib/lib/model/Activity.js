const activityTypes = {
    'message': 'message',
    'endOfConversation': 'endOfConversation'
};

const activityInputHintTypes = {
    'expectingInput':'expectingInput'
}

const activityButtonTypes = {
    'imBack': 'imBack',
    'postBack': 'postBack',
    'openUrl':'openUrl',
    'signin':'signin'
}

const activityAttachmentTypes = {
    'AnimationCard': 'application/vnd.microsoft.card.animation',
    'AudioCard': 'application/vnd.microsoft.card.audio',
    'HeroCard': 'application/vnd.microsoft.card.hero',
    'ReceiptCard': 'application/vnd.microsoft.com.card.receipt',
    'SigninCard': 'application/vnd.microsoft.card.signin',
    'ThumbnailCard': 'application/vnd.microsoft.card.thumbnail',
    'VideoCard': 'application/vnd.microsoft.card.video'
};

module.exports = { activityTypes, activityInputHintTypes, activityButtonTypes, activityAttachmentTypes };