﻿using System;
using System.IO;
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

            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".psmoveapi\\moved_hosts.txt"),
                settings.MovedHostsFile);
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
            string path = String.Format(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\{0}.cam.json", camera.GUID);
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
                        CameraModel tmp = JsonConvert.DeserializeObject<CameraModel>(fileContents);

                        camera.Name = tmp.Name;
                        camera.FPS = tmp.FPS;
                        camera.Annotate = tmp.Annotate;

                        reader.Close();
                    }
                }
            }
        } // LoadCameras

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
            string movedMdPath = AppDomain.CurrentDomain.BaseDirectory + "\\moved.md";
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
                settings.MovedHostsFile = File.ReadAllText(movedHostsPath);
                //string[] lines = File.ReadAllLines(movedHostsPath);
                //foreach (string line in lines)
                //{
                //    settings.MovedHostsFile += line + "&#10;";
                //}
            }
            if (File.Exists(movedMdPath))
            {
                settings.MovedHostsWaterMark = File.ReadAllText(movedMdPath);
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
