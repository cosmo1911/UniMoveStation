using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using UniMoveStation.Model;
using UniMoveStation.Service;

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
        private MotionControllerModel _newMotionController;
        private bool _newControllersDetected;
        private RelayCommand _cancelCommand;
        private RelayCommand _createCommand;
        private RelayCommand _refreshCommand;
        private RelayCommand<object> _selectItemCommand;

        private ObservableCollection<MotionControllerModel> _availableMotionControllers;

        #region Properties
        public ObservableCollection<MotionControllerModel> AvailableMotionControllers
        {
            get
            {
                if (_availableMotionControllers == null)
                {
                    AvailableMotionControllers = new ObservableCollection<MotionControllerModel>();
                }
                return _availableMotionControllers;
            }
            set
            {
                Set(() => AvailableMotionControllers, ref _availableMotionControllers, value);
            }
        }
        public MotionControllerModel NewMotionController
        {
            get
            {
                return _newMotionController;
            }
            private set
            {
                Set(() => NewMotionController, ref _newMotionController, value);
            }
        }

        public bool NewControllersDetected
        {
            get
            {
                NewControllersDetected = io.thp.psmove.pinvoke.count_connected() > 0;

                return _newControllersDetected;
            }
            set
            {
                Set(() => NewControllersDetected, ref _newControllersDetected, value);
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
            NewMotionController = new MotionControllerModel();
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

        /// <summary>
        /// Gets the SelectItemCommand.
        /// </summary>
        public RelayCommand<object> SelectItemCommand
        {
            get
            {
                return _selectItemCommand
                    ?? (_selectItemCommand = new RelayCommand<object>(DoSelectItemCommand));
            }
        }
        #endregion


        #region Command Executions
        public void DoCancelCommand()
        {
            NewMotionController = new MotionControllerModel();
            IsOpen = false;
        }

        public void DoCreateCommand()
        {
            if (NewMotionController != null)
            {
                MotionControllerViewModel mcvw = new MotionControllerViewModel(NewMotionController);
                SimpleIoc.Default.Register<MotionControllerViewModel>(
                    () => mcvw,
                    NewMotionController.Serial,
                    true);
                ViewModelLocator.Instance.Navigation.MotionControllerTabs.Add(mcvw);
                IsOpen = false;
            }
        }

        public void DoSelectItemCommand(object item)
        {
            if(item != null)
            {
                NewMotionController = (MotionControllerModel)item;
                MotionControllerService mcs = new MotionControllerService();
                mcs.Initialize(NewMotionController.Id);
                NewMotionController = mcs.MotionController;
            }
        }

        public void DoRefreshCommand()
        {
            ObservableCollection<MotionControllerModel> existingControllers = new ObservableCollection<MotionControllerModel>();
            AvailableMotionControllers = new ObservableCollection<MotionControllerModel>();
            NewMotionController = null;
            NewControllersDetected = false;

            int connectedCount = io.thp.psmove.pinvoke.count_connected();
            if(connectedCount > 0)
            {
                foreach (MotionControllerViewModel mcvw in SimpleIoc.Default.GetAllInstances<MotionControllerViewModel>())
                {
                    existingControllers.Add(mcvw.MotionController);
                }

                MotionControllerService motionControllerService = new MotionControllerService();
                for (int i = 0; i < connectedCount; i++)
                {
                    motionControllerService.Initialize(i);
                    foreach(MotionControllerModel mcw in existingControllers)
                    {
                        if(motionControllerService.MotionController.ConnectStatus == UniMove.PSMoveConnectStatus.OK)
                        {
                            if (!motionControllerService.MotionController.Serial.Equals(mcw.Serial))
                            {
                                if (existingControllers.IndexOf(mcw) == existingControllers.Count - 1)
                                {
                                    AvailableMotionControllers.Add(motionControllerService.MotionController);
                                }
                            }
                        }
                    }
                }

                if(AvailableMotionControllers.Count > 0)
                {
                    NewControllersDetected = true;
                    NewMotionController = AvailableMotionControllers[0];
                }
            }
        }
        #endregion
    }
}