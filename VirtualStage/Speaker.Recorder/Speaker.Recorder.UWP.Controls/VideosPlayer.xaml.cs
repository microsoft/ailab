using System;
using Windows.Media;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Speaker.Recorder.UWP
{
    public sealed partial class VideosPlayer : UserControl
    {
        private MediaTimelineController timelineController;

        public event EventHandler<TimeSpan> PositionChanged;

        public event EventHandler MediaEnded;

        public VideosPlayer()
        {
            this.InitializeComponent();
            this.timelineController = new MediaTimelineController();
            this.timelineController.Ended += TimelineController_Ended;
            this.timelineController.PositionChanged += TimelineController_PositionChanged;
        }

        public void MoveMainToBack() => this.MoveUiToBack(this.MainMediaElementButton, this.MainGrid);

        public void MoveSecondToBack() => this.MoveUiToBack(this.SecondMediaElementButton, this.MainGrid);

        public void MoveThirdToBack() => this.MoveUiToBack(this.ThirdMediaElementButton, this.MainGrid);

        public void MoveUiToBack(UIElement element, Grid yourParentElement)
        {
            int index = yourParentElement.Children.IndexOf(element);
            if (index != -1)
                yourParentElement.Children.Move((uint)index, 0);
        }

        public async void SetVideos(string[] videos)
        {
            this.timelineController.Pause();
            this.timelineController.Position = TimeSpan.Zero;
            this.timelineController.Duration = null;
            this.MainMediaElementButton.IsChecked = true;
            if (videos?.Length >= 1)
            {
                this.MainMediaPlayer.Source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(videos[0]));
            }
            else
            {
                this.MainMediaPlayer.Source = null;
            }
            if (videos?.Length >= 2)
            {
                this.SecondMediaPlayer.Source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(videos[1]));
            }
            else
            {
                this.SecondMediaPlayer.Source = null;
            }
            if (videos?.Length >= 3)
            {
                this.ThirdMediaPlayer.Source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(videos[2]));
            }
            else
            {
                this.ThirdMediaPlayer.Source = null;
            }
        }

        private void TimelineController_PositionChanged(MediaTimelineController sender, object args)
        {
            this.PositionChanged?.Invoke(this, sender.Position);
        }

        private void TimelineController_Ended(MediaTimelineController sender, object args)
        {
            this.MediaEnded?.Invoke(this, EventArgs.Empty);
        }

        public void Play(TimeSpan? duration)
        {
            if (this.timelineController.State == MediaTimelineControllerState.Paused && this.timelineController.Position != TimeSpan.Zero)
            {
                this.timelineController.Resume();
            }
            else
            {
                this.SetTimeController(this.MainMediaPlayer);
                this.SetTimeController(this.SecondMediaPlayer);
                this.SetTimeController(this.ThirdMediaPlayer);

                this.timelineController.Duration = duration;
                this.timelineController.Start();
            }
        }

        private void SetTimeController(MediaPlayerElement mediaPlayerElement)
        {
            if (mediaPlayerElement.MediaPlayer != null)
            {
                mediaPlayerElement.MediaPlayer.TimelineController = this.timelineController;
            }
        }

        public void Pause()
        {
            this.timelineController.Pause();
        }

        public void Seek(TimeSpan position)
        {
            var isRunning = this.timelineController.State == MediaTimelineControllerState.Running;
            this.timelineController.Position = position;
            if (isRunning)
            {
                this.timelineController.Resume();
            }
        }
    }
}
