using System;
using System.Windows.Input;

namespace Speaker.Recorder.ViewModels.Base
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> executedMethod;
        private readonly Func<T, bool> canExecuteMethod;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            this.executedMethod = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecuteMethod = canExecute;
        }

        public bool CanExecute(object parameter) => this.canExecuteMethod == null || this.canExecuteMethod((T)parameter);

        public void Execute(object parameter) => this.executedMethod((T)parameter);

        public void OnCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
