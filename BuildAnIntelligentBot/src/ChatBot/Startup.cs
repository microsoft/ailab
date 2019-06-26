// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using ChatBot.Dialogs;
using ChatBot.Middlewares;
using ChatBot.Models;
using ChatBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.PersonalityChat;
using Microsoft.Bot.Builder.PersonalityChat.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalityChatMiddleware = ChatBot.Middlewares.PersonalityChatMiddleware;

namespace ChatBot
{
  /// <summary>
  /// The Startup class configures services and the request pipeline.
  /// </summary>
  public class Startup
  {
    private ILoggerFactory _loggerFactory;
    private bool _isProduction = false;

    public Startup(IHostingEnvironment env)
    {
      _isProduction = env.IsProduction();
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
          .AddEnvironmentVariables();

      Configuration = builder.Build();
    }

    /// <summary>
    /// Gets the configuration that represents a set of key/value application configuration properties.
    /// </summary>
    /// <value>
    /// The <see cref="IConfiguration"/> that represents a set of key/value application configuration properties.
    /// </value>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> specifies the contract for a collection of service descriptors.</param>
    /// <seealso cref="IStatePropertyAccessor{T}"/>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/dependency-injection"/>
    /// <seealso cref="https://docs.microsoft.com/en-us/azure/bot-service/bot-service-manage-channels?view=azure-bot-service-4.0"/>
    public void ConfigureServices(IServiceCollection services)
    {
      services.Configure<MySettings>(Configuration);
#if DEBUG
      services.AddMvc();
#else
            services.AddMvc(mvcConfig =>
            {
                mvcConfig.Filters.Add(new BasicAuthFilter(Configuration["BasicAuthUsername"], Configuration["BasicAuthPassword"]));
            });
#endif

      // create luis recognizer

      // Create the credential provider to be used with the Bot Framework Adapter.
      services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

      // Create the Bot Framework Adapter with error handling enabled.
      services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

      // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
      services.AddSingleton<IStorage, MemoryStorage>();

      // Create the User state. (Used in this bot's Dialog implementation.)
      services.AddSingleton<UserState>();

      // Create the Conversation state. (Used by the Dialog system itself.)
      services.AddSingleton<ConversationState>();

      // The Dialog that will be run by the bot.
      services.AddSingleton<ReservationDialog>();

      // Memory Storage is for local bot debugging only. When the bot
      // is restarted, everything stored in memory will be gone.
      IStorage dataStore = new MemoryStorage();

      // Create and add conversation state.
      var conversationState = new ConversationState(dataStore);
      services.AddSingleton(conversationState);

      var userState = new UserState(dataStore);
      services.AddSingleton(userState);

      // Add the personality chat middleware

      // Add the translator speech middleware

      services.AddTransient<IBot, EchoBot>();

      // Create and register state accessors.
      // Accessors created here are passed into the IBot-derived class on every turn.
      services.AddSingleton(sp =>
      {
        // We need to grab the conversationState we added on the options in the previous step
        var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
        if (options == null)
        {
          throw new InvalidOperationException("BotFrameworkOptions must be configured prior to setting up the State Accessors");
        }

        // Create the custom state accessor.
        // State accessors enable other components to read and write individual properties of state.
        var accessors = new EchoBotAccessors(conversationState, userState)
        {
          // Initialize Dialog State
          ReservationState = userState.CreateProperty<ReservationData>("ReservationState"),
        };

        return accessors;
      });

      // Add QnA Maker here
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      _loggerFactory = loggerFactory;

      if (env.IsDevelopment())
      {
        app.UseBrowserLink();
        app.UseDeveloperExceptionPage();
      }

      app.UseDefaultFiles()
          .UseStaticFiles()
          .UseHttpsRedirection()
          .UseMvc(routes =>
          {
            routes.MapRoute(
                      name: "default",
                      template: "{controller=Home}/{action=Index}/{id?}");
          });
    }
  }
}
