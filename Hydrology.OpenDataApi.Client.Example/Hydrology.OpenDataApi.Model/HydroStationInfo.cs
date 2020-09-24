﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydrology.OpenDataApi.Model
{
    public abstract class HydroStationInfo
    {
        [JsonIgnore()]
        protected HydroStationTypes _stationType;

        /// <summary>
        /// 水情監測站型態
        /// </summary>
        public enum HydroStationTypes 
        {
            /// <summary>
            /// 都市淹水感知站
            /// </summary>
            Uswg, 
            /// <summary>
            /// 河川水位站
            /// </summary>
            River, 
            /// <summary>
            /// 雨量站
            /// </summary>
            Precipitation, 
            /// <summary>
            /// 其他
            /// </summary>
            Other 
        };

        /// <summary>
        /// 水資源物聯網監測站Id
        /// </summary>
        [JsonProperty(Order = 0)]
        public Guid IoWStationId { get; set; }

        /// <summary>
        /// 監測站Id，業管單位依照實際需求所給定之編號
        /// </summary>
        [JsonProperty(Order = 1)]
        public string StationId { get; set; }

        /// <summary>
        /// 名稱
        /// </summary>
        [JsonProperty(Order = 2)]
        public string Name { get; set; }

        /// <summary>
        /// 縣市代碼
        /// </summary>
        [JsonProperty(Order = 3)]
        public string CountyCode { get; set; }

        /// <summary>
        /// 縣市名稱
        /// </summary>
        [JsonProperty(Order = 4)]
        public string CountyName { get; set; }

        /// <summary>
        /// 鄉鎮代碼
        /// </summary>
        [JsonProperty(Order = 5)]
        public string TownCode { get; set; }

        /// <summary>
        /// 鄉鎮名稱
        /// </summary>
        [JsonProperty(Order = 6)]
        public string TownName { get; set; }

        /// <summary>
        /// 緯度(WGS84)
        /// </summary>
        [JsonProperty(Order = 10)]
        public float Latitude { get; set; }

        /// <summary>
        /// 經度(WGS84)
        /// </summary>
        [JsonProperty(Order = 11)]
        public float Longtiude { get; set; }

        /// <summary>
        /// 業管單位名稱
        /// </summary>
        [JsonProperty(Order = 12)]
        public string AdminName { get; set; }

        /// <summary>
        /// 監測站種類
        /// </summary>
        [JsonProperty(Order = 30)]
        public HydroStationTypes StationType { get { return _stationType; } }

        /// <summary>
        /// 測量值
        /// </summary>
        [JsonProperty(Order = 20)]
        public Measurement[] Measurements { get; set; }
    }

    /// <summary>
    /// 河川監測站資訊
    /// </summary>
    public class RiverStationInfo : HydroStationInfo
    { 
        /// <summary>
        /// 河川、區排水位資訊
        /// </summary>
        public RiverStationInfo()
        {
            _stationType = HydroStationTypes.River;
        }

        /// <summary>
        /// 水系代碼
        /// </summary>
        [JsonProperty(Order = 7)]
        public int BasinCode { get; set; }

        /// <summary>
        /// 水系名稱
        /// </summary>
        [JsonProperty(Order = 8)]
        public string BasinName { get; set; }
    }

    /// <summary>
    /// 都市淹水感知站資訊
    /// </summary>
    public class UswgStationInfo : HydroStationInfo
    {
        /// <summary>
        /// 都市淹水感知器站資訊
        /// </summary>
        public UswgStationInfo()
        {
            _stationType = HydroStationTypes.Uswg;
        }
    }

    /// <summary>
    /// 雨量站資訊
    /// </summary>
    public class PrecipitationStationInfo : HydroStationInfo
    {
        /// <summary>
        /// 雨量站資訊
        /// </summary>
        public PrecipitationStationInfo()
        {
            _stationType = HydroStationTypes.Precipitation;
        }
    }

}
