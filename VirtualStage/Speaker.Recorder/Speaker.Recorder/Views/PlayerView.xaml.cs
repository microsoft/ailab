using Microsoft.Toolkit.Wpf.UI.XamlHost;
using Speaker.Recorder.Services;
using Speaker.Recorder.UWP;
using Speaker.Recorder.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Speaker.Recorder.Views
{
    public partial class PlayerView : UserControl
    {
        private bool isDragging;
        private VideosPlayer player;

        public PlayerView()
        {
            InitializeComponent();

            this.Unloaded += PlayerView_Unloaded;
            this.DataContextChanged += PlayerView_DataContextChanged;
        }

        private void PlayerView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.player?.SetVideos(null);
            this.PlayerViewerHost.Child = null;
        }

        private void PlayerView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue is PlayerViewModel player)
            {
                player.PropertyChanged -= PlayerView_PropertyChanged;
            }

            if (e.NewValue != null && e.NewValue is PlayerViewModel playerNew)
            {
                playerNew.PropertyChanged += PlayerView_PropertyChanged;
                ResetMediaElements(((PlayerViewModel)this.DataContext).RecordedSessionSelected);
            }
        }

        private void PlayerView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayerViewModel.RecordedSessionSelected))
            {
                ResetMediaElements(((PlayerViewModel)this.DataContext).RecordedSessionSelected);
            }
        }

        private void ResetMediaElements(Session session)
        {
            if(this.player == null)
            {
                return;
            }

            ((PlayerViewModel)this.DataContext).IsPlaying = false;
            this.player.SetVideos(session?.KinectVideoPaths?.Concat(new[] { session?.PresentationVideoPath }).Where(x => x != null).ToArray());
            var time = TimeSpan.Zero;
            TimelineSlider.Value = time.TotalSeconds;
            TextTimeLine.Text = time.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
        }

        private void TimelineSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void TimelineSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (this.player != null)
            {
                this.player.Seek(TimeSpan.FromSeconds(TimelineSlider.Value));
                TextTimeLine.Text = TimeSpan.FromSeconds(TimelineSlider.Value).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
            }

            isDragging = false;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var session = ((PlayerViewModel)this.DataContext).RecordedSessionSelected;
            if (session?.Duration > TimeSpan.Zero)
            {
                this.player.Play(session.Duration);
            }
            else
            {
                ((PlayerViewModel)this.DataContext).IsPlaying = false;
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.player.Pause();
        }

        private void WindowsXamlHost_ChildChanged(object sender, EventArgs e)
        {
            if(this.player != null)
            {
                this.player.PositionChanged -= Player_PositionChanged;
                this.player.MediaEnded -= Player_MediaEnded;
            }

            this.player = ((WindowsXamlHost)sender).Child as VideosPlayer;
            if (this.player != null)
            {
                this.player.PositionChanged += Player_PositionChanged;
                this.player.MediaEnded += Player_MediaEnded;
                ResetMediaElements(((PlayerViewModel)this.DataContext).RecordedSessionSelected);
            }
        }

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ((PlayerViewModel)this.DataContext).IsPlaying = false;
            });
        }

        private void Player_PositionChanged(object sender, TimeSpan e)
        {
            if (!isDragging)
            {
                this.Dispatcher.Invoke(() =>
                {
                    TimelineSlider.Value = e.TotalSeconds;
                    TextTimeLine.Text = e.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                });
            }
        }
    }
}
