using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBot.Models
{
    public class BotConstants
    {
        public const string GenderMale = "male";
        public const string GenderFemale = "female";
        public const string YesString = "yes";
        public const string EnglishLanguage = "en";
        public const string SpanishLanguage = "es-es";
        public const string FrenchLanguage = "fr-fr";
        public const string Site = "http://localhost:3978/images";
        public const string ValidAudioContentTypes = @"^audio/(wav)|multipart/(form-data)$";

        // Text To Speech API
        public const string TextToSpeechUri = "https://speech.platform.bing.com/synthesize";
        public const string TextToSpeechAzureContainer = "texttospeech";

        /// <summary>
        /// Key in the bot config (.bot file) for the LUIS instance.
        /// In the .bot file, multiple instances of LUIS can be configured.
        /// </summary>
        public const string LuisKey = "BasicBotLuisApplication";

        /// <summary>
        /// Key in the bot config (.bot file) for the QnA Maker instance.
        /// In the ".bot" file, multiple instances of QnA Maker can be configured.
        /// </summary>
        public const string QnAMakerKey = "QnABot";

        public static readonly string[] Specialties = new string[] { "Pizza", "Lasagna", "Carbonara" };
    }
}
