using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hydrology.OpenDataApi.Model
{
    /// <summary>
    /// 監測物理量
    /// </summary>
    public class Measurement
    {
        public Guid IoWPhysicalQuantityId { get; set; }

        /// <summary>
        /// 監測資料時間
        /// </summary>
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// 監測物理量名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 國際標準單位(SI)
        /// </summary>
        public string SIUnit { get; set; }

        /// <summary>
        /// 數值
        /// </summary>
        [JsonConverter(typeof(ValueJsonConverter))]
        public dynamic Value { get; set; }
    }

    public class ValueJsonConverter : JsonConverter
    {

        public ValueJsonConverter()
        {
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is ValueType)
            {
                if (((ValueType)value) is double || ((ValueType)value) is float)
                {
                    double dvalue = Convert.ToDouble(value);
                    writer.WriteValue(Math.Round(dvalue, 4));
                }
            }
            else
            {
                JToken token = JToken.FromObject(value);
                token.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.Equals(typeof(ValueType));
        }
    }
}
