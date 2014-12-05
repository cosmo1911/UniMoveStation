using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using UniMoveStation.ViewModel.Flyout;

namespace UniMoveStation.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private RelayCommand _settingsCommand;
        private RelayCommand<FlyoutBaseViewModel> _toggleFlyoutCommand;

        public ObservableCollection<FlyoutBaseViewModel> Flyouts
        {
            get;
            private set;
        }

        #region Constructor
        public MainViewModel()
        {
            Flyouts = new ObservableCollection<FlyoutBaseViewModel>();
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the ShowFlyout.
        /// </summary>
        public RelayCommand<FlyoutBaseViewModel> ToggleFlyoutCommand
        {
            get
            {
                return _toggleFlyoutCommand
                    ?? (_toggleFlyoutCommand = new RelayCommand<FlyoutBaseViewModel>(DoToggleFlyout));
            }
        }

        /// <summary>
        /// Gets the SettingsCommand.
        /// </summary>
        public RelayCommand SettingsCommand
        {
            get
            {
                return _settingsCommand
                    ?? (_settingsCommand = new RelayCommand(
                    () =>
                    {
                        DoToggleFlyout(ViewModelLocator.Instance.Settings);
                    }));
            }
        }
        #endregion

        #region Command Executions
        public void DoToggleFlyout(FlyoutBaseViewModel flyout)
        {
            if(!Flyouts.Contains(flyout))
            {
                Flyouts.Add(flyout);
            }
            foreach(FlyoutBaseViewModel fbvw in Flyouts)
            {
                if (fbvw.Equals(flyout)) continue;
                fbvw.IsOpen = false;
            }
            flyout.IsOpen = !flyout.IsOpen;
        }
        #endregion
    } // MainViewModel
} // namespace