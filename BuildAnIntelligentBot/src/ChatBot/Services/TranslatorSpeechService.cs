using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChatBot.Models;
using ChatBot.Utils;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.Extensions.Configuration;

namespace ChatBot.Services
{
    public class TranslatorSpeechService
  {

        private readonly string _region;
        private readonly string _subscriptionKey;

        public TranslatorSpeechService(string speechKey, string region)
        {
            _region = region;
            _subscriptionKey = speechKey;
        }

        private List<TranscriptUtterance> Transcripts { get; } = new List<TranscriptUtterance>();

        public async Task<TranscriptUtterance> SpeechToTranslatedTextAsync(string audioUrl, string sourceLanguage, string targetLanguage)
        {
            Transcripts.Clear();

            TranscriptUtterance utterance = null;

            var config = SpeechTranslationConfig.FromSubscription(_subscriptionKey, _region);
            config.SpeechRecognitionLanguage = sourceLanguage;

            config.AddTargetLanguage(targetLanguage);

            var stopTranslation = new TaskCompletionSource<int>();

            using (var audioInput = await AudioUtils.DownloadWavFileAsync(audioUrl))
            {
                using (var recognizer = new TranslationRecognizer(config, audioInput))
                {
                    // Subscribes to events.
                    recognizer.Recognized += (s, e) => {

                        if (e.Result.Reason == ResultReason.TranslatedSpeech)
                        {
                            utterance = new TranscriptUtterance
                            {
                                Recognition = e.Result.Text,
                                Translation = e.Result.Translations.FirstOrDefault().Value,
                            };
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Trace.TraceError($"NOMATCH: Speech could not be translated.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        if (e.Reason == CancellationReason.Error)
                        {
                            Trace.TraceError($"Failed to decode incoming text message: {e.ErrorDetails}");
                        }

                        stopTranslation.TrySetResult(0);
                    };

                    recognizer.SessionStopped += (s, e) => {
                        Trace.TraceInformation("Session stopped event.");
                        stopTranslation.TrySetResult(0);
                    };

                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopTranslation.Task });

                    // Stops translation.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

                    return utterance;
                }
            }
        }
    }
}
