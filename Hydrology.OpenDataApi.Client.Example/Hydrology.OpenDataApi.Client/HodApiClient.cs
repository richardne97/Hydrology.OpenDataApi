using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Hydrology.OpenDataApi.Model;

namespace Hydrology.OpenDataApi.Client
{
    /// <summary>
    /// 水利署水文資料開放Api 客戶端連接物件
    /// </summary>
    public class HodApiClient
    {
        private HttpClient _httpClient = null;
        private OAuth2Client _oauth2Client;

        public enum UswgStationParameterTypes { countyName, townName, countyCode, townCode }
        public enum RiverStationParameterTypes { countyName, townName, countyCode, townCode, basinName, basinCode }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="baseAddress">水利署水文開放Api，伺服器地址</param>
        /// <param name="oauth2Client">水利署水文開放Api OAuth2 認證</param>
        public HodApiClient(Uri baseAddress, OAuth2Client oauth2Client)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = baseAddress
            };
            _oauth2Client = oauth2Client;
        }

        /// <summary>
        /// 取得縣市代碼
        /// </summary>
        /// <param name="countyInfos">回傳結果物件，若失敗為null</param>
        /// <returns></returns>
        public bool GetAdminDevisionsCountyInfo(out CountyInfo[] countyInfos)
        {
            countyInfos = null;
            if(HttpGet("adminDivisions/county", null, out string responseString) == HttpStatusCode.OK)
            {
                try
                {
                    countyInfos = JsonConvert.DeserializeObject<CountyInfo[]>(responseString);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 取得鄉鎮代碼
        /// </summary>
        /// <param name="townInfos">回傳結果物件，若失敗為null</param>
        /// <returns></returns>
        public bool GetAdminDevisionsTownInfo(out TownInfo[] townInfos)
        {
            townInfos = null;
            if (HttpGet("adminDivisions/town", null, out string responseString) == HttpStatusCode.OK)
            {
                try
                {
                    townInfos = JsonConvert.DeserializeObject<TownInfo[]>(responseString);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 取得都市淹水感知器監測站資訊
        /// </summary>
        /// <param name="uswgParamterType">給定參數種類</param>
        /// <param name="parameterValue">參數數值</param>
        /// <param name="uswgStationInfos">回傳結果，若失敗為null</param>
        /// <returns>擷取成功或失敗</returns>
        public bool GetUswgs(UswgStationParameterTypes uswgParamterType, string parameterValue, out UswgStationInfo[] uswgStationInfos)
        {
            uswgStationInfos = null;
            KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>(uswgParamterType.ToString(), parameterValue),
            };

            if(HttpGet("uswg/stations", parameters, out string responseString) == HttpStatusCode.OK)
            {
                uswgStationInfos = JsonConvert.DeserializeObject<UswgStationInfo[]>(responseString);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 使用空間分析取得都市淹水感知器監測站資訊
        /// </summary>
        /// <param name="latitude">搜尋中心緯度</param>
        /// <param name="longtitude">搜尋中心經度</param>
        /// <param name="radius">搜尋半徑</param>
        /// <param name="uswgStationInfos">回傳結果物件，若失敗為null</param>
        /// <returns></returns>
        public bool GetUswgs(double latitude, double longtitude, double radius, out UswgStationInfo[] uswgStationInfos)
        {
            uswgStationInfos = null;
            KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("centerLat", latitude.ToString()),
                new KeyValuePair<string, string>("centerLong", longtitude.ToString()),
                new KeyValuePair<string, string>("radius", radius.ToString()),
            };

            if (HttpGet("uswg/stations", parameters, out string responseString) == HttpStatusCode.OK)
            {
                uswgStationInfos = JsonConvert.DeserializeObject<UswgStationInfo[]>(responseString);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 取得河川代碼
        /// </summary>
        /// <param name="basinInfos">回傳結果物件，若失敗為null</param>
        /// <returns></returns>
        public bool GetRiverBasinsInfo(out BasinInfo[] basinInfos)
        {
            basinInfos = null;
            if (HttpGet("river/basins", null, out string responseString) == HttpStatusCode.OK)
            {
                try
                {
                    basinInfos = JsonConvert.DeserializeObject<BasinInfo[]>(responseString);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 取得河川監測站資訊
        /// </summary>
        /// <param name="riverStationParamterType">給定查詢參數種類</param>
        /// <param name="parameterValue">參數數值</param>
        /// <param name="riverStationInfos">回傳結果物件，若失敗為null</param>
        /// <returns></returns>
        public bool GetRiverStations(RiverStationParameterTypes riverStationParamterType, string parameterValue, out RiverStationInfo[] riverStationInfos)
        {
            riverStationInfos = null;
            KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>(riverStationParamterType.ToString(), parameterValue),
            };

            if (HttpGet("river/stations", parameters, out string responseString) == HttpStatusCode.OK)
            {
                riverStationInfos = JsonConvert.DeserializeObject<RiverStationInfo[]>(responseString);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 使用空間分析取得河川水位站資訊
        /// </summary>
        /// <param name="latitude">搜尋中心緯度</param>
        /// <param name="longtitude">搜尋中心經度</param>
        /// <param name="radius">搜尋半徑</param>
        /// <param name="riverStationInfos">回傳結果物件，若失敗為null</param>
        /// <returns></returns>
        public bool GetRiverStations(double latitude, double longtitude, double radius, out RiverStationInfo[] riverStationInfos)
        {
            riverStationInfos = null;
            KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("centerLat", latitude.ToString()),
                new KeyValuePair<string, string>("centerLong", longtitude.ToString()),
                new KeyValuePair<string, string>("radius", radius.ToString()),
            };

            if (HttpGet("river/stations", parameters, out string responseString) == HttpStatusCode.OK)
            {
                riverStationInfos = JsonConvert.DeserializeObject<RiverStationInfo[]>(responseString);
                return true;
            }
            return false;
        }

        #region Http Actions 

        private HttpStatusCode HttpPost(string additionalUri, string body, out string responseString)
        {
            OAuth2Client.AccessToken ac = _oauth2Client.GetAccessToken(false);
            if (ac == null)
            {
                responseString = null;
                return HttpStatusCode.BadRequest;
            }

            responseString = string.Empty;
            HttpStatusCode responseCode = HttpStatusCode.BadRequest;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _oauth2Client.GetAccessToken(false).access_token);

            // Build up the data to POST.
            StringContent content = new StringContent(body);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            try
            {
                HttpResponseMessage message = _httpClient.PostAsync(additionalUri, content).ConfigureAwait(false).GetAwaiter().GetResult();
                responseCode = message.StatusCode;
                responseString = message.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                if (!message.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Post fail {message.StatusCode} {responseString}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return responseCode;
        }

        private HttpStatusCode HttpGet(string additionalUri, KeyValuePair<string, string>[] parameters, out string responseString)
        {
            OAuth2Client.AccessToken ac = null;

            if (_oauth2Client != null)
            {
                ac = _oauth2Client.GetAccessToken(false);
                if (ac == null)
                {
                    responseString = null;
                    return HttpStatusCode.BadRequest;
                }
                // We want the response to be JSON.
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ac.access_token);
            }
            responseString = string.Empty;

            if(parameters != null)
            {
                StringBuilder sb = new StringBuilder();
                string[] parameterPairs = parameters.Select(p => $"{p.Key}={p.Value}").ToArray();
                additionalUri = $"{additionalUri}?{String.Join("&", parameterPairs)}";
            }

            HttpResponseMessage message = null;
            try
            {
                message = _httpClient.GetAsync(additionalUri).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Request Get fail: {_httpClient.BaseAddress}{additionalUri} {ex.Message}");
                return HttpStatusCode.BadRequest;
            }

            if (message.IsSuccessStatusCode)
            {
                try
                {
                    responseString = message.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    return message.StatusCode;
                }
                catch { }
            }
            return HttpStatusCode.BadRequest;
        }

        #endregion

    }
}
