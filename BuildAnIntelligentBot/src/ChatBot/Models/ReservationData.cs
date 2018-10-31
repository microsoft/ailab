using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Models
{
    /// <summary>
    /// Class to store conversation data. We need a dictionary structure to pass the conversation state to dialogs.
    /// </summary>
    public class ReservationData : Dictionary<string, object>
    {
        private const string AmountPeopleKey = "AmountPeople";
        private const string FullNameKey = "FullName";
        private const string TimeKey = "Time";
        private const string ConfirmedKey = "Confirmed";
        private const string ConversationLanguageKey = "ConversationLanguage";

        public ReservationData()
        {
            this[AmountPeopleKey] = null;
            this[FullNameKey] = null;
            this[TimeKey] = null;
            this[ConfirmedKey] = null;
            this[ConversationLanguageKey] = null;
        }

        public ReservationData(IDictionary<string, object> source)
        {
            if (source != null)
            {
                source.ToList().ForEach(x => this.Add(x.Key, x.Value));
            }
        }

        public string AmountPeople
        {
            get { return (string)this[AmountPeopleKey]; }
            set { this[AmountPeopleKey] = value; }
        }

        public string Time
        {
            get { return (string)this[TimeKey]; }
            set { this[TimeKey] = value; }
        }

        public string FullName
        {
            get { return (string)this[FullNameKey]; }
            set { this[FullNameKey] = value; }
        }

        public string FirstName
        {
            get { return ((string)this[FullNameKey])?.Split(" ")[0]; }
        }

        public string Confirmed
        {
            get { return (string)this[ConfirmedKey]; }
            set { this[ConfirmedKey] = value; }
        }

        public string ConversationLanguage
        {
            get { return (string)this[ConversationLanguageKey]; }
            set { this[ConversationLanguageKey] = value; }
        }
    }
}
