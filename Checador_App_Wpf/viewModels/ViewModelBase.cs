using System.ComponentModel;
using System.Runtime.CompilerServices;
using ControlDeCheckeo.Helpers;

namespace ControlDeCheckeo.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propName = "")
        {
            if (!Equals(backingField, value))
            {
                backingField = value;
                OnPropertyChanged(propName);
                return true;
            }
            return false;
        }
    }
}
