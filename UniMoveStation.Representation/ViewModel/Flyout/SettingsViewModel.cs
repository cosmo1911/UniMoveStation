﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;
using UniMoveStation.Business.Service.Interfaces;

namespace UniMoveStation.Representation.ViewModel.Flyout
{
    public class SettingsViewModel : FlyoutBaseViewModel
    {
        private RelayCommand _saveSettingsCommand;
        private RelayCommand _saveCameraConfigurationsCommand;
        private RelayCommand _showCameraConfigurationsCommand;
        private RelayCommand _showPsMoveApiConfigurationsCommand;
        private RelayCommand _reloadSettingsCommand;
        private SettingsModel _settings;

        public JsonSettingsService SettingsService { get; set; }

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
        [PreferredConstructor]
        public SettingsViewModel() : this(new JsonSettingsService())
        {

        }

        public SettingsViewModel(JsonSettingsService jsonSettingsService)
        {
            Header = "Settings";
            Settings = new SettingsModel();
            SettingsService = jsonSettingsService ?? new JsonSettingsService();

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
            Settings = SettingsService.ReloadSettings();
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

        /// <summary>
        /// Gets the SaveCameraConfigurationsCommand.
        /// </summary>
        public RelayCommand SaveCameraConfigurationsCommand
        {
            get
            {
                return _saveCameraConfigurationsCommand
                    ?? (_saveCameraConfigurationsCommand = new RelayCommand(DoSaveCameraConfigurations));
            }
        }

        /// <summary>
        /// Gets the ShowCameraConfigurationsCommand.
        /// </summary>
        public RelayCommand ShowCameraConfigurationsCommand
        {
            get
            {
                return _showCameraConfigurationsCommand
                    ?? (_showCameraConfigurationsCommand = new RelayCommand(DoShowCameraConfigurations));
            }
        }

        /// <summary>
        /// Gets the ShowCameraConfigurationsCommand.
        /// </summary>
        public RelayCommand ShowPsMoveApiConfigurationsCommand
        {
            get
            {
                return _showPsMoveApiConfigurationsCommand
                    ?? (_showPsMoveApiConfigurationsCommand = new RelayCommand(DoShowPsMoveApiConfigurations));
            }
        }
        #endregion Commands

        #region Command Executions
        public void DoSaveSettings()
        {
            SettingsService.SaveSettings(Settings);
        }

        public void DoSaveCameraConfigurations()
        {
            foreach (CameraViewModel cvm in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
            {
                SimpleIoc.Default.GetInstance<ISettingsService>().SaveCamera(cvm.Camera);
            }
            SettingsService.SaveSettings(Settings);
        }

        /// <summary>
        /// opens the folder of the configuration files in the explorer
        /// </summary>
        private void DoShowCameraConfigurations()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "cfg";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Process.Start(path);
        }

        /// <summary>
        /// opens the folder of the psmoveapi configuration files in the explorer
        /// </summary>
        private void DoShowPsMoveApiConfigurations()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".psmoveapi");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Process.Start(path);
        }

        private void DoReloadSettings()
        {
            Settings = SettingsService.ReloadSettings();
            Application.Current.MainWindow.Top = Settings.Top;
            Application.Current.MainWindow.Left = Settings.Left;
        }

        protected override void DoClose()
        {
            DoReloadSettings();
            base.DoClose();
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
