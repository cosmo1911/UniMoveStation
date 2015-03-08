using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UniMoveStation.Business.JsonConverter;
using UniMoveStation.Business.Model;

namespace UniMoveStation.Business.Service
{
    public class JsonSettingsService
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

        public void SaveCalibration(CameraModel camera)
        {
            TextWriter writer = null;
            try
            {
                string json = JsonConvert.SerializeObject(
                    camera.Calibration,
                    Formatting.Indented,
                    new JsonIntrinsicCameraParametersConverter(),
                    new JsonExtrinsicCameraParametersConverter(),
                    new JsonMatrixConverter(),
                    new JsonPointFConverter());
                writer = new StreamWriter(String.Format(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\{0}.calib.json", camera.GUID), false);
                writer.Write(json);
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        } // SaveCalibration

        public void ReloadSettings(SettingsModel settings)
        {
            settings = ReloadSettings();
        }

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

        public void LoadCalibration(CameraModel camera)
        {
            TextReader reader = null;
            try
            {
                string path = String.Format(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\{0}.calib.json", camera.GUID);
                if (!File.Exists(path)) return;
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
                    camera.Calibration = JsonConvert.DeserializeObject<CameraCalibrationModel>(
                        fileContents,
                        new JsonIntrinsicCameraParametersConverter(),
                        new JsonExtrinsicCameraParametersConverter(),
                        new JsonMatrixConverter(),
                        new JsonPointFConverter());

                    reader.Close();
                }
            }
        } // LoadCalibration
    } // JsonSettingsService
} // namespace
