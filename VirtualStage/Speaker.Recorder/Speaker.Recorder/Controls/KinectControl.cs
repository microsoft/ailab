using Microsoft.Extensions.DependencyInjection;
using Speaker.Recorder.Kinect;
using System;
using System.Windows;
using System.Windows.Controls;
using Image = System.Windows.Controls.Image;

namespace Speaker.Recorder.Controls
{
    [TemplatePart(Name = ImageHostPart, Type = typeof(Image))]
    public class KinectControl : Control
    {
        protected const string ImageHostPart = "PART_ImageHost";
        private Image imageHost;
        private KinectD3DImage imageHostSource;
        private KinectCaptureProvider captureProvider;
        private KinectImageProvider imageProvider;
        private object lastKey;
        private KinectCaptureProvidersFactory kinectCaptureProvidersFactory;
        public static readonly DependencyProperty DeviceIndexProperty = DependencyProperty.Register(nameof(DeviceIndex), typeof(int), typeof(KinectControl), new PropertyMetadata(-1, DeviceIndexChanged));

        public int DeviceIndex
        {
            get { return (int)GetValue(DeviceIndexProperty); }
            set { SetValue(DeviceIndexProperty, value); }
        }

        public static readonly DependencyProperty PlaybackPathProperty = DependencyProperty.Register(nameof(PlaybackPath), typeof(string), typeof(KinectControl), new PropertyMetadata(null, PlaybackPathChanged));


        public string PlaybackPath
        {
            get { return (string)GetValue(PlaybackPathProperty); }
            set { SetValue(PlaybackPathProperty, value); }
        }

        public static readonly DependencyProperty SelectedImageProperty = DependencyProperty.Register(nameof(SelectedImage), typeof(KinectCaptureImage), typeof(KinectControl), new PropertyMetadata(KinectCaptureImage.Color, ProviderPropertyChanged));

        public KinectCaptureImage SelectedImage
        {
            get { return (KinectCaptureImage)GetValue(SelectedImageProperty); }
            set { SetValue(SelectedImageProperty, value); }
        }

        public static readonly DependencyProperty IsFailedProperty = DependencyProperty.Register(nameof(IsFailed), typeof(bool), typeof(KinectControl), new PropertyMetadata(false));

        public bool IsFailed
        {
            get { return (bool)GetValue(IsFailedProperty); }
            set { SetValue(IsFailedProperty, value); }
        }

        public static readonly DependencyProperty RenderWidthProperty = DependencyProperty.Register(nameof(RenderWidth), typeof(double), typeof(KinectControl), new PropertyMetadata(double.NaN, ProviderPropertyChanged));

        public double RenderWidth
        {
            get { return (double)GetValue(RenderWidthProperty); }
            set { SetValue(RenderWidthProperty, value); }
        }

        public static readonly DependencyProperty RenderHeightProperty = DependencyProperty.Register(nameof(RenderHeight), typeof(double), typeof(KinectControl), new PropertyMetadata(double.NaN, ProviderPropertyChanged));

        public double RenderHeight
        {
            get { return (double)GetValue(RenderHeightProperty); }
            set { SetValue(RenderHeightProperty, value); }
        }

        public static readonly DependencyProperty FpsDividerProperty = DependencyProperty.Register(nameof(FpsDivider), typeof(int), typeof(KinectControl), new PropertyMetadata(1, ProviderPropertyChanged));

        public int FpsDivider
        {
            get { return (int)GetValue(FpsDividerProperty); }
            set { SetValue(FpsDividerProperty, value); }
        }

        static KinectControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KinectControl), new FrameworkPropertyMetadata(typeof(KinectControl)));
        }

        public KinectControl()
        {
            this.kinectCaptureProvidersFactory = (Application.Current as App)?.AppHost.Services.GetRequiredService<KinectCaptureProvidersFactory>();
            this.Dispatcher.ShutdownStarted += this.DispatcherShutdownStarted;
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
            this.IsVisibleChanged += this.OnIsVisibleChanged;
        }

        private void DispatcherShutdownStarted(object sender, EventArgs e)
        {
            this.UpdateProvider(null);
            this.imageHostSource?.Dispose();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                this.UpdateSource();
            }
            else
            {
                this.imageHostSource?.SetProvider(null);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.UpdateProvider(null);
            this.imageHostSource?.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.UpdateSource();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.imageHost != null)
            {
                this.imageHostSource?.Dispose();
                this.imageHost.Source = null;
            }

            this.imageHost = (Image)this.GetTemplateChild(ImageHostPart);
            this.imageHostSource = new KinectD3DImage();
            this.imageHost.Source = this.imageHostSource;
        }

        private void UpdateProvider(object newValue)
        {
            this.IsFailed = false;

            if (this.captureProvider != null)
            {
                this.captureProvider.IsConnectedChanged -= this.CaptureProviderIsConnectedChanged;
            }

            this.kinectCaptureProvidersFactory.Remove(this.lastKey);

            if (App.Current?.MainWindow == null)
            {
                return;
            }

            KinectCaptureProvider next = null;
            if (newValue is string path && path != null || newValue is int index && index >= 0)
            {
                next = this.kinectCaptureProvidersFactory.GetOrCreateProvider(newValue);
            }

            this.lastKey = newValue;
            this.captureProvider = next;
            if (this.captureProvider != null)
            {
                this.captureProvider.IsConnectedChanged += this.CaptureProviderIsConnectedChanged;
            }

            this.CaptureProviderIsConnectedChanged(null, null);
            this.UpdateSource();
        }

        private void CaptureProviderIsConnectedChanged(object sender, System.EventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                this.IsFailed = this.captureProvider?.IsConnected == null ? false : !this.captureProvider.IsConnected.Value;
            });
        }

        private void UpdateSource()
        {
            if (this.IsVisible)
            {
                this.imageProvider?.Dispose();
                this.imageProvider = this.captureProvider?.For(this.SelectedImage);
                if (this.imageHostSource != null)
                {
                    this.imageHostSource.FpsDivider = this.FpsDivider;
                    this.imageHostSource.RenderWidth = (int)(double.IsNaN(this.RenderWidth) ? -1 : this.RenderWidth);
                    this.imageHostSource.RenderHeight = (int)(double.IsNaN(this.RenderHeight) ? -1 : this.RenderHeight);
                    this.imageHostSource.SetProvider(this.imageProvider);
                }
            }
        }

        private static void PlaybackPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((KinectControl)d).UpdateProvider(e.NewValue);
        }

        private static void DeviceIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((KinectControl)d).UpdateProvider(e.NewValue);
        }

        private static void ProviderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((KinectControl)d).UpdateSource();
        }
    }
}
