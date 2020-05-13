using Microsoft.Extensions.DependencyInjection;
using Speaker.Recorder.PowerPoint;
using Speaker.Recorder.Services;
using Speaker.Recorder.ViewModels;
using Speaker.Recorder.ViewModels.Base;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Speaker.Recorder
{
    public partial class MainWindow : Window, INavigationService
    {
        private readonly Application application;
        private readonly PowerPointGraphicsCaptureItemsFactory pointGraphicsCaptureItemsFactory;
        private readonly IServiceProvider serviceProvider;

        public MainWindow(Application application, PowerPointGraphicsCaptureItemsFactory pointGraphicsCaptureItemsFactory, IServiceProvider serviceProvider)
        {
            this.application = application;
            this.pointGraphicsCaptureItemsFactory = pointGraphicsCaptureItemsFactory;
            this.serviceProvider = serviceProvider;
            this.InitializeComponent();
            this.Title += $" {Assembly.GetExecutingAssembly().GetName().Version}";
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            this.pointGraphicsCaptureItemsFactory.SendKey(e.Key);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            foreach (var item in this.MainContent.Children)
            {
                if (item is ContentControl element && element.Content is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.application.MainWindow = this;
                    this.To<RecordViewModel>();
                    this.Show();
                    this.Activate();
                }).Task;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.pointGraphicsCaptureItemsFactory.CloseAll();
            return Task.CompletedTask;
        }

        public void To<TViewModel>()
        {
            var viewModel = serviceProvider.GetService<TViewModel>();
            if (this.MainContent.Children.Count > 0)
            {
                var lastView = this.MainContent.Children[^1];
                lastView.Visibility = Visibility.Collapsed;
                if (lastView is ContentControl element && element.Content is IPausableViewModel pausable)
                {
                    pausable.Pause();
                }
            }

            this.MainContent.Children.Add(new ContentControl { Content = viewModel });
        }

        public void Back()
        {
            var currentView = this.MainContent.Children[^1];
            if (currentView is ContentControl element && element.Content is IDisposable disposable)
            {
                disposable.Dispose();
            }

            this.MainContent.Children.Remove(currentView);
            if (this.MainContent.Children.Count > 0)
            {
                var lastView = this.MainContent.Children[^1];
                lastView.Visibility = Visibility.Visible;
                if (lastView is ContentControl lastElement && lastElement.Content is IPausableViewModel pausable)
                {
                    pausable.Resume();
                }
            }
        }
    }
}
