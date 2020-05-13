using System;
using System.Windows.Input;

namespace Speaker.Recorder.ViewModels.Base
{
    public class RelayCommand : ICommand
    {
        private readonly Action executedMethod;
        private readonly Func<bool> canExecuteMethod;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            this.executedMethod = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecuteMethod = canExecute;
        }

        public bool CanExecute(object parameter) => this.canExecuteMethod == null || this.canExecuteMethod();

        public void Execute(object parameter) => this.executedMethod();

        public void OnCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
