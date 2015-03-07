using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniMoveStation.Business.Model;

namespace UniMoveStation.Business.JsonConverter
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
            // TODO get exisiting camera corresponding to the GUID
            //object target = SimpleIoc.Default.GetInstance<CameraViewModel>(GUID.ToString()).Camera;

            object target = null;
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }
    } // JsonCameraConverter
} // namespace
