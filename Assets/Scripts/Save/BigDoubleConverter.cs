using System;
using BreakInfinity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.Save
{
    /// 将 BigDouble 序列化为 {"m": mantissa, "e": exponent}。
    public class BigDoubleConverter : JsonConverter<BigDouble>
    {
        public override void WriteJson(JsonWriter writer, BigDouble value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("m");
            writer.WriteValue(value.Mantissa);
            writer.WritePropertyName("e");
            writer.WriteValue(value.Exponent);
            writer.WriteEndObject();
        }

        public override BigDouble ReadJson(JsonReader reader, Type objectType, BigDouble existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // 兼容三种来源：对象 {m,e}、纯数字、纯字符串
            if (reader.TokenType == JsonToken.Null) return BigDouble.Zero;

            if (reader.TokenType == JsonToken.StartObject)
            {
                var jo = JObject.Load(reader);
                double m = jo["m"]?.Value<double>() ?? 0;
                long e = jo["e"]?.Value<long>() ?? 0;
                return new BigDouble(m, e);
            }

            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
            {
                return new BigDouble(Convert.ToDouble(reader.Value));
            }

            if (reader.TokenType == JsonToken.String)
            {
                return BigDouble.Parse((string)reader.Value);
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} for BigDouble");
        }
    }
}
