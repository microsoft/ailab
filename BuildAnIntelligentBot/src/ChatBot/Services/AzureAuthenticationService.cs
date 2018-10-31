using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ChatBot.Models;

namespace ChatBot.Services
{
    /// <summary>
    /// Service to handle the Azure authentication and handle the authentication process, generation of
    /// tokes and the cache of those tokens
    /// </summary>
    [Serializable]
    public class AzureAuthenticationService
    {
        // URL of the token service
        private const string ServiceUrl = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";

        // After obtaining a valid token, this class will cache it for this duration. Use a duration of
        // 8 minutes, which is less than the actual token lifetime of 10 minutes.
        private static readonly TimeSpan TokenCacheDuration = new TimeSpan(0, 8, 0);

        // Stores the associated tokens using the Subscription Key as the reference
        private static Dictionary<string, AzureAuthToken> _tokens = new Dictionary<string, AzureAuthToken>();

        public AzureAuthenticationService()
        {
        }

        /// <summary>
        /// Gets a token for the specified subscription.
        /// </summary>
        /// <returns>The encoded JWT token prefixed with the string "Bearer ".</returns>
        /// <remarks>
        /// This method uses a cache to limit the number of request to the token service. A fresh token
        /// can be re-used during its lifetime of 10 minutes. After a successful request to the token
        /// service, this method caches the access token. Subsequent invocations of the method return the
        /// cached token for the next 8 minutes. After 8 minutes, a new token is fetched from the token
        /// service and the cache is updated.
        /// </remarks>
        public static async Task<string> GetAccessTokenAsync(string key, string tokenUri = ServiceUrl)
        {
            AzureAuthToken azureAuthToken;

            // Re-use the cached AzureAuthToken if there is one.
            if (_tokens.ContainsKey(key))
            {
                azureAuthToken = _tokens[key];
                if ((DateTime.Now - azureAuthToken.LastUpdateTime) > TokenCacheDuration)
                {
                    // Current token value expires; lets create a new one
                    azureAuthToken.StoredTokenValue = await GenerateTokenAsync(key, tokenUri);
                }
            }
            else
            {
                // There is not an Object associated to the key,
                // create a new one an put in into the dictionary
                var tokenValue = await GenerateTokenAsync(key, tokenUri);
                azureAuthToken = new AzureAuthToken(key, tokenValue);
                _tokens.Add(key, azureAuthToken);
            }

            return azureAuthToken.StoredTokenValue;
        }

        private static async Task<string> GenerateTokenAsync(string key, string tokenUri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

                using (var response = await client.PostAsync(tokenUri, new StringContent(string.Empty)))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
