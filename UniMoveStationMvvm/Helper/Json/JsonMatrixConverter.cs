using Emgu.CV;
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
    public class JsonMatrixConverter : JsonCreationConverter<Matrix<double>>
    {
        protected override Type GetType(Type objectType, JObject jsonObject)
        {
            
            var type = (string) jsonObject.Property("Rows");
            if(type.Equals("Rows"))
            {
                return typeof(Matrix<double>);
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
                Matrix<double> matrix = (Matrix<double>)value;

                JObject o = new JObject();

                o.Add("Rows", matrix.Rows);
                o.Add("Cols", matrix.Cols);
                o.Add("Data", new JArray(matrix.Data));

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

            for (int y = 0; y < rows; y++)
            {
                for(int x = 0; x < cols; x++)
                {
                    data[y, x] = array[y * cols + x];
                }
            }

            return new Matrix<double>(data);
        }
    } // JsonMatrixConverter
} // namespace
