using System;
using Emgu.CV;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniMoveStation.Business.JsonConverter
{
    public class JsonMatrixConverter : JsonCreationConverter<Matrix<double>>
    {
        protected override Type GetType(Type objectType, JObject jsonObject)
        {
            
            var type = (string) jsonObject.Property("Rows");
            if(type.Equals("Rows"))
            {
                return typeof(Matrix<double>);
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
                Matrix<double> matrix = (Matrix<double>)value;

                JObject o = new JObject
                {
                    {"Rows", matrix.Rows},
                    {"Cols", matrix.Cols},
                    {"Data", new JArray(matrix.Data)}
                };


                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            JObject jObject = JObject.Load(reader);
            int rows = (int)jObject["Rows"];
            int cols = (int)jObject["Cols"];
            double[,] data = new double[rows, cols];

            double[] array = jObject["Data"].ToObject<double[]>();

            for (int row = 0; row < rows; row++)
            {
                for(int col = 0; col < cols; col++)
                {
                    data[row, col] = array[row * cols + col];
                }
            }

            return new Matrix<double>(data);
        }
    } // JsonMatrixConverter
} // namespace
