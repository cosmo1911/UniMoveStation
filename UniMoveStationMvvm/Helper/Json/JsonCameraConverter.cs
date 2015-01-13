using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniMoveStation.Model;
using UniMoveStation.ViewModel;

namespace UniMoveStation.Helper
{
    public class JsonCameraConverter : JsonCreationConverter<SingleCameraModel>
    {
        protected override Type GetType(Type objectType, JObject jsonObject)
        {
            var type = (string)jsonObject.Property("Type");
            if (type.Equals("SingleCameraModel"))
            {
                return typeof(SingleCameraModel);
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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            Type targetType = GetType(objectType, jObject);

            Guid GUID = (Guid) jObject.Property("GUID");
            object target = SimpleIoc.Default.GetInstance<SingleCameraViewModel>(GUID.ToString()).Camera;

            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }
    } // JsonCameraConverter
} // namespace
