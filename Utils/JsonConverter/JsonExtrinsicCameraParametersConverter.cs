using Emgu.CV;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace UniMoveStation.Utils.JsonConverter
{
    public class JsonExtrinsicCameraParametersConverter : JsonCreationConverter<ExtrinsicCameraParameters>
    {

        protected override Type GetType(Type objectType, JObject jsonObject)
        {
            var type = jsonObject.Property("ExtrinsicMatrix").Name;
            if (type != null)
            {
                return typeof(ExtrinsicCameraParameters);
            }
            throw new ApplicationException(String.Format("The given type {0} is not supported!", type));
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
                ExtrinsicCameraParameters ex = (ExtrinsicCameraParameters)value;

                JObject rotationVector = JObject.Parse(
                    JsonConvert.SerializeObject(
                        ex.RotationVector.RotationMatrix, 
                        new JsonMatrixConverter()));

                JObject translationVector = JObject.Parse(
                    JsonConvert.SerializeObject(
                        ex.TranslationVector, 
                        new JsonMatrixConverter()));
                JObject extrinsicMatrix = JObject.Parse(
                    JsonConvert.SerializeObject(
                        ex.ExtrinsicMatrix, 
                        new JsonMatrixConverter()));

                JObject o = new JObject
                {
                    {"RotationVector", rotationVector},
                    {"TranslationVector", translationVector},
                    {"ExtrinsicMatrix", extrinsicMatrix}
                };
                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            ExtrinsicCameraParameters ecp = new ExtrinsicCameraParameters()
            {
                RotationVector = new RotationVector3D
                {
                    RotationMatrix = (Matrix<double>) new JsonMatrixConverter().ReadJson(
                        jObject["RotationVector"].CreateReader(),
                        typeof (Matrix<double>),
                        null,
                        serializer)
                },
                TranslationVector = (Matrix<double>) new JsonMatrixConverter().ReadJson(
                    jObject["TranslationVector"].CreateReader(), 
                    typeof(Matrix<double>), 
                    null, 
                    serializer)
            };

            
            return ecp;
        }
    } // JsonExtrinsicCameraParametersConverter
} // namespace
