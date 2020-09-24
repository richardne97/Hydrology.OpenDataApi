using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Net;

namespace Hydrology.OpenDataApi.Client
{
    /// <summary>
    /// OAuth2 認證客戶端
    /// </summary>
    public class OAuth2Client
    {
        #region Private Fields

        protected Uri _baseUri;
        protected Uri _redirectUri;
        protected string _clientId;
        protected string _clientSecret;
        protected DateTime _cunTokenExpireTime = DateTime.Now;
        protected AccessToken _cunAccessToken;

        private AuthTypes _authType;
        private HttpClient _httpClient;

        private enum AuthTypes { ClientCredential, AuthorizationCode }

        private readonly string _userName;
        private readonly string _password;

        #endregion

        #region Public Struct

        /// <summary>
        /// OAuth2 Token 資料結構
        /// </summary>
        public class AccessToken
        {
            /// <summary>
            /// Token 內容
            /// </summary>
            public string access_token;
            /// <summary>
            /// Token 有效期間長度，單位分鐘
            /// </summary>
            public long expires_in;
            /// <summary>
            /// Refresh Token
            /// </summary>
            public string refresh_token;
            /// <summary>
            /// Token 型態
            /// </summary>
            public string token_type;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// OAuth2 客戶端，使用 Authorization Code認證
        /// </summary>
        /// <param name="clientId">Client Id</param>
        /// <param name="clientSecret">Client Secret</param>
        /// <param name="userName">水文開放資料平台帳號</param>
        /// <param name="password">水文開放資料平台密碼</param>
        /// <param name="baseUri">OAuth2 Server 網址, 範例 http://{root}/oauth2/ </param>
        public OAuth2Client(Uri baseUri, Uri redirectUri, string clientId, string clientSecret, string userName, string password)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (!baseUri.AbsoluteUri.StartsWith("https"))
                throw new Exception("Only support https protocol");

            _authType = AuthTypes.AuthorizationCode;
            _baseUri = baseUri;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
            _password = password;
            _userName = userName;

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = _baseUri; //http://{root}/oauth2/

            // We want the response to be JSON.
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// OAuth2 客戶端，使用 Client Credential 認證
        /// </summary>
        /// <param name="clientId">Client Id</param>
        /// <param name="clientSecret">Client Secret</param>
        /// <param name="baseUri">OAuth2 Server 網址, 範例 http://{root}/oauth2/</param>
        public OAuth2Client(Uri baseUri, string clientId, string clientSecret)
        {
            _authType = AuthTypes.ClientCredential;
            _baseUri = baseUri;
            _clientId = clientId;
            _clientSecret = clientSecret;

            _httpClient = new HttpClient
            {
                BaseAddress = _baseUri
            };

            // We want the response to be JSON.
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 取得或設定 Client Id
        /// </summary>
        public string ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }

        /// <summary>
        /// 取得或設定 Client Secret
        /// </summary>
        public string ClientSecret
        {
            get { return _clientSecret; }
            set { _clientSecret = value; }
        }

        /// <summary>
        /// 目前 Token 失效時間
        /// </summary>
        public DateTime CurrentTokenExipreTime
        {
            get { return _cunTokenExpireTime; }
        }

        /// <summary>
        /// 模擬 Browser，使用senslinkUserName, senslinkPassword 取得 Authentication Code
        /// </summary>
        /// <returns></returns>
        private string GetCode()
        {
            //忽略SSL自簽認證問題
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

            //Login
            Dictionary<string, string> keys = new Dictionary<string, string>();
            keys.Add("username", _userName);
            keys.Add("password", _password);
            keys.Add("isPersistent", "True");
            keys.Add("submit.Signin", "submit.Signin");
            FormUrlEncodedContent content = new FormUrlEncodedContent(keys);

            HttpResponseMessage message = _httpClient.PostAsync("Account/login", content).ConfigureAwait(false).GetAwaiter().GetResult();
            string responseString = message.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            //Grant
            string grantUri = $"authorize?response_type=code&state=&client_id={HttpUtility.UrlEncode(_clientId)}&scope=&redirect_uri={_redirectUri}";
            keys.Clear();
            keys.Add("submit.Grant", "submit.Grant");
            content = new FormUrlEncodedContent(keys);

            message = _httpClient.PostAsync(grantUri, content).ConfigureAwait(false).GetAwaiter().GetResult();

            if (message.RequestMessage.RequestUri.TryReadQueryAsJson(out JObject queryObj))
            {
                if (queryObj.ContainsKey("code"))
                    return queryObj["code"].ToString();
            }
            return null;
        }

        /// <summary>
        /// 取得 AccessToken
        /// </summary>
        /// <param name="forceUpdate">強迫由伺服器取得Token，而非上次存在記憶體中的Cache，若為false，則Token失效時會自動取得</param>
        /// <returns></returns>
        public AccessToken GetAccessToken(bool forceUpdate)
        {
            if (_cunTokenExpireTime.AddMinutes(-1) < DateTime.Now || forceUpdate)
            {
                if (_authType == AuthTypes.AuthorizationCode)
                {
                    string code = GetCode();
                    if (string.IsNullOrEmpty(code) || code == null)
                        return null;

                    return GetAccessTokenAuthorizationCode(code);
                }
                else
                    return GetAccessTokenClientCredential();
            }
            return _cunAccessToken;
        }

        #endregion

        /// <summary>
        /// 取得Token，使用Authorization Code方法，使用登入認證後取得的Code，配合其他參數取得Token
        /// </summary>
        /// <param name="code">Authorization Code</param>
        /// <returns></returns>
        private AccessToken GetAccessTokenAuthorizationCode(string code)
        {
            FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", _redirectUri.ToString()),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret)
            });

            HttpResponseMessage message = _httpClient.PostAsync("token", content).ConfigureAwait(false).GetAwaiter().GetResult();
            string responseString = message.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            AccessToken tokenResponse = null;
            try
            {
                tokenResponse = JsonConvert.DeserializeObject<AccessToken>(responseString);
            }
            catch { }
            if (tokenResponse != null)
                return tokenResponse;
            return null;
        }

        /// <summary>
        /// 使用 clientId 及 clientSecret 執行 Client Credential 流程取得 Token
        /// </summary>
        /// <returns></returns>
        private AccessToken GetAccessTokenClientCredential()
        {
            // Build up the data to POST.
            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret)
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(postData);
            HttpResponseMessage message = _httpClient.PostAsync("token", content).ConfigureAwait(false).GetAwaiter().GetResult();

            if (message.IsSuccessStatusCode)
            {
                string responseString = message.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                try
                {
                    _cunAccessToken = JsonConvert.DeserializeObject<AccessToken>(responseString);
                    _cunTokenExpireTime = DateTime.Now.AddSeconds(_cunAccessToken.expires_in);
                }
                catch { }
                return _cunAccessToken;
            }
            return null;
        }

        /// <summary>
        /// Refresh Token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public AccessToken RefreshAccessToken(string refreshToken)
        {
            // Build up the data to POST.
            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(postData);

            HttpResponseMessage message = _httpClient.PostAsync("token", content).ConfigureAwait(false).GetAwaiter().GetResult();
            string responseString = message.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            try
            {
                AccessToken responseData = JsonConvert.DeserializeObject<AccessToken>(responseString);
                return responseData;
            }
            catch { }
            return null;
        }
    }
}
