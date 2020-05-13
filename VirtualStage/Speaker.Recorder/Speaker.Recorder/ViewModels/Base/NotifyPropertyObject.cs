using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Speaker.Recorder.ViewModels.Base
{
    public abstract class NotifyPropertyObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName]string propertyName = "")
        {
            if (this.AreEquals(ref backingStore, ref value, propertyName))
            {
                return false;
            }

            backingStore = value;
            this.OnPropertyChanged(propertyName);

            return true;
        }

        protected void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (this.PropertyChanged == null)
            {
                return;
            }

            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool AreEquals<T>(ref T backingStore, ref T value, string propertyName)
        {
            return EqualityComparer<T>.Default.Equals(backingStore, value);
        }
    }
}
