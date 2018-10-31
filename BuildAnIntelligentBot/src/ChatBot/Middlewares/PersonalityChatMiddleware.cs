using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.PersonalityChat;
using Microsoft.Bot.Builder.PersonalityChat.Core;
using Microsoft.Bot.Schema;

namespace ChatBot.Middlewares
{
    public class PersonalityChatMiddleware : IMiddleware
    {
        private readonly PersonalityChatService personalityChatService;
        private readonly PersonalityChatMiddlewareOptions personalityChatMiddlewareOptions;

        public PersonalityChatMiddleware(PersonalityChatMiddlewareOptions personalityChatMiddlewareOptions)
        {
            this.personalityChatMiddlewareOptions = personalityChatMiddlewareOptions ?? throw new ArgumentNullException(nameof(personalityChatMiddlewareOptions));

            this.personalityChatService = new PersonalityChatService(personalityChatMiddlewareOptions);
        }

        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var messageActivity = context.Activity.AsMessageActivity();
                if (!string.IsNullOrEmpty(messageActivity.Text))
                {
                    var results = await this.personalityChatService.QueryServiceAsync(messageActivity.Text.Trim()).ConfigureAwait(false);

                    if (!this.personalityChatMiddlewareOptions.RespondOnlyIfChat || results.IsChatQuery)
                    {
                        string personalityChatResponse = this.GetResponse(results);
                        await this.PostPersonalityChatResponseToUserAsync(context, next, personalityChatResponse);
                    }
                }
            }

            if (this.personalityChatMiddlewareOptions.EndActivityRoutingOnResponse)
            {
                // Query is answered, don't keep routing
                return;
            }

            await next(cancellationToken);
        }

        public virtual string GetResponse(PersonalityChatResults personalityChatResults)
        {
            var matchedScenarios = personalityChatResults?.ScenarioList;

            string response = string.Empty;

            if (matchedScenarios != null)
            {
                var topScenario = matchedScenarios.FirstOrDefault();

                if (topScenario?.Responses != null && topScenario.Score > this.personalityChatMiddlewareOptions.ScoreThreshold && topScenario.Responses.Count > 0)
                {
                    Random randomGenerator = new Random();
                    int randomIndex = randomGenerator.Next(topScenario.Responses.Count);

                    response = topScenario.Responses[randomIndex];
                }
            }

            return response;
        }

        public virtual async Task PostPersonalityChatResponseToUserAsync(ITurnContext context, NextDelegate next, string personalityChatResponse)
        {
            if (!string.IsNullOrEmpty(personalityChatResponse))
            {
                await context.SendActivityAsync(personalityChatResponse).ConfigureAwait(false);
            }
        }
    }
}
