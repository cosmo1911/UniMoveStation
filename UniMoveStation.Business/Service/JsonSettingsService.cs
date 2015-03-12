using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UniMoveStation.Business.JsonConverter;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Interfaces;

namespace UniMoveStation.Business.Service
{
    public class JsonSettingsService : ISettingsService
    {
        public void SaveSettings(SettingsModel settings)
        {
            TextWriter writer = null;
            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\user.conf.json", false);
                writer.Write(json);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

            File.WriteAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".psmoveapi\\moved_hosts.txt"),
                settings.MovedHosts);
        } // SaveSettings

        public void SaveCamera(CameraModel cameraModel)
        {
            TextWriter writer = null;
            try
            {
                string json = JsonConvert.SerializeObject(cameraModel, Formatting.Indented);
                writer = new StreamWriter(String.Format(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\{0}.cam.json", cameraModel.GUID), false);
                writer.Write(json);
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        } // SaveCameras

        public void SaveCalibration(CameraCalibrationModel calibration)
        {
            TextWriter writer = null;
            try
            {
                string json = JsonConvert.SerializeObject(
                    calibration,
                    Formatting.Indented,
                    new JsonIntrinsicCameraParametersConverter(),
                    new JsonExtrinsicCameraParametersConverter(),
                    new JsonMatrixConverter(),
                    new JsonPointFConverter());
                writer = new StreamWriter(String.Format(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\{0}.calib.json", calibration.CameraGuid), false);
                writer.Write(json);
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        } // SaveCalibration

        public SettingsModel ReloadSettings()
        {
            SettingsModel settings = new SettingsModel();
            TextReader reader = null;
            try
            {
                try
                {
                    reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\user.conf.json");
                }
                catch (FileNotFoundException)
                {
                    reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\default.conf.json");
                }
            }
            finally
            {
                if (reader != null)
                {
                    string fileContents = reader.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<SettingsModel>(fileContents);
                    reader.Close();
                }
            }

            settings.MovedHosts = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".psmoveapi\\moved_hosts.txt")).ToList();

            return settings;
        } // ReloadSettings

        public CameraCalibrationModel LoadCalibration(string guid)
        {
            CameraCalibrationModel calibration = new CameraCalibrationModel()
            {
                CameraGuid = guid
            };

            TextReader reader = null;
            try
            {
                string path = String.Format(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\{0}.calib.json", guid);
                if (!File.Exists(path)) return calibration;
                try
                {
                    reader = new StreamReader(path);
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
            finally
            {
                if (reader != null)
                {
                    string fileContents = reader.ReadToEnd();
                    calibration = JsonConvert.DeserializeObject<CameraCalibrationModel>(
                        fileContents,
                        new JsonIntrinsicCameraParametersConverter(),
                        new JsonExtrinsicCameraParametersConverter(),
                        new JsonMatrixConverter(),
                        new JsonPointFConverter());

                    reader.Close();

                }
            }
            return calibration;
        } // LoadCalibration
    } // JsonSettingsService
} // namespace
