using System;
using System.Collections.Generic;
using System.Text;

namespace Hydrology.OpenDataApi.Model
{
    public class ResponseJsonFileUtility
    {
        public static string GetResponseJsonFileName(FileContentTypes fileType)
        {
            return $"{fileType}.json";
        }

        public enum FileContentTypes
        {
            countyCodes, townCodes, basinCodes, riverStations, riverStationWithValues, uswgStations, uswgStationWithValues, precipitationStations,  precipitationStationWithValues
        }
    }
}
