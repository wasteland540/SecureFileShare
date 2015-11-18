using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dropbox.Api;

namespace DropboxTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string appKey = "zkmv3u3ic34dnta";
        public MainWindow()
        {
            InitializeComponent();


        }

        private const string RedirectUri = "https://www.dropbox.com/1/oauth2/authorize";
        private string oauth2State;

        public string AccessToken { get; private set; }
        public string UserId { get; private set; }

        private void WebBrowser_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (!e.Uri.ToString().StartsWith(RedirectUri, StringComparison.OrdinalIgnoreCase))
            {
                // we need to ignore all navigation that isn't to the redirect uri.
                return;
            }

            try
            {
                OAuth2Response result = DropboxOAuth2Helper.ParseTokenFragment(e.Uri);
                if (result.State != this.oauth2State)
                {
                    // The state in the response doesn't match the state in the request.
                    return;
                }

                this.AccessToken = result.AccessToken;
                this.Uid = result.Uid;
            }
            catch (ArgumentException)
            {
                // There was an error in the URI passed to ParseTokenFragment
            }
            finally
            {
                e.Cancel = true;
                this.Close();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.oauth2State = Guid.NewGuid().ToString("N");
            Uri authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, appKey, RedirectUri, state: oauth2State);
            this.Browser.Navigate(authorizeUri);
        }
    }
}
