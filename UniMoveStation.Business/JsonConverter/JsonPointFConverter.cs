using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniMoveStation.Business.JsonConverter
{
    public class JsonPointFConverter : JsonCreationConverter<PointF>
    {
        protected override Type GetType(Type objectType, JObject jsonObject)
        {

            var x = (string)jsonObject.Property("X");
            var y = (string)jsonObject.Property("Y");
            var isEmpty = (string)jsonObject.Property("IsEmpty");
            if (x.Equals("X") && y.Equals("Y") && isEmpty.Equals("IsEmpty"))
            {
                return typeof(List<PointF>);
            }
            throw new ApplicationException(String.Format("The given type {0} is not supported!", x));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);

            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                PointF point = (PointF) value;
                JObject o = new JObject
                {
                    {"PointF", new JArray(point.X, point.Y)}
                };

                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            float[] data = jObject["PointF"].ToObject<float[]>();

            return new PointF(data[0], data[1]);
        }
    } // JsonMatrixConverter
} // namespace
