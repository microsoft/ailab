using Microsoft.Extensions.Logging;
using Speaker.Recorder.Services;
using Speaker.Recorder.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Speaker.Recorder.ViewModels
{
    public class PlayerViewModel : BaseViewModel, IDisposable
    {
        private readonly INavigationService navigationService;
        private ObservableCollection<Session> recordedSessions;
        private Session recordedSessionSelected;
        private readonly SessionsService sessionsService;
        private readonly ILogger<PlayerViewModel> logger;
        private Session temporalSessionForStop;

        private bool isPlaying;
        public bool IsPlaying
        {
            get => isPlaying;
            set => SetProperty(ref isPlaying, value);
        }
        
        private bool stopUploadDialogIsShowing;
        public bool StopUploadDialogIsShowing
        {
            get => stopUploadDialogIsShowing;
            set => SetProperty(ref stopUploadDialogIsShowing, value);
        }

        public ObservableCollection<Session> RecordedSessions
        {
            get => recordedSessions;
            set
            {
                SetProperty(ref recordedSessions, value);
            }
        }

        public Session RecordedSessionSelected { get => recordedSessionSelected; set => SetProperty(ref recordedSessionSelected, value); }

        public ICommand GoBackCommand { get; }

        public ICommand CancelStopUploadDialogCommand { get; }

        public ICommand AcceptStopUploadDialogCommand { get; }

        public RelayCommand<Session> OpenFolderCommand { get; }

        public RelayCommand<Session> PauseUploadCommand { get; }

        public RelayCommand<Session> PlayUploadCommand { get; }

        public RelayCommand<Session> StopUploadCommand { get; }

        public RelayCommand<Session> UploadCommand { get; }

        public PlayerViewModel(INavigationService navigationService, SessionsService sessionsService, ILogger<PlayerViewModel> logger)
        {
            this.navigationService = navigationService;
            this.sessionsService = sessionsService;
            this.logger = logger;
            this.GoBackCommand = new RelayCommand(this.GoBack);
            this.AcceptStopUploadDialogCommand = new RelayCommand(this.AcceptStopUploadDialog);
            this.CancelStopUploadDialogCommand = new RelayCommand(this.CancelStopUploadDialog);
            this.PlayUploadCommand = new RelayCommand<Session>(this.PlayUpload);
            this.PauseUploadCommand = new RelayCommand<Session>(this.PauseUpload);
            this.StopUploadCommand = new RelayCommand<Session>(this.StopUpload);
            this.UploadCommand = new RelayCommand<Session>(this.Upload);
            this.OpenFolderCommand = new RelayCommand<Session>(this.OpenFolder);

            this.Initialize();
        }

        private void CancelStopUploadDialog()
        {
            this.StopUploadDialogIsShowing = false;
        }

        private async void AcceptStopUploadDialog()
        {
            this.StopUploadDialogIsShowing = false;
            await temporalSessionForStop.StopUploadAsync();
        }

        private void OpenFolder(Session session)
        {
            this.logger.LogInformation($"Opening folder : {session.LocalRecordingFolderPath}");
            Process.Start(new ProcessStartInfo(session.LocalRecordingFolderPath) { UseShellExecute = true });
        }

        protected virtual void Initialize()
        {
            this.RecordedSessions = new ObservableCollection<Session>(sessionsService.GetSessions().OrderByDescending(x => x.Created));
            if (this.RecordedSessions.Count != 0)
            {
                this.RecordedSessionSelected = this.RecordedSessions[0];
            }
        }

        private async void PauseUpload(Session session)
        {
            await session.PauseUploadAsync().ConfigureAwait(false);
        }

        private async void PlayUpload(Session session)
        {
            await session.UploadAsync().ConfigureAwait(false);
        }

        private async void Upload(Session session)
        {
            await session.UploadAsync().ConfigureAwait(false);
        }

        private void StopUpload(Session session)
        {
            this.StopUploadDialogIsShowing = true;
            this.temporalSessionForStop = session;
        }

        private void GoBack() => this.navigationService.Back();

        public void Dispose()
        {
            foreach (var item in this.RecordedSessions)
            {
                item.Dispose();
            }
        }
    }
}