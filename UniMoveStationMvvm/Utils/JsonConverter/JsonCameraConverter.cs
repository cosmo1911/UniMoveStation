using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using GalaSoft.MvvmLight.Ioc;
using UniMoveStation.Model;
using UniMoveStation.ViewModel;

namespace UniMoveStation.Utils.JsonConverter
{
    public class JsonCameraConverter : JsonCreationConverter<CameraModel>
    {
        protected override Type GetType(Type objectType, JObject jsonObject)
        {
            var type = (string)jsonObject.Property("Type");
            if (type.Equals("CameraModel"))
            {
                return typeof(CameraModel);
            }
            throw new ApplicationException(String.Format("The given type {0} is not supported!", type));
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
            object target = SimpleIoc.Default.GetInstance<CameraViewModel>(GUID.ToString()).Camera;

            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }
    } // JsonCameraConverter
} // namespace
