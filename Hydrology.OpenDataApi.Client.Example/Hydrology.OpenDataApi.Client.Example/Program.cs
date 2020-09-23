using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Hydrology.OpenDataApi.Client;
using Hydrology.OpenDataApi.Model;
using Newtonsoft.Json;

namespace Hydrology.OpenDataApi.Client.Example
{
    class Program
    {
        static HodApiClient _hodApiClient;
        static RiverStationInfo[] _riverStationInfos;
        static UswgStationInfo[] _uswgStationInfos;

        //水利署水文開放資料 API 伺服器地址
        static Uri _hydrologyServerUri = new Uri("http://192.168.12.11:88/");
        static Uri _hydrologyOAuth2ServerUri = new Uri("http://localhost:33904/oauth2/");

        static OAuth2Client _oauth2Client;

        static void Main(string[] args)
        {
            _oauth2Client = new OAuth2Client(_hydrologyOAuth2ServerUri, "rFAnRsHF894Z8lZ36P4oOGIv1hr4J7rXBABr6lf/O7s=", "3D2FlYlLIVwWIpR7JBe3IGw5k/PGw9Q93RSmCkmNOug=");
            _hodApiClient = new HodApiClient(_hydrologyServerUri, _oauth2Client);

            //取得縣市代碼
            _hodApiClient.GetAdminDevisionsCountyInfo(out CountyInfo[] countyInfos);
            Console.WriteLine(JsonConvert.SerializeObject(countyInfos, Formatting.Indented));
            
            //取得鄉鎮代碼
            _hodApiClient.GetAdminDevisionsTownInfo(out TownInfo[] townInfos);
            Console.WriteLine(JsonConvert.SerializeObject(townInfos, Formatting.Indented));

            //取得水系代碼
            _hodApiClient.GetRiverBasinsInfo(out BasinInfo[] basinInfos);
            Console.WriteLine(JsonConvert.SerializeObject(basinInfos, Formatting.Indented));
            
            //使用水系代碼取得河川監測站
            _hodApiClient.GetRiverStations(HodApiClient.RiverStationParameterTypes.basinCode, "165000", out _riverStationInfos);
            Console.WriteLine(JsonConvert.SerializeObject(_riverStationInfos, Formatting.Indented));

            //使用水系名稱取得河川監測站
            _hodApiClient.GetRiverStations(HodApiClient.RiverStationParameterTypes.basinCode, "鹽水溪", out _riverStationInfos);
            Console.WriteLine(JsonConvert.SerializeObject(_riverStationInfos, Formatting.Indented));

            //使用經緯度中心，半徑取的範圍內的河川監測站
            _hodApiClient.GetRiverStations(23.003868, 120.226729, 5000, out _riverStationInfos);
            Console.WriteLine(JsonConvert.SerializeObject(_riverStationInfos, Formatting.Indented));
            
            //使用縣市名稱取得都市淹水感知站
            _hodApiClient.GetUswgs(HodApiClient.UswgStationParameterTypes.countyName, "臺南市", out _uswgStationInfos);
            Console.WriteLine(JsonConvert.SerializeObject(_uswgStationInfos, Formatting.Indented));

            //使用經緯度中心，半徑取的範圍內的都市淹水感知站
            _hodApiClient.GetUswgs(23.003868, 120.226729, 5000, out _uswgStationInfos);
            Console.WriteLine(JsonConvert.SerializeObject(_uswgStationInfos, Formatting.Indented));
        }
    }
}
