using Microsoft.Practices.Unity;
using SecureFileShare.App.ViewModels;
using SecureFileShare.App.Views.Interfaces;

namespace SecureFileShare.App.Views
{
    /// <summary>
    ///     Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : ICloseable
    {
        public LoginView()
        {
            InitializeComponent();
        }

        [Dependency]
        public LoginViewModel ViewModel
        {
            set { DataContext = value; }
        }
    }
}