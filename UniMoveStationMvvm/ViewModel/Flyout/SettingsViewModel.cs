using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using UniMoveStation.Helper;
using UniMoveStation.Model;

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
                                       .Select(a => new AccentColorMenuData() { 
                                           Name = a.Name, 
                                           ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                                       .ToList();

            // create metro theme color menu items for the demo
            AppThemes = ThemeManager.AppThemes
                                    .Select(a => new AppThemeMenuData() { 
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
                string json = JsonConvert.SerializeObject(Settings, Newtonsoft.Json.Formatting.Indented);
                writer = new StreamWriter("cfg\\user.conf.json", false);
                writer.Write(json);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public void DoSaveCameras()
        {
            ObservableCollection<SingleCameraModel> cameras = new ObservableCollection<SingleCameraModel>();

            foreach (SingleCameraViewModel scvm in SimpleIoc.Default.GetAllCreatedInstances<SingleCameraViewModel>())
            {
                TextWriter writer = null;
                try
                {
                    string json = JsonConvert.SerializeObject(scvm.Camera, Newtonsoft.Json.Formatting.Indented);
                    writer = new StreamWriter(String.Format("cfg\\{0}.cam.json", scvm.Camera.GUID), false);
                    writer.Write(json);
                }
                finally
                {
                    if (writer != null) writer.Close();
                }
            }
        }

        public void DoSaveCalibration(SingleCameraModel camera)
        {
            TextWriter writer = null;
            try
            {
                string json = JsonConvert.SerializeObject(
                    camera.Calibration, 
                    Newtonsoft.Json.Formatting.Indented, 
                    new JsonIntrinsicCameraParametersConverter(),
                    new JsonExtrinsicCameraParametersConverter());
                writer = new StreamWriter(String.Format("cfg\\{0}.calib.json", camera.GUID), false);
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
                    reader = new StreamReader("cfg\\user.conf.json");
                }
                catch (FileNotFoundException)
                {
                    reader = new StreamReader("cfg\\default.conf.json");
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
        }

        public void LoadCalibration(SingleCameraModel camera)
        {
            TextReader reader = null;
            try
            {
                try
                {
                    reader = new StreamReader(String.Format("cfg\\{0}.calib.json", camera.GUID));
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
                        new JsonExtrinsicCameraParametersConverter());

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
            var accent = ThemeManager.GetAccent(this.Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
        }
    }

    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void DoChangeTheme()
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = ThemeManager.GetAppTheme(this.Name);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
        }
    } // SettingsViewModel
} // namespace
