using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Models;
using ChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ChatBot.Dialogs
{
  public class ReservationDialog
  {
    // Conversation steps
    public const string TimePrompt = "timePrompt";
    public const string AmountPeoplePrompt = "amountPeoplePrompt";
    public const string NamePrompt = "namePrompt";
    public const string ConfirmationPrompt = "confirmationPrompt";

    // Dialog IDs
    private const string ProfileDialog = "profileDialog";

    private readonly TextToSpeechService _ttsService;

    public ReservationDialog(
        IStatePropertyAccessor<ReservationData> userProfileStateAccessor,
        TextToSpeechService ttsService)
    {
      UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));

      _ttsService = ttsService;

      // Add control flow dialogs

      // Add control flow dialogs
    }

    public IStatePropertyAccessor<ReservationData> UserProfileAccessor { get; }
  }
}