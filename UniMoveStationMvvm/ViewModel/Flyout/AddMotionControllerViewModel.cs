using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using UniMoveStation.Model;

namespace UniMoveStation.ViewModel.Flyout
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class AddMotionControllerViewModel : FlyoutBaseViewModel
    {
        private MotionControllerModel _motionController;
        private bool _controllersDetected;
        private RelayCommand _cancelCommand;
        private RelayCommand _createCommand;
        private RelayCommand _refreshCommand;

        #region Properties
        public MotionControllerModel MotionController
        {
            get
            {
                return _motionController;
            }
            private set
            {
                Set(() => MotionController, ref _motionController, value);
            }
        }

        public bool ControllersDetected
        {
            get
            {
                ControllersDetected = io.thp.psmove.pinvoke.count_connected() > 0;

                return _controllersDetected;
            }
            set
            {
                Set(() => ControllersDetected, ref _controllersDetected, value);
            }
        }
        #endregion

        #region Contructor
        /// <summary>
        /// Initializes a new instance of the AddMotionControllerViewModel class.
        /// </summary>
        public AddMotionControllerViewModel()
        {
            Position = Position.Right;
            Header = "Add Motion Controller";
            MotionController = new MotionControllerModel();
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
                    ?? (_cancelCommand = new RelayCommand(DoCancelCommand));
            }
        }

        /// <summary>
        /// Gets the CreateCommand.
        /// </summary>
        public RelayCommand CreateCommand
        {
            get
            {
                return _createCommand
                    ?? (_createCommand = new RelayCommand(DoCreateCommand));
            }
        }

        /// <summary>
        /// Gets the RefreshCommand.
        /// </summary>
        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(DoRefreshCommand));
            }
        }
        #endregion

        #region Command Executions
        public void DoCancelCommand()
        {

        }

        public void DoCreateCommand()
        {

        }

        public void DoRefreshCommand()
        {
            int connectedCount = io.thp.psmove.pinvoke.count_connected();
            ObservableCollection<MotionControllerViewModel> detectedControllers = new ObservableCollection<MotionControllerViewModel>();

            foreach(MotionControllerViewModel mcvw in SimpleIoc.Default.GetAllInstances<MotionControllerViewModel>())
            {
                detectedControllers.Add(mcvw);
            }

            foreach(MotionControllerViewModel mcvw in detectedControllers)
            {

            }
        }
        #endregion
    }
}