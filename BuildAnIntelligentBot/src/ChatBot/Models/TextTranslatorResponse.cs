using System.Collections.Generic;

namespace ChatBot.Models
{
    public class TextTranslatorResponse
    {
        public TextTranslatorDetectedLanguage DetectedLanguage { get; set; }

        public List<TextTranslatorTranslation> Translations { get; set; }
    }

    public class TextTranslatorDetectedLanguage
    {
        public string Language { get; set; }

        public double Score { get; set; }
    }

    public class TextTranslatorTranslation
    {
        public string Text { get; set; }

        public string To { get; set; }
    }

    public class TextTranslatorDetectResponse
    {
        public string Language { get; set; }

        public double Score { get; set; }

        public bool IsTranslationSupported { get; set; }

        public bool IsTransliterationSupported { get; set; }
    }
}
