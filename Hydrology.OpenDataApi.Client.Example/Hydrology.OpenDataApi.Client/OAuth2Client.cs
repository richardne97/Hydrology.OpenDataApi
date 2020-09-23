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

        private string _senslinkUserName, _senslinkPassword;

        #endregion

        #region Public Struct

        /// <summary>
        /// OAuth2 Token 資料結構
        /// </summary>
        public class AccessToken
        {
            public string access_token;
            public long expires_in;
            public string refresh_token;
            public string token_type;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// OAuth2 Authentication Code
        /// </summary>
        /// <param name="baseUri">for senslink 3.0, http://{root}/v3/oauth2 </param>
        public OAuth2Client(Uri baseUri, Uri redirectUri, string clientId, string clientSecret, string senslinkUserName, string senslinkPassword)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (!baseUri.AbsoluteUri.StartsWith("https"))
                throw new Exception("Only support https protocol");

            _authType = AuthTypes.AuthorizationCode;
            _baseUri = baseUri;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
            _senslinkPassword = senslinkPassword;
            _senslinkUserName = senslinkUserName;

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = _baseUri; //http://{root}/v3/oauth2

            // We want the response to be JSON.
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// OAuth2 Client Credential
        /// </summary>
        /// <param name="baseUri">for senslink 3.0, http://{root}/v3/oauth2 </param>
        public OAuth2Client(Uri baseUri, string clientId, string clientSecret)
        {
            _authType = AuthTypes.ClientCredential;
            _baseUri = baseUri;
            _clientId = clientId;
            _clientSecret = clientSecret;

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = _baseUri; //http://{root}/v3/oauth2
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
        /// <param name="senslinkUserName"></param>
        /// <param name="senslinkPassword"></param>
        /// <returns></returns>
        private string GetCode()
        {
            //本機測試時需加上這段，避免SSL認證問題
            ServicePointManager.ServerCertificateValidationCallback =
                    delegate { return true; };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

            //Login
            Dictionary<string, string> keys = new Dictionary<string, string>();
            keys.Add("username", _senslinkUserName);
            keys.Add("password", _senslinkPassword);
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
                {
                    return queryObj["code"].ToString();
                }
            }
            return null;
        }

        /// <summary>
        /// 取得 AccessToken
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public AccessToken GetAccessToken(bool forceUpdate)
        {
            if (_cunTokenExpireTime.AddMinutes(-1) < DateTime.Now || forceUpdate)
            {
                if (_authType == AuthTypes.AuthorizationCode)
                {
                    string code = GetCode();
                    if (code == string.Empty || code == null)
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
        /// <param name="code"></param>
        /// <param name="redirect_uri"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
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
        /// 使用 clientId 及 clientSecret 取得 Token
        /// </summary>
        /// <param name="foreceUpdate">是否強迫重新取得 Token，若為false，則Token失效時會自動取得</param>
        /// <returns></returns>
        private AccessToken GetAccessTokenClientCredential()
        {
            // Build up the data to POST.
            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            postData.Add(new KeyValuePair<string, string>("client_id", _clientId));
            postData.Add(new KeyValuePair<string, string>("client_secret", _clientSecret));

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

        public AccessToken RefreshAccessToken(string refreshToken)
        {
            // Build up the data to POST.
            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
            postData.Add(new KeyValuePair<string, string>("refresh_token", refreshToken));
            //postData.Add(new KeyValuePair<string, string>("scope", ""));

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
