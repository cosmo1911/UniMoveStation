using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniMoveStation.Model;

namespace UniMoveStation.Helper
{
    public class JsonCameraCalibrationConverter : JsonCreationConverter<CameraCalibrationModel>
    {
        protected override Type GetType(Type objectType, JObject jsonObject)
        {
            var type = (string)jsonObject.Property("Type");
            if(type.Equals("CameraCalibrationModel"))
            {
                return typeof(CameraCalibrationModel);
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
    } // JsonCameraCalibrationConverter
} // namespace
