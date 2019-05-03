// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// This bot will respond to the user's input with rich card content.
    /// Microsoft Bot Framework currently supports eight types of rich cards.
    /// We will demonstrate the use of each of these types in this project.
    /// Not all card types are supported on all channels.
    /// Please view the documentation in the ReadMe.md file in this project for more information.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class CardsBot : IBot
    {
        private const string WelcomeText = @"This bot will show you different types of Rich Cards.
                                           Please type anything to get started.";

        private CardsBotAccessors _accessors;

        private DialogSet _dialogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardsBot"/> class.
        /// In the constructor for the bot we are instantiating our <see cref="DialogSet"/>, giving our field a value,
        /// and adding our <see cref="WaterfallDialog"/> and <see cref="ChoicePrompt"/> to the dialog set.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        public CardsBot(CardsBotAccessors accessors)
        {
            this._accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            this._dialogs = new DialogSet(accessors.ConversationDialogState);
            this._dialogs.Add(new WaterfallDialog("cardSelector", new WaterfallStep[] { ChoiceCardStepAsync, ShowCardStepAsync }));
            this._dialogs.Add(new ChoicePrompt("cardPrompt"));
        }

        /// <summary>
        /// This controls what happens when an activity gets sent to the bot.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogContext = await this._dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dialogContext.BeginDialogAsync("cardSelector", cancellationToken: cancellationToken);
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded != null)
                {
                    await SendWelcomeMessageAsync(turnContext, cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected", cancellationToken: cancellationToken);
            }

            // Save the dialog state into the conversation state.
            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Sends a welcome message to the user.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = turnContext.Activity.CreateReply();
                    reply.Text = $"Welcome to CardBot {member.Name}. {WelcomeText}";
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Prompts the user for input by sending a <see cref="ChoicePrompt"/> so the user may select their
        /// choice from a list of options.
        /// </summary>
        /// <param name="step">A <see cref="WaterfallStepContext"/> provides context for the current waterfall step.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="DialogTurnResult"/> to communicate some flow control back to the containing WaterfallDialog.</returns>
        private static async Task<DialogTurnResult> ChoiceCardStepAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            return await step.PromptAsync("cardPrompt", GenerateOptions(step.Context.Activity), cancellationToken);
        }

        /// <summary>
        /// Creates options for a <see cref="ChoicePrompt"/> so the user may select an option.
        /// </summary>
        /// <param name="activity">The message activity the bot received.</param>
        /// <returns>A <see cref="PromptOptions"/> to be used in a prompt.</returns>
        /// <remarks>Related type <see cref="Choice"/>.</remarks>
        private static PromptOptions GenerateOptions(Activity activity)
        {
            // Create options for the prompt
            var options = new PromptOptions()
            {
                Prompt = activity.CreateReply("What card would you like to see? You can click or type the card name"),
                Choices = new List<Choice>(),
            };

            // Add the choices for the prompt.
            options.Choices.Add(new Choice() { Value = "Adaptive card" });
            options.Choices.Add(new Choice() { Value = "Animation card" });
            options.Choices.Add(new Choice() { Value = "Audio card" });
            options.Choices.Add(new Choice() { Value = "Hero card" });
            options.Choices.Add(new Choice() { Value = "Receipt card" });
            options.Choices.Add(new Choice() { Value = "Signin card" });
            options.Choices.Add(new Choice() { Value = "Thumbnail card" });
            options.Choices.Add(new Choice() { Value = "Video card" });
            options.Choices.Add(new Choice() { Value = "All cards" });

            return options;
        }

        /// <summary>
        /// This method uses the text of the activity to decide which type
        /// of card to respond with and reply with that card to the user.
        /// </summary>
        /// <param name="step">A <see cref="WaterfallStepContext"/> provides context for the current waterfall step.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="DialogTurnResult"/> indicating the turn has ended.</returns>
        /// <remarks>Related types <see cref="Attachment"/> and <see cref="AttachmentLayoutTypes"/>.</remarks>
        private static async Task<DialogTurnResult> ShowCardStepAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            // Get the text from the activity to use to show the correct card
            var text = step.Context.Activity.Text.ToLowerInvariant().Split(' ')[0];

            // Reply to the activity we received with an activity.
            var reply = step.Context.Activity.CreateReply();

            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments on the activity.
            reply.Attachments = new List<Attachment>();

            // Decide which type of card(s) we are going to show the user
            if (text.StartsWith("hero"))
            {
                // Display a HeroCard.
                reply.Attachments.Add(GetHeroCard().ToAttachment());
            }
            else if (text.StartsWith("thumb"))
            {
                // Display a ThumbnailCard.
                reply.Attachments.Add(GetThumbnailCard().ToAttachment());
            }
            else if (text.StartsWith("receipt"))
            {
                // Display a ReceiptCard.
                reply.Attachments.Add(GetReceiptCard().ToAttachment());
            }
            else if (text.StartsWith("sign"))
            {
                // Display a SignInCard.
                reply.Attachments.Add(GetSigninCard().ToAttachment());
            }
            else if (text.StartsWith("animation"))
            {
                // Display an AnimationCard.
                reply.Attachments.Add(GetAnimationCard().ToAttachment());
            }
            else if (text.StartsWith("video"))
            {
                // Display a VideoCard
                reply.Attachments.Add(GetVideoCard().ToAttachment());
            }
            else if (text.StartsWith("audio"))
            {
                // Display an AudioCard
                reply.Attachments.Add(GetAudioCard().ToAttachment());
            }
            else if (text.StartsWith("adaptive"))
            {
                reply.Attachments.Add(CreateAdaptiveCardAttachment());
            }
            else
            {
                // Display a carousel of all the rich card types.
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Attachments.Add(CreateAdaptiveCardAttachment());
                reply.Attachments.Add(GetHeroCard().ToAttachment());
                reply.Attachments.Add(GetThumbnailCard().ToAttachment());
                reply.Attachments.Add(GetReceiptCard().ToAttachment());
                reply.Attachments.Add(GetSigninCard().ToAttachment());
                reply.Attachments.Add(GetAnimationCard().ToAttachment());
                reply.Attachments.Add(GetVideoCard().ToAttachment());
                reply.Attachments.Add(GetAudioCard().ToAttachment());
            }

            // Send the card(s) to the user as an attachment to the activity
            await step.Context.SendActivityAsync(reply, cancellationToken);

            // Give the user instructions about what to do next
            await step.Context.SendActivityAsync("Type anything to see another card.", cancellationToken: cancellationToken);

            return await step.EndDialogAsync(cancellationToken: cancellationToken);
        }

        // The following methods are all used to generate cards

        /// <summary>
        /// This creates an <see cref="AdaptiveCard"/> as an <see cref="Attachment"/> from a .json file.
        /// </summary>
        /// <returns>An <see cref="AdaptiveCard"/>.</returns>
        private static Attachment CreateAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            string[] paths = { ".", "Resources", "adaptiveCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Creates a <see cref="HeroCard"/>.
        /// </summary>
        /// <returns>A <see cref="HeroCard"/> the user can view and/or interact with.</returns>
        /// <remarks>Related types <see cref="CardImage"/>, <see cref="CardAction"/>,
        /// and <see cref="ActionTypes"/>.</remarks>
        private static HeroCard GetHeroCard()
        {
            var heroCard = new HeroCard
            {
                Title = "BotFramework Hero Card",
                Subtitle = "Microsoft Bot Framework",
                Text = "Build and connect intelligent bots to interact with your users naturally wherever they are," +
                       " from text/sms to Skype, Slack, Office 365 mail and other popular services.",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            };

            return heroCard;
        }

        /// <summary>
        /// Creates a <see cref="ThumbnailCard"/>.
        /// </summary>
        /// <returns>A <see cref="ThumbnailCard"/> the user can view and/or interact with.</returns>
        /// <remarks>Related types <see cref="CardImage"/>, <see cref="CardAction"/>,
        /// and <see cref="ActionTypes"/>.</remarks>
        private static ThumbnailCard GetThumbnailCard()
        {
            var heroCard = new ThumbnailCard
            {
                Title = "BotFramework Thumbnail Card",
                Subtitle = "Microsoft Bot Framework",
                Text = "Build and connect intelligent bots to interact with your users naturally wherever they are," +
                       " from text/sms to Skype, Slack, Office 365 mail and other popular services.",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            };

            return heroCard;
        }

        /// <summary>
        /// Creates a <see cref="ReceiptCard"/>.
        /// </summary>
        /// <returns>A <see cref="ReceiptCard"/> the user can view and/or interact with.</returns>
        /// <remarks>Related types <see cref="CardImage"/>, <see cref="CardAction"/>,
        /// <see cref="ActionTypes"/>, <see cref="ReceiptItem"/>, and <see cref="Fact"/>.</remarks>
        private static ReceiptCard GetReceiptCard()
        {
            var receiptCard = new ReceiptCard
            {
                Title = "John Doe",
                Facts = new List<Fact> { new Fact("Order Number", "1234"), new Fact("Payment Method", "VISA 5555-****") },
                Items = new List<ReceiptItem>
                {
                    new ReceiptItem(
                        "Data Transfer",
                        price: "$ 38.45",
                        quantity: "368",
                        image: new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png")),
                    new ReceiptItem(
                        "App Service",
                        price: "$ 45.00",
                        quantity: "720",
                        image: new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png")),
                },
                Tax = "$ 7.50",
                Total = "$ 90.95",
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Type = ActionTypes.OpenUrl,
                        Title = "More information",
                        Value = @"https://azure.microsoft.com/en-us/pricing/"
                    },
                },
            };

            return receiptCard;
        }

        /// <summary>
        /// Creates a <see cref="SigninCard"/>.
        /// </summary>
        /// <returns>A <see cref="SigninCard"/> the user can interact with.</returns>
        /// <remarks>Related types <see cref="CardAction"/> and <see cref="ActionTypes"/>.</remarks>
        private static SigninCard GetSigninCard()
        {
            var signinCard = new SigninCard
            {
                Text = "BotFramework Sign-in Card",
                Buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "Sign-in", value: "https://login.microsoftonline.com/") },
            };

            return signinCard;
        }

        /// <summary>
        /// Creates a <see cref="AnimationCard"/>.
        /// </summary>
        /// <returns>A <see cref="AnimationCard"/> the user can view and/or interact with.</returns>
        /// <remarks>Related types <see cref="CardImage"/>, <see cref="CardAction"/>,
        /// <see cref="ActionTypes"/>, <see cref="MediaUrl"/>, and <see cref="ThumbnailUrl"/>.</remarks>
        private static AnimationCard GetAnimationCard()
        {
            var animationCard = new AnimationCard
            {
                Title = "Microsoft Bot Framework",
                Subtitle = "Animation Card",
                Image = new ThumbnailUrl
                {
                    Url = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png",
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "http://i.giphy.com/Ki55RUbOV5njy.gif",
                    },
                },
            };

            return animationCard;
        }

        /// <summary>
        /// Creates a <see cref="VideoCard"/>.
        /// </summary>
        /// <returns>A <see cref="VideoCard"/> the user can view and/or interact with.</returns>
        /// <remarks> Related types <see cref="CardAction"/>,
        /// <see cref="ActionTypes"/>, <see cref="MediaUrl"/>, and <see cref="ThumbnailUrl"/>.</remarks>
        private static VideoCard GetVideoCard()
        {
            var videoCard = new VideoCard
            {
                Title = "Big Buck Bunny",
                Subtitle = "by the Blender Institute",
                Text = "Big Buck Bunny (code-named Peach) is a short computer-animated comedy film by the Blender Institute," +
                       " part of the Blender Foundation. Like the foundation's previous film Elephants Dream," +
                       " the film was made using Blender, a free software application for animation made by the same foundation." +
                       " It was released as an open-source film under Creative Commons License Attribution 3.0.",
                Image = new ThumbnailUrl
                {
                    Url = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Big_buck_bunny_poster_big.jpg/220px-Big_buck_bunny_poster_big.jpg",
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "http://download.blender.org/peach/bigbuckbunny_movies/BigBuckBunny_320x180.mp4",
                    },
                },
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Title = "Learn More",
                        Type = ActionTypes.OpenUrl,
                        Value = "https://peach.blender.org/",
                    },
                },
            };

            return videoCard;
        }

        /// <summary>
        /// Creates a <see cref="AudioCard"/>.
        /// </summary>
        /// <returns>A <see cref="AudioCard"/> the user can listen to or interact with.</returns>
        /// <remarks> Related types <see cref="CardAction"/>,
        /// <see cref="ActionTypes"/>, <see cref="MediaUrl"/>, and <see cref="ThumbnailUrl"/>.</remarks>
        private static AudioCard GetAudioCard()
        {
            var audioCard = new AudioCard
            {
                Title = "I am your father",
                Subtitle = "Star Wars: Episode V - The Empire Strikes Back",
                Text = "The Empire Strikes Back (also known as Star Wars: Episode V – The Empire Strikes Back)" +
                       " is a 1980 American epic space opera film directed by Irvin Kershner. Leigh Brackett and" +
                       " Lawrence Kasdan wrote the screenplay, with George Lucas writing the film's story and serving" +
                       " as executive producer. The second installment in the original Star Wars trilogy, it was produced" +
                       " by Gary Kurtz for Lucasfilm Ltd. and stars Mark Hamill, Harrison Ford, Carrie Fisher, Billy Dee Williams," +
                       " Anthony Daniels, David Prowse, Kenny Baker, Peter Mayhew and Frank Oz.",
                Image = new ThumbnailUrl
                {
                    Url = "https://upload.wikimedia.org/wikipedia/en/3/3c/SW_-_Empire_Strikes_Back.jpg",
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "http://www.wavlist.com/movies/004/father.wav",
                    },
                },
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Title = "Read More",
                        Type = ActionTypes.OpenUrl,
                        Value = "https://en.wikipedia.org/wiki/The_Empire_Strikes_Back",
                    },
                },
            };

            return audioCard;
        }
    }
}
