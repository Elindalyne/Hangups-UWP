using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace Hangups.Core
{
    public static class Auth
    {
        private static string oAuth_Scope = "https://www.google.com/accounts/OAuthLogin";
        private static string client_secret = "KWsJlkaMn1jGLxQpWxMnOox-";
        private static string client_id = "936475272427.apps.googleusercontent.com";
        private static string redirect_uri = "urn:ietf:wg:oauth:2.0:oob";
        private static string login_url = "https://accounts.google.com/o/oauth2/auth?{0}";
        private static string token_request_url = "https://accounts.google.com/o/oauth2/token";

       
        public static void PerformOauth()
        {
            var refreshToken = SettingsHelper.GetStringSetting("refresh_token");

            if(refreshToken != String.Empty)
            {
                PerformRefreshTokenOAuth(refreshToken);
            }
            else
            {
                PerformGoogleOAuth();
            }
        }

        private async static void PerformGoogleOAuth()
        {
            var startUri = new Uri(string.Format(login_url, formatLoginURL()));
            Uri EndUri = new Uri("https://accounts.google.com/o/oauth2/approval?");
            try
            {
                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.UseTitle, startUri, EndUri);
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    PerformTokenAuth(WebAuthenticationResult.ResponseData.ToString().Split('=')[1]);
                }
                else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    NotificationHelper.ErrorMessage("The result returned Error " + WebAuthenticationResult.ResponseErrorDetail.ToString(), "Authentication Failure");
                }
                else
                {
                    NotificationHelper.ErrorMessage("An unknown error has occurred", "Authentication Failed");
                }
            }


            catch (Exception Error)
            {
                NotificationHelper.ErrorMessage(Error.Message, "Authentication Failed");
            }
        }

        private async static void PerformTokenAuth(string authToken)
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
            var accessToken = (string)returnedData.SelectToken("access_token");
            SettingsHelper.StoreSetting("access_token", accessToken);
            SettingsHelper.StoreSetting("refresh_token", (string)returnedData.SelectToken("refresh_token"));
            GetCookies(accessToken);
        }

        private async static void PerformRefreshTokenOAuth(string refreshToken)
        {
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
            var accessToken = (string)returnedData.SelectToken("access_token");
            SettingsHelper.StoreSetting("access_token", accessToken);
            GetCookies(accessToken);
        }

        private async static void GetCookies(string accessToken)
        {
            HttpClient aClient = new HttpClient();
            aClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            var r = await aClient.GetAsync("https://accounts.google.com/accounts/OAuthLogin?source=hangups&issueuberauth=1");
            r.EnsureSuccessStatusCode();
            var uberAuth = await r.Content.ReadAsStringAsync();
            r = await aClient.GetAsync(String.Format("https://accounts.google.com/MergeSession?service=mail&continue=http://www.google.com&uberauth={0}", uberAuth));
            r.EnsureSuccessStatusCode();
            IEnumerable<string> cookies;
            var responseCookies = new CookieContainer();
            if (r.Headers.TryGetValues("set-cookie", out cookies))
            {
                foreach (var c in cookies)
                {
                    responseCookies.SetCookies(r.RequestMessage.RequestUri, c);
                }
            }
        }

        private static string formatLoginURL()
        {
            return "client_id=" + Uri.EscapeDataString(client_id) + "&redirect_uri=" + Uri.EscapeDataString(redirect_uri) + "&response_type=code&scope=" + Uri.EscapeDataString(oAuth_Scope);
        }
    }
}
