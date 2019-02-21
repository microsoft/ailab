// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using ChatBot.Models;
using Microsoft.Bot.Builder;

namespace ChatBot
{
    /// <summary>
    /// This class is created as a Singleton and passed into the IBot-derived constructor.
    ///  - See <see cref="EchoBot"/> constructor for how that is injected.
    ///  - See the Startup.cs file for more details on creating the Singleton that gets
    ///    injected into the constructor.
    /// </summary>
    public class EchoBotAccessors
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EchoBotAccessors"/> class.
        /// Contains the <see cref="ConversationState"/> and associated <see cref="IStatePropertyAccessor{T}"/>.
        /// </summary>
        /// <param name="conversationState">The state object that stores the conversation data.</param>
        public EchoBotAccessors(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }

        public static string ReservationStateName { get; } = $"{nameof(EchoBotAccessors)}.ReservationState";

        public static string CounterStateName { get; } = $"{nameof(EchoBotAccessors)}.CounterState";

        public IStatePropertyAccessor<CounterState> CounterState { get; set; }

        public IStatePropertyAccessor<ReservationData> ReservationState { get; set; }

        /// <summary>
        /// Gets the <see cref="ConversationState"/> object for the conversation.
        /// </summary>
        /// <value>The <see cref="ConversationState"/> object.</value>
        public ConversationState ConversationState { get; }
    }
}
