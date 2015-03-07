using System;
using Emgu.CV;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniMoveStation.Business.JsonConverter
{
    public class JsonIntrinsicCameraParametersConverter : JsonCreationConverter<IntrinsicCameraParameters>
    {

        protected override Type GetType(Type objectType, JObject jsonObject)
        {
            var type = jsonObject.Property("IntrinsicMatrix").Name;
            if (type != null)
            {
                return typeof(IntrinsicCameraParameters);
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
                IntrinsicCameraParameters para = (IntrinsicCameraParameters)value;


                JObject distortionCoeffs = JObject.Parse(JsonConvert.SerializeObject(para.DistortionCoeffs, new JsonMatrixConverter()));
                JObject intrinsicMatrix = JObject.Parse(JsonConvert.SerializeObject(para.IntrinsicMatrix, new JsonMatrixConverter()));

                JObject o = new JObject
                {
                    {"DistortionCoeffs", distortionCoeffs}, 
                    {"IntrinsicMatrix", intrinsicMatrix}
                };
                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            IntrinsicCameraParameters intrinsicParameters = new IntrinsicCameraParameters
            {
                IntrinsicMatrix = (Matrix<double>) new JsonMatrixConverter().ReadJson(
                    jObject["IntrinsicMatrix"].CreateReader(),
                    typeof (Matrix<double>),
                    null,
                    serializer),

                DistortionCoeffs = (Matrix<double>) new JsonMatrixConverter().ReadJson(
                    jObject["DistortionCoeffs"].CreateReader(),
                    typeof (Matrix<double>),
                    null,
                    serializer)
            };

            return intrinsicParameters;
        }
    } // JsonIntrinsicCameraParametersConverter
} // namespace
