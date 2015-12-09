using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System.Net;
using System.Dynamic;
using Hangups.HangupsSystem;
using Newtonsoft.Json.Linq;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Hangups
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static string oAuth_Scope = "https://www.google.com/accounts/OAuthLogin";
        private static string client_secret = "KWsJlkaMn1jGLxQpWxMnOox-";
        private static string client_id = "936475272427.apps.googleusercontent.com";
        private static string redirect_uri = "urn:ietf:wg:oauth:2.0:oob";
        private static string login_url = "https://accounts.google.com/o/oauth2/auth?{0}";
        private static string token_request_url = "https://accounts.google.com/o/oauth2/token";

        private string access_token;
        private CookieContainer responseCookies;

        public MainPage()
        {
            this.InitializeComponent();
            if(loadRefreshToken() != string.Empty)
            {
                AuthenticateWithRefreshToken();
            }
            else
            {
                AuthenticateWithGoogle();
            }
        }

        private async void AuthenticateWithGoogle()
        {
            var startUri = new Uri(string.Format(login_url, formatLoginURL()));
            Uri EndUri = new Uri("https://accounts.google.com/o/oauth2/approval?");
            try
            {
                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.UseTitle, startUri, EndUri);
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    AuthenticateWithToken(WebAuthenticationResult.ResponseData.ToString().Split('=')[1]);
                }
                else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    //
                }
                else
                {
                    //
                }
            }


            catch (Exception Error)
            {
               // rootPage.NotifyUser(Error.Message, NotifyType.ErrorMessage);
            } 
}

        private async void AuthenticateWithRefreshToken()
        {
            var refreshToken = loadRefreshToken();
            HttpClient aClient = new HttpClient();
            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", client_id),
                new KeyValuePair<string, string>("client_secret", client_secret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });
            var response = await aClient.PostAsync(token_request_url, body);
            response.EnsureSuccessStatusCode();

            var resultContent = await response.Content.ReadAsStringAsync();
            JObject returnedData = JObject.Parse(resultContent);
            access_token = (string)returnedData.SelectToken("access_token"); // This is what we do auth with. I will probably store this somewhere
            GetSessionCookies();
        }

        private async void GetSessionCookies()
        {
            HttpClient aClient = new HttpClient();
            aClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", access_token));
            var r = await aClient.GetAsync("https://accounts.google.com/accounts/OAuthLogin?source=hangups&issueuberauth=1");
            r.EnsureSuccessStatusCode();
            var uberAuth = await r.Content.ReadAsStringAsync();
            r = await aClient.GetAsync(String.Format("https://accounts.google.com/MergeSession?service=mail&continue=http://www.google.com&uberauth={0}", uberAuth));
            r.EnsureSuccessStatusCode();
            IEnumerable<string> cookies;
            responseCookies = new CookieContainer();
            if (r.Headers.TryGetValues("set-cookie", out cookies))
            {
                foreach (var c in cookies)
                {
                    responseCookies.SetCookies(r.RequestMessage.RequestUri, c);
                }
            }
        }

        private async void AuthenticateWithToken(string authToken)
        {
            HttpClient aClient = new HttpClient();
            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", client_id),
                new KeyValuePair<string, string>("client_secret", client_secret),
                new KeyValuePair<string, string>("code", authToken),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", redirect_uri),
            });

            var response = await aClient.PostAsync(token_request_url, body);
            response.EnsureSuccessStatusCode();

            var resultContent = await response.Content.ReadAsStringAsync();
            JObject returnedData = JObject.Parse(resultContent);
            access_token = (string)returnedData.SelectToken("access_token"); // This is what we do auth with. I will probably store this somewhere
            saveRefreshToken((string)returnedData.SelectToken("refresh_token"));
            GetSessionCookies();
        }

        private string formatLoginURL()
        {
            return "client_id=" + Uri.EscapeDataString(client_id) + "&redirect_uri=" + Uri.EscapeDataString(redirect_uri) + "&response_type=code&scope=" + Uri.EscapeDataString(oAuth_Scope);
        }

        private string loadRefreshToken() {
            return SettingsHelper.GetStringSetting("refresh_token");
        }

        private void saveRefreshToken(string token) {
            SettingsHelper.StoreSetting("refresh_token", token);
        }
    }
}
