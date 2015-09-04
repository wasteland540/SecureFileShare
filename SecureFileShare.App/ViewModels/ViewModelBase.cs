using System.ComponentModel;
using System.Windows;
using Microsoft.Practices.Unity;

namespace SecureFileShare.App.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected IUnityContainer Container;

        protected ViewModelBase()
        {
            var app = (App) Application.Current;
            Container = app.Container;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}