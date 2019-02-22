using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Models;
using ChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ChatBot.Middlewares
{
    public class TranslatorSpeechMiddleware : IMiddleware
    {
        private readonly TranslatorSpeechService _translatorSpeechService;
        private readonly TranslatorTextService _translatorTextService;
        private readonly TextToSpeechService _textToSpeechService;

        public TranslatorSpeechMiddleware(string speechKey, string translatorKey, string region, IStatePropertyAccessor<ReservationData> reservationStateAccessor)
        {
            ReservationStateAccessor = reservationStateAccessor ?? throw new ArgumentNullException(nameof(reservationStateAccessor));
            if (string.IsNullOrEmpty(speechKey))
            {
                throw new ArgumentNullException(nameof(speechKey));
            }

            if (string.IsNullOrEmpty(translatorKey))
            {
                throw new ArgumentNullException(nameof(translatorKey));
            }

            this._translatorTextService = new TranslatorTextService(translatorKey);
            this._translatorSpeechService = new TranslatorSpeechService(speechKey, region);
            this._textToSpeechService = new TextToSpeechService();
        }

        public IStatePropertyAccessor<ReservationData> ReservationStateAccessor { get; }

        public virtual async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                IMessageActivity message = context.Activity.AsMessageActivity();
                if (message != null)
                {
                    await TranslateMessageAsync(context, message, cancellationToken, true).ConfigureAwait(false);

                    context.OnSendActivities(async (newContext, activities, nextSend) =>
                    {
                        // Translate messages sent to the user to user language
                        List<Task> tasks = new List<Task>();
                        foreach (Activity currentActivity in activities.Where(a => a.Type == ActivityTypes.Message))
                        {
                            tasks.Add(TranslateMessageAsync(newContext, currentActivity.AsMessageActivity(), cancellationToken));
                        }

                        if (tasks.Any())
                            await Task.WhenAll(tasks).ConfigureAwait(false);

                        return await nextSend();
                    });

                    context.OnUpdateActivity(async (newContext, activity, nextUpdate) =>
                    {
                        // Translate messages sent to the user to user language
                        if (activity.Type == ActivityTypes.Message)
                        {
                            await TranslateMessageAsync(newContext, activity.AsMessageActivity(), cancellationToken).ConfigureAwait(false);
                        }

                        return await nextUpdate();
                    });
                }
            }

            await next(cancellationToken);
        }

        private async Task TranslateMessageAsync(ITurnContext turnContext, IMessageActivity message, CancellationToken cancellationToken, bool receivingMessage = false)
        {
            var text = message.Text;
            var audioUrl = GetAudioUrl(turnContext.Activity);
            var state = await ReservationStateAccessor.GetAsync(turnContext, () => new ReservationData(), cancellationToken);
            var conversationLanguage = receivingMessage ? turnContext.Activity.Locale : state.ConversationLanguage ?? BotConstants.EnglishLanguage;

            if (string.IsNullOrEmpty(state.ConversationLanguage) || !conversationLanguage.Equals(state.ConversationLanguage))
            {
                state.ConversationLanguage = conversationLanguage;
            }

            // Skip translation if the source language is already English
            if (!conversationLanguage.Contains(BotConstants.EnglishLanguage))
            {
                // STT target language will be English for this lab
                if (!string.IsNullOrEmpty(audioUrl) && string.IsNullOrEmpty(text))
                {
                    var transcript = await this._translatorSpeechService.SpeechToTranslatedTextAsync(audioUrl, conversationLanguage, BotConstants.EnglishLanguage);
                    if (transcript != null)
                    {
                        text = transcript.Translation;
                    }
                }
                else
                {
                    // Use TTS translation
                    text = await _translatorTextService.Translate(BotConstants.EnglishLanguage, conversationLanguage, message.Text);
                    turnContext.Activity.Text = text;

                    var ssml = _textToSpeechService.GenerateSsml(text, conversationLanguage);
                    await SendAudioResponseAsync(turnContext, text, ssml);
                }
            }

            message.Text = text;
        }

        private string GetAudioUrl(Activity activity)
        {
            var regex = new Regex(BotConstants.ValidAudioContentTypes, RegexOptions.IgnoreCase);

            var attachment = ((List<Attachment>)activity?.Attachments)?
              .FirstOrDefault(item => regex.Matches(item.ContentType).Count > 0);

            return attachment?.ContentUrl;
        }

        private async Task SendAudioResponseAsync(ITurnContext context, string message, string ssml)
        {
            var audioMsg = context.Activity.CreateReply();
            audioMsg.Type = "PlayAudio";
            audioMsg.Text = message;
            audioMsg.Speak = ssml;

            await context.SendActivityAsync(audioMsg);
        }
    }
}
