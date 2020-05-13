using Microsoft.Extensions.DependencyInjection;
using Speaker.Recorder.Kinect;
using Speaker.Recorder.PowerPoint;
using System;
using System.Windows;
using System.Windows.Controls;
using Windows.Graphics.Capture;
using Image = System.Windows.Controls.Image;

namespace Speaker.Recorder.Controls
{
    [TemplatePart(Name = ImageHostPart, Type = typeof(Image))]
    public class PowerPointControl : Control
    {
        protected const string ImageHostPart = "PART_ImageHost";
        private Image imageHost;
        private GraphicsItemD3DImage imageHostSource;
        private GraphicsCaptureItem currentGraphicsItem;
        private PowerPointGraphicsCaptureItemsFactory powerPointGraphicsCaptureItemsFactory;
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(nameof(FilePath), typeof(string), typeof(PowerPointControl), new PropertyMetadata(null, FilePathChanged));


        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public static readonly DependencyProperty IsFailedProperty = DependencyProperty.Register(nameof(IsFailed), typeof(bool), typeof(PowerPointControl), new PropertyMetadata(false));

        public bool IsFailed
        {
            get { return (bool)GetValue(IsFailedProperty); }
            set { SetValue(IsFailedProperty, value); }
        }

        public static readonly DependencyProperty RenderWidthProperty = DependencyProperty.Register(nameof(RenderWidth), typeof(double), typeof(PowerPointControl), new PropertyMetadata(double.NaN));

        public double RenderWidth
        {
            get { return (double)GetValue(RenderWidthProperty); }
            set { SetValue(RenderWidthProperty, value); }
        }

        public static readonly DependencyProperty RenderHeightProperty = DependencyProperty.Register(nameof(RenderHeight), typeof(double), typeof(PowerPointControl), new PropertyMetadata(double.NaN));

        public double RenderHeight
        {
            get { return (double)GetValue(RenderHeightProperty); }
            set { SetValue(RenderHeightProperty, value); }
        }

        public static readonly DependencyProperty FpsDividerProperty = DependencyProperty.Register(nameof(FpsDivider), typeof(int), typeof(PowerPointControl), new PropertyMetadata(1, ProviderPropertyChanged));

        public int FpsDivider
        {
            get { return (int)GetValue(FpsDividerProperty); }
            set { SetValue(FpsDividerProperty, value); }
        }

        static PowerPointControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PowerPointControl), new FrameworkPropertyMetadata(typeof(PowerPointControl)));
        }

        public PowerPointControl()
        {
            this.powerPointGraphicsCaptureItemsFactory = (Application.Current as App)?.AppHost.Services.GetRequiredService<PowerPointGraphicsCaptureItemsFactory>();
            this.Dispatcher.ShutdownStarted += this.DispatcherShutdownStarted;
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
            this.IsVisibleChanged += this.OnIsVisibleChanged;
        }

        private void DispatcherShutdownStarted(object sender, EventArgs e)
        {
            this.UpdateProvider(this.FilePath, null);
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                this.UpdateSource();
            }
            else
            {
                this.imageHostSource?.SetGraphicItem(null);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.UpdateProvider(this.FilePath, null);
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
                this.imageHost.Source = null;
            }

            this.imageHost = (Image)this.GetTemplateChild(ImageHostPart);
            this.imageHostSource = new GraphicsItemD3DImage();
            this.imageHost.Source = this.imageHostSource;
            this.UpdateSource();
        }

        private void UpdateProvider(string oldValue, string newValue)
        {
            this.IsFailed = false;

            this.powerPointGraphicsCaptureItemsFactory.Remove(oldValue);
            if (this.currentGraphicsItem != null)
            {
                this.currentGraphicsItem.Closed -= Next_Closed;
            }

            if (App.Current?.MainWindow == null)
            {
                return;
            }

            var next = this.powerPointGraphicsCaptureItemsFactory.GetOrCreateGraphicsCaptureItem(newValue);
            if (next == null && newValue != null)
            {
                this.IsFailed = true;
            }
            else if (next != null)
            {
                next.Closed += Next_Closed;
            }

            this.currentGraphicsItem = next;
                this.UpdateSource();
        }

        private void Next_Closed(GraphicsCaptureItem sender, object args)
        {
            sender.Closed -= Next_Closed;
            this.UpdateProvider(null, this.FilePath);
        }

        private void UpdateSource()
        {
            if (this.IsVisible && this.imageHostSource != null)
            {
                this.imageHostSource.FpsDivider = this.FpsDivider;
                this.imageHostSource.RenderWidth = (int)(double.IsNaN(this.RenderWidth) ? -1 : this.RenderWidth);
                this.imageHostSource.RenderHeight = (int)(double.IsNaN(this.RenderHeight) ? -1 : this.RenderHeight);
                this.imageHostSource?.SetGraphicItem(this.currentGraphicsItem);
            }
        }

        private static void FilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PowerPointControl)d).UpdateProvider((string)e.OldValue, (string)e.NewValue);
        }

        private static void ProviderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PowerPointControl)d).UpdateSource();
        }
    }
}
