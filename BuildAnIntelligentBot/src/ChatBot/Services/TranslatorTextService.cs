using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChatBot.Models;
using Newtonsoft.Json;

namespace ChatBot.Services
{
    public class TranslatorTextService
    {
        private const string TranslateMethodUri = "https://api.cognitive.microsofttranslator.com";
        private const string UriParams = "?api-version=3.0";
        private static readonly HttpClient Client = new HttpClient();
        private readonly string _translatorTextKey;

        public TranslatorTextService(string translatorTextKey)
        {
            _translatorTextKey = translatorTextKey;
        }

        public async Task<string> Detect(string text)
        {
            var body = new System.Object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri($"{TranslateMethodUri}/detect{UriParams}");
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _translatorTextKey);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<TextTranslatorDetectResponse>>(responseBody);
                return result?.FirstOrDefault()?.Language;
            }
        }

        public async Task<string> Translate(string sourceLanguage, string targetLanguage, string text)
        {
            if (string.Equals(sourceLanguage, targetLanguage, StringComparison.OrdinalIgnoreCase))
            {
                return text; // No translation required
            }

            var body = new System.Object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri($"{TranslateMethodUri}/translate{UriParams}&to={targetLanguage}");
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _translatorTextKey);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<TextTranslatorResponse>>(responseBody);
                return result.First().Translations.First().Text;
            }
        }
    }
}
