using Microsoft.Practices.Unity;
using SecureFileShare.App.ViewModels.Help;
using SecureFileShare.App.Views.Interfaces;

namespace SecureFileShare.App.Views.Help
{
    /// <summary>
    ///     Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView : ICloseable
    {
        public HelpView()
        {
            InitializeComponent();
        }

        [Dependency]
        public HelpViewModel ViewModel
        {
            set { DataContext = value; }
        }
    }
}