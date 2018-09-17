using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SnipInsight.Forms.Common
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool isBusy = false;

        public BaseViewModel()
        {
            this.OpenUrlCommand = new Command<string>(this.OpenUrl);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public ICommand OpenUrlCommand { get; }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName]string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                return false;
            }

            backingStore = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected async Task<bool> InvokeWithErrorHandling(Task task, Func<Exception, Task<bool>> errorHandler = null)
        {
            Task<object> innerTask = Task.Run<object>(async () =>
            {
                await task.ConfigureAwait(false);
                return Task.FromResult<object>(null);
            });

            (object result, bool succeeded) r = await this.InvokeWithErrorHandling(innerTask, errorHandler).ConfigureAwait(false);

            return await Task.FromResult(r.succeeded);
        }

        protected async Task<(T result, bool succeeded)> InvokeWithErrorHandling<T>(Task<T> task, Func<Exception, Task<bool>> errorHandler = null)
        {
            var requestResult = default(T);
            var succeeded = false;

            try
            {
                requestResult = await task.ConfigureAwait(false);
                succeeded = true;
            }
            catch (Exception ex)
            {
                if (errorHandler == null || !await errorHandler?.Invoke(ex))
                {
                    Debug.WriteLine($"Exception: {ex}");
                }
            }

            return (requestResult, succeeded);
        }

        private void OpenUrl(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    var uri = new Uri(url);
                    Device.OpenUri(uri);
                }
                catch (UriFormatException)
                {
                    Debug.WriteLine($"Invalid url: {url}");
                }
            }
        }
    }
}