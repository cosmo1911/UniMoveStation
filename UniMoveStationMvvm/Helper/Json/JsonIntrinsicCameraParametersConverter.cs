using Emgu.CV;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniMoveStation.Helper
{
    public class JsonIntrinsicCameraParametersConverter : JsonCreationConverter<IntrinsicCameraParameters>
    {

        protected override Type GetType(Type objectType, JObject jsonObject)
        {
            var type = (string) jsonObject.Property("IntrinsicMatrix").Name;
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

                JObject o = new JObject();
                o.Add("DistortionCoeffs", distortionCoeffs);
                o.Add("IntrinsicMatrix", intrinsicMatrix);
                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            IntrinsicCameraParameters intrinsicParameters = new IntrinsicCameraParameters();
            
            intrinsicParameters.IntrinsicMatrix = (Matrix<double>)new JsonMatrixConverter().ReadJson(
                jObject["IntrinsicMatrix"].CreateReader(),
                typeof(Matrix<double>),
                null,
                serializer);
            intrinsicParameters.DistortionCoeffs = (Matrix<double>)new JsonMatrixConverter().ReadJson(
                jObject["DistortionCoeffs"].CreateReader(),
                typeof(Matrix<double>),
                null,
                serializer);

            return intrinsicParameters;
        }
    } // JsonIntrinsicCameraParametersConverter
} // namespace
