using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.ObjectModel;
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
            get
            {
                return _flyouts;
            }
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

            Flyouts.Add(new AddMotionControllerViewModel());
        }

        private RelayCommand<string> _showFlyoutCommand;

        /// <summary>
        /// Gets the ShowFlyout.
        /// </summary>
        public RelayCommand<string> ToggleFlyout
        {
            get
            {
                return _showFlyoutCommand
                    ?? (_showFlyoutCommand = new RelayCommand<string>(DoToggleFlyout));
            }
        }

        public void DoToggleFlyout(string tag)
        {
            if (tag.Equals("controllers")) IsOpen = true;
        }

        /// <summary>
        /// The <see cref="IsOpen" /> property's name.
        /// </summary>
        public const string IsOpenPropertyName = "IsOpen";

        private bool _isOpen = false;

        /// <summary>
        /// Sets and gets the IsOpen property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }

            set
            {
                if (_isOpen == value)
                {
                    return;
                }

                _isOpen = value;
                RaisePropertyChanged(() => IsOpen);
            }
        }
    }
}