using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using UniMoveStation.Model;
using UniMoveStation.Utils.JsonConverter;

namespace UniMoveStation.ViewModel.Flyout
{
    public class SettingsViewModel : FlyoutBaseViewModel
    {
        private RelayCommand _saveSettingsCommand;
        private RelayCommand _reloadSettingsCommand;
        private SettingsModel _settings;

        public List<AccentColorMenuData> AccentColors 
        { 
            get; 
            set; 
        }

        public List<AppThemeMenuData> AppThemes 
        { 
            get; 
            set; 
        }

        #region Constructor
        public SettingsViewModel()
        {
            Header = "Settings";
            Settings = new SettingsModel();

            // create accent color menu items for the demo
            AccentColors = ThemeManager.Accents
                                       .Select(a => new AccentColorMenuData
                                       { 
                                           Name = a.Name, 
                                           ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                                       .ToList();

            // create metro theme color menu items for the demo
            AppThemes = ThemeManager.AppThemes
                                    .Select(a => new AppThemeMenuData
                                    { 
                                        Name = a.Name, 
                                        BorderColorBrush = a.Resources["BlackColorBrush"] as Brush,
                                        ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
                                    .ToList();
            DoReloadSettings();
        }
        #endregion

        #region Properties
        /// <summary>
        /// The <see cref="Settings" /> property's name.
        /// </summary>
        public const string SettingsPropertyName = "Settings";

        /// <summary>
        /// Sets and gets the Settings property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the MessengerInstance when it changes.
        /// </summary>
        public SettingsModel Settings
        {
            get
            {
                return _settings;
            }

            set
            {
                if (_settings == value)
                {
                    return;
                }

                var oldValue = _settings;
                _settings = value;
                RaisePropertyChanged(() => Settings, oldValue, value, true);
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the ReloadSettingsCommand.
        /// </summary>
        public RelayCommand ReloadSettingsCommand
        {
            get
            {
                return _reloadSettingsCommand
                    ?? (_reloadSettingsCommand = new RelayCommand(DoReloadSettings));
            }
        }

        /// <summary>
        /// Gets the SaveSettingsCommand.
        /// </summary>
        public RelayCommand SaveSettingsCommand
        {
            get
            {
                return _saveSettingsCommand
                    ?? (_saveSettingsCommand = new RelayCommand(DoSaveSettings));
            }
        }
        #endregion Commands

        #region Command Executions
        public void DoSaveSettings()
        {
            TextWriter writer = null;
            try
            {
                string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\user.conf.json", false);
                writer.Write(json);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

            File.WriteAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".psmoveapi\\moved_hosts.txt"), 
                Settings.MovedHosts);
        }

        public void DoSaveCameras()
        {
            ObservableCollection<CameraModel> cameras = new ObservableCollection<CameraModel>();

            foreach (CameraViewModel scvm in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
            {
                TextWriter writer = null;
                try
                {
                    string json = JsonConvert.SerializeObject(scvm.Camera, Formatting.Indented);
                    writer = new StreamWriter(String.Format(AppDomain.CurrentDomain.BaseDirectory + "\\cfg\\{0}.cam.json", scvm.Camera.GUID), false);
                    writer.Write(json);
                }
                finally
                {
                    if (writer != null) writer.Close();
                }
            }
        }

        public void DoSaveCalibration(CameraModel camera)
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
        }

        public void DoReloadSettings()
        {
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
                    Settings = JsonConvert.DeserializeObject<SettingsModel>(fileContents);
                    reader.Close();
                }
            }

            Settings.MovedHosts = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                ".psmoveapi\\moved_hosts.txt")).ToList();

        }

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
        }
        #endregion
    }

    public class AccentColorMenuData
    {
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }

        private RelayCommand _changeAccentCommand;

        /// <summary>
        /// Gets the ChangeThemeCommand.
        /// </summary>
        public RelayCommand ChangeAccentCommand
        {
            get
            {
                return _changeAccentCommand
                    ?? (_changeAccentCommand = new RelayCommand(DoChangeTheme));
            }
        }

        protected virtual void DoChangeTheme()
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
        }
    }

    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void DoChangeTheme()
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = ThemeManager.GetAppTheme(Name);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
        }
    } // SettingsViewModel
} // namespace
