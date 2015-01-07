using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using UniMoveStation.Model;

namespace UniMoveStation.ViewModel.Flyout
{
    public class SettingsViewModel : FlyoutBaseViewModel
    {
        private RelayCommand _saveCommand;
        private RelayCommand _reloadCommand;
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

            DoReload();
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
        /// Gets the ReloadCommand.
        /// </summary>
        public RelayCommand ReloadCommand
        {
            get
            {
                return _reloadCommand
                    ?? (_reloadCommand = new RelayCommand(DoReload));
            }
        }

        /// <summary>
        /// Gets the ApplyCommand.
        /// </summary>
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand
                    ?? (_saveCommand = new RelayCommand(DoSave));
            }
        }
        #endregion Commands

        #region Command Executions
        public void DoSave()
        {
            TextWriter writer = null;
            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(Settings, Newtonsoft.Json.Formatting.Indented);
                writer = new StreamWriter("user.conf.json", false);
                writer.Write(json);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public void DoReload()
        {
            TextReader reader = null;
            try
            {
                try
                {
                    reader = new StreamReader("user.conf.json");
                }
                catch(FileNotFoundException)
                {
                    reader = new StreamReader("default.conf.json");
                }
                finally
                {
                    string fileContents = reader.ReadToEnd();
                    Settings = Newtonsoft.Json.JsonConvert.DeserializeObject<SettingsModel>(fileContents);
                }
            }
            finally
            {
                if (reader != null)
                    reader.Close();
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
    }
}
