using Emgu.CV;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UniMoveStation.Helper
{
    public class JsonExtrinsicCameraParametersConverter : JsonCreationConverter<ExtrinsicCameraParameters>
    {

        protected override Type GetType(Type objectType, JObject jsonObject)
        {
            var type = (string) jsonObject.Property("ExtrinsicMatrix").Name;
            if (type != null)
            {
                return typeof(ExtrinsicCameraParameters);
            }
            else
            {
                throw new ApplicationException(String.Format("The given type {0} is not supported!", type));
            }
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

                JObject o = new JObject();
                o.Add("RotationVector", rotationVector);
                o.Add("TranslationVector", translationVector);
                o.Add("ExtrinsicMatrix", extrinsicMatrix);
                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            RotationVector3D rotation = new RotationVector3D();

            rotation.RotationMatrix = (Matrix<double>) new JsonMatrixConverter().ReadJson(
                jObject["RotationVector"].CreateReader(), 
                typeof(Matrix<double>), 
                null, 
                serializer);

            Matrix<double> translation = (Matrix<double>) new JsonMatrixConverter().ReadJson(
                jObject["TranslationVector"].CreateReader(), 
                typeof(Matrix<double>), 
                null, 
                serializer);

            
            return new ExtrinsicCameraParameters(rotation, translation);
        }
    } // JsonExtrinsicCameraParametersConverter
} // namespace
