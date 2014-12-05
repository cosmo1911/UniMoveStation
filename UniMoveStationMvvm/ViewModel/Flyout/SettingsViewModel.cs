using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace UniMoveStation.ViewModel.Flyout
{
    public class SettingsViewModel : FlyoutBaseViewModel
    {
        private RelayCommand _applyCommand;
        private RelayCommand _cancelCommand;

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
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the CancelCommand.
        /// </summary>
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(DoCancel));
            }
        }

        /// <summary>
        /// Gets the ApplyCommand.
        /// </summary>
        public RelayCommand ApplyCommand
        {
            get
            {
                return _applyCommand
                    ?? (_applyCommand = new RelayCommand(DoApply));
            }
        }
        #endregion Commands

        #region Command Executions
        public void DoApply()
        {

        }

        public void DoCancel()
        {
            IsOpen = false;
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
