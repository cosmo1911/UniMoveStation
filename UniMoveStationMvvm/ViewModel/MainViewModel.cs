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
        private ObservableCollection<FlyoutBaseViewModel> _flyouts = new ObservableCollection<FlyoutBaseViewModel>();
        public ObservableCollection<FlyoutBaseViewModel> Flyouts
        {
            get { return _flyouts; }
            set { Set(() => Flyouts, ref _flyouts, value); }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            foreach (FlyoutBaseViewModel fbvw in SimpleIoc.Default.GetAllCreatedInstances<FlyoutBaseViewModel>())
            {
                _flyouts.Add(fbvw);
            }
        }

        private RelayCommand<FlyoutBaseViewModel> _toggleFlyoutCommand;

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

        public void DoToggleFlyout(FlyoutBaseViewModel flyout)
        {
            if(!Flyouts.Contains(flyout))
            {
                Flyouts.Add(flyout);
            }
            flyout.IsOpen = !flyout.IsOpen;
        }
    } // MainViewModel
} // namespace