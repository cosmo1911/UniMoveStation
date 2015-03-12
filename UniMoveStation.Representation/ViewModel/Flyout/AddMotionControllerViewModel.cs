using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;
using UniMoveStation.Business.PsMove;

namespace UniMoveStation.Representation.ViewModel.Flyout
{
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
            get { return _availableMotionControllers; }
            set { Set(() => AvailableMotionControllers, ref _availableMotionControllers, value); }
        }
        public MotionControllerModel NewMotionController
        {
            get { return _newMotionController; }
            private set { Set(() => NewMotionController, ref _newMotionController, value); }
        }

        public bool NewControllersDetected
        {
            get { return _newControllersDetected; }
            set { Set(() => NewControllersDetected, ref _newControllersDetected, value); }
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
            AvailableMotionControllers = new ObservableCollection<MotionControllerModel>();
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
        /// Gets the CreateCommand.
        /// </summary>
        public RelayCommand CreateCommand
        {
            get
            {
                return _createCommand
                    ?? (_createCommand = new RelayCommand(DoCreate));
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
                    ?? (_refreshCommand = new RelayCommand(DoRefresh));
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
                    ?? (_selectItemCommand = new RelayCommand<object>(DoSelectItem));
            }
        }
        #endregion

        #region Command Executions
        public void DoCancel()
        {
            IsOpen = false;
        }

        public void DoCreate()
        {
            if (NewMotionController != null)
            {
                MotionControllerViewModel mcvw;
                if (NewMotionController.ConnectStatus != PSMoveConnectStatus.Unknown)
                {
                    mcvw = new MotionControllerViewModel(
                        NewMotionController, 
                        new MotionControllerService());
                    
                }
                else
                {
                    mcvw = new MotionControllerViewModel();
                }
                IsOpen = false;
            }
        }

        public void DoSelectItem(object item)
        {
            if(item != null)
            {
                string tmp = NewMotionController.Name;
                NewMotionController = (MotionControllerModel)item;
                NewMotionController.Name = tmp;
                MotionControllerService mcs = new MotionControllerService();
                mcs.Initialize(NewMotionController);
            }
        }

        public void DoRefresh()
        {
            ObservableCollection<MotionControllerModel> existingControllers = new ObservableCollection<MotionControllerModel>();
            AvailableMotionControllers.Clear();
            NewMotionController = new MotionControllerModel();
            NewMotionController.Name = null;
            NewControllersDetected = false;

            int connectedCount = PsMoveApi.count_connected();
            if(connectedCount > 0)
            {
                foreach (MotionControllerViewModel mcvw in SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>())
                {
                    existingControllers.Add(mcvw.MotionController);
                }

                MotionControllerService motionControllerService = new MotionControllerService();
                for (int i = 0; i < connectedCount; i++)
                {
                    MotionControllerModel tmp = motionControllerService.Initialize(i);

                    if(existingControllers.Count > 0)
                    {
                        bool duplicate = false;
                        foreach (MotionControllerModel mcw in existingControllers)
                        {
                            if (tmp.ConnectStatus == PSMoveConnectStatus.OK)
                            {
                                if (tmp.Serial.Equals(mcw.Serial))
                                {
                                    duplicate = true;
                                    break;
                                }
                            }
                        } // end foreach
                        if (!duplicate) AvailableMotionControllers.Add(tmp);
                    }
                    else
                    {
                        if (tmp.ConnectStatus == PSMoveConnectStatus.OK)
                        {
                            AvailableMotionControllers.Add(tmp);
                        }
                    }
                } // end for
            }

            if (AvailableMotionControllers.Count > 0) NewControllersDetected = true;
        } // DoRefresh
        #endregion
    } // AddMotionControllerViewModel
} // namespace