using System;
using System.Collections.Generic;
using System.Text;

namespace Hydrology.OpenDataApi.Model
{
    /// <summary>
    /// 鄉鎮資訊
    /// </summary>
    public class TownInfo
    {
        /// <summary>
        /// 所屬縣市代碼
        /// </summary>
        public string countyCode { get; set; }
        /// <summary>
        /// 所屬縣市名稱
        /// </summary>
        public string countyName { get; set; }
        /// <summary>
        /// 鄉鎮代碼
        /// </summary>
        public string townCode { get; set; }
        /// <summary>
        /// 鄉鎮名稱
        /// </summary>
        public string townName { get; set; }
    }
}
