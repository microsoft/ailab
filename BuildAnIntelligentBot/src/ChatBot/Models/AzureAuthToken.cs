using System;

namespace ChatBot.Models
{
  /// <summary>
  /// Class with the Cognitive Services Azure Auth Token information.
  /// </summary>
  public class AzureAuthToken
  {
    // When the last valid token was obtained.
    private DateTime _storedTokenTime = DateTime.MinValue;

    // Cache the value of the last valid token obtained from the token service.
    private string _storedTokenValue = string.Empty;

    public AzureAuthToken(string key, string tokenValue)
    {
      if (string.IsNullOrEmpty(key))
      {
        throw new ArgumentNullException("key", "A subscription key is required");
      }

      SubscriptionKey = key;
      StoredTokenValue = tokenValue;
    }

    // Gets the subscription key.
    public string SubscriptionKey { get; private set; }

    public string StoredTokenValue
    {
      get
      {
        return this._storedTokenValue;
      }

      set
      {
        this._storedTokenTime = DateTime.Now;
        this._storedTokenValue = "Bearer " + value;
      }
    }

    public DateTime LastUpdateTime
    {
      get
      {
        return this._storedTokenTime;
      }
    }
  }
}
