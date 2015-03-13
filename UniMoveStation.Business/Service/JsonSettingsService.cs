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

        public void LoadCamera(CameraModel camera)
        {
            CameraModel tmp = new CameraModel
            {
                GUID = camera.GUID
            };

            string path = String.Format(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\{0}.cam.json", tmp.GUID);
            if (!File.Exists(path))
            {
                camera.Name = "Camera " + camera.TrackerId;
            }
            else
            {
                TextReader reader = null;
                try
                {
                    reader = new StreamReader(path);
                }
                finally
                {
                    if (reader != null)
                    {
                        string fileContents = reader.ReadToEnd();
                        tmp = JsonConvert.DeserializeObject<CameraModel>(fileContents);

                        reader.Close();

                        camera.Annotate = tmp.Annotate;
                        camera.FPS = tmp.FPS;
                        camera.Name = tmp.Name;
                    }
                }
            }
            
        }

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
            TextReader reader;
            string defaultPath = AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\default.conf.json";
            string userPath = AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\user.conf.json";
            string movedHostsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".psmoveapi\\moved_hosts.txt");
            if (File.Exists(userPath))
            {
                reader = new StreamReader(userPath);
                string fileContents = reader.ReadToEnd();
                settings = JsonConvert.DeserializeObject<SettingsModel>(fileContents);
                reader.Close();
            }
            else if (File.Exists(defaultPath))
            {
                reader = new StreamReader(defaultPath);
                string fileContents = reader.ReadToEnd();
                settings = JsonConvert.DeserializeObject<SettingsModel>(fileContents);
                reader.Close();
            }
            if (File.Exists(movedHostsPath))
            {
                settings.MovedHosts = File.ReadAllLines(movedHostsPath).ToList();
            }
            return settings;
        } // ReloadSettings

        public CameraCalibrationModel LoadCalibration(string guid)
        {
            CameraCalibrationModel calibration = new CameraCalibrationModel()
            {
                CameraGuid = guid
            };
            TextReader reader = null;

            string path = String.Format(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\{0}.calib.json", guid);
            if (File.Exists(path))
            {
                try
                {
                    reader = new StreamReader(path);
                    string fileContents = reader.ReadToEnd();
                    calibration = JsonConvert.DeserializeObject<CameraCalibrationModel>(
                        fileContents,
                        new JsonIntrinsicCameraParametersConverter(),
                        new JsonExtrinsicCameraParametersConverter(),
                        new JsonMatrixConverter(),
                        new JsonPointFConverter());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }
            return calibration;
        } // LoadCalibration
    } // JsonSettingsService
} // namespace
