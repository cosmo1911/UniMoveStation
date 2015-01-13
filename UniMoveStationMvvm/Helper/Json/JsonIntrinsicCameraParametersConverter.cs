using Emgu.CV;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniMoveStation.Helper.Json
{
    public class JsonIntrinsicCameraParametersConverter : JsonCreationConverter<IntrinsicCameraParameters>
    {

        protected override Type GetType(Type objectType, JObject jsonObject)
        {
            var type = (string) jsonObject.Property("IntrinsicMatrix");
            if (type != null)
            {
                return typeof(IntrinsicCameraParameters);
            }
            else
            {
                throw new ApplicationException(String.Format("The given type {0} is not supported!", type));
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    } // JsonIntrinsicCameraParametersConverter
} // namespace
