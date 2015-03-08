using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using UniMoveStation.Business.Nito;
using UniMoveStation.Representation.MessengerMessage;
using UniMoveStation.Representation.ViewModel.Flyout;

namespace UniMoveStation.Representation.ViewModel
{
    public class NavigationViewModel : ViewModelBase
    {
        private int _lastSelectedIndex;
        private int _motionControllerCount;
        private int _cameraCount;
        private RelayCommand<TabControl> _tabSelectedCommand;
        private RelayCommand<Object> _addCommand;
        private RelayCommand<object> _removeCommand;

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the NavigationViewModel class.
        /// </summary>
        [PreferredConstructor]
        public NavigationViewModel()
        {
            CameraTabs = new ObservableCollection<object>();
            CameraTabs.CollectionChanged += (obj, args) =>
            {
                CameraCount = Math.Max(CameraTabs.Count - 1, 0);
            };
            MotionControllerTabs = new ObservableCollection<object>();
            MotionControllerTabs.CollectionChanged += (obj, args) =>
            {
                MotionControllerCount = Math.Max(MotionControllerTabs.Count - 1, 0);
            };

            ServerTabs = new ObservableCollection<object>();

            Messenger.Default.Register<AddMotionControllerMessage>(this,
                message =>
                {
                    MotionControllerTabs.Add(SimpleIoc.Default.GetInstance<MotionControllerViewModel>(message.MotionController.Serial));
                });

            Messenger.Default.Register<RemoveMotionControllerMessage>(this,
                message =>
                {
                    MotionControllerTabs.Remove(SimpleIoc.Default.GetInstance<MotionControllerViewModel>(message.MotionController.Serial));
                });

            Messenger.Default.Register<AddCameraMessage>(this,
                message =>
                {
                    CameraTabs.Add(SimpleIoc.Default.GetInstance<CameraViewModel>(message.Camera.GUID));
                });

            Messenger.Default.Register<RemoveCameraMessage>(this,
                message =>
                {
                    CameraTabs.Remove(SimpleIoc.Default.GetInstance<CameraViewModel>(message.Camera.GUID));
                });

            if (IsInDesignMode)
            {
                MotionControllerTabs.Add(new MotionControllerViewModel());
                MotionControllerTabs.Add(new MotionControllerViewModel());
                CameraTabs.Add(new CameraViewModel());
                CameraTabs.Add(new CameraViewModel());
                CameraTabs.Add(new CameraViewModel());
                CameraTabs.Add(new CameraViewModel());
            }

            Refresh();
        }
        #endregion

        #region Properties
        public int MotionControllerCount
        {
            get { return _motionControllerCount; }
            set { Set(() => MotionControllerCount, ref _motionControllerCount, value); }
        }

        public int CameraCount
        {
            get { return _cameraCount; }
            set { Set(() => CameraCount, ref _cameraCount, value); }
        }

        public ObservableCollection<object> CameraTabs
        {
            get;
            private set;
        }

        public ObservableCollection<object> MotionControllerTabs
        {
            get;
            private set;
        }

        public ObservableCollection<object> ServerTabs
        {
            get;
            private set;
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the TabSelectedCommand.
        /// </summary>
        public RelayCommand<TabControl> TabSelectedCommand
        {
            get
            {
                return _tabSelectedCommand
                    ?? (_tabSelectedCommand = new RelayCommand<TabControl>(
                    tabControl =>
                    {
                        if(tabControl.SelectedIndex == tabControl.Items.Count - 1)
                        {
                            tabControl.SelectedIndex = _lastSelectedIndex;
                        }
                        else
                        {
                            _lastSelectedIndex = tabControl.SelectedIndex;
                        }
                    }));
            }
        }

        /// <summary>
        /// Gets the AddCommand.
        /// </summary>
        public RelayCommand<Object> AddCommand
        {
            get
            {
                return _addCommand
                    ?? (_addCommand = new RelayCommand<Object>(
                    tag =>
                    {
                        if (tag.ToString().Equals("controllers"))
                        {
                            AddMotionControllerViewModel amcvm = SimpleIoc.Default.GetInstance<AddMotionControllerViewModel>();
                            amcvm.DoRefresh();
                            Messenger.Default.Send(new ToggleFlyoutMessage(amcvm));
                        }
                        else if (tag.ToString().Equals("cameras"))
                        {
                            AddCameraViewModel acvm = SimpleIoc.Default.GetInstance<AddCameraViewModel>();
                            acvm.DoRefresh();
                            Messenger.Default.Send(new ToggleFlyoutMessage(acvm));
                        }
                        else if (tag.ToString().Equals("server"))
                        {
                            if(SimpleIoc.Default.GetInstance<ServerViewModel>().Server.Enabled)
                            {
                                NitoClient client = new NitoClient();
                                client.Connect("127.0.0.1", 3000);
                            }
                        }
                    }));
            }
        }
        /// <summary>
        /// Gets the RemoveCommand.
        /// </summary>
        public RelayCommand<object> RemoveCommand
        {
            get
            {
                return _removeCommand
                    ?? (_removeCommand = new RelayCommand<object>(DoRemove));
            }
        }
        #endregion

        #region Command Executions
        public void DoRemove(object obj)
        {
            if(obj is MotionControllerViewModel)
            {
                MotionControllerViewModel mcvw = (MotionControllerViewModel) obj;
                mcvw.Cleanup();
            }
            else if (obj is CameraViewModel)
            {
                CameraViewModel scvw = (CameraViewModel) obj;
                scvw.Cleanup();
            }
        }
        #endregion

        public void Refresh()
        {
            {
                CameraTabs.Clear();
                CameraTabs.Add(SimpleIoc.Default.GetInstance<CamerasViewModel>());
                foreach (CameraViewModel scvm in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
                {
                    CameraTabs.Add(scvm);
                }
                IEditableCollectionView itemsView = (IEditableCollectionView)CollectionViewSource.GetDefaultView(CameraTabs);
                itemsView.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;
            }

            {
                MotionControllerTabs.Clear();
                MotionControllerTabs.Add(SimpleIoc.Default.GetInstance<MotionControllersViewModel>());
                foreach (MotionControllerViewModel mcvm in SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>())
                {
                    MotionControllerTabs.Add(mcvm);
                }
                IEditableCollectionView itemsView = (IEditableCollectionView)CollectionViewSource.GetDefaultView(MotionControllerTabs);
                itemsView.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;
            }

            {
                ServerTabs.Clear();
                ServerTabs.Add(SimpleIoc.Default.GetInstance<ServerViewModel>());
                IEditableCollectionView itemsView = (IEditableCollectionView)CollectionViewSource.GetDefaultView(ServerTabs);
                itemsView.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;
            }
        } // Refresh
    } // NavigationViewModel
} // namespace