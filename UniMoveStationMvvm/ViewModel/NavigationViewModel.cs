using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UniMoveStation.Model;

namespace UniMoveStation.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NavigationViewModel : ViewModelBase
    {
        private ObservableCollection<object> _cameraTabs;
        private ObservableCollection<object> _motionControllerTabs;
        private ObservableCollection<SingleCameraViewModel> _singleCameras;
        private int lastSelectedIndex = 0;

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the NavigationViewModel class.
        /// </summary>
        [PreferredConstructor]
        public NavigationViewModel()
        {
            SingleCameras.Add(new SingleCameraViewModel(0));
            SingleCameras.Add(new SingleCameraViewModel(1337));
            SingleCameras.Add(new SingleCameraViewModel(1));

            MotionControllerModel motionController = new MotionControllerModel();
            motionController.Name = "name";
            motionController.Serial = "name";
            SimpleIoc.Default.Register<MotionControllerViewModel>(
                () => new MotionControllerViewModel(motionController),
                motionController.Serial,
                true);
        }
        #endregion

        #region Properties


        public ObservableCollection<object> CameraTabs
        {
            get
            {
                ObservableCollection<object> collection = new ObservableCollection<object>();
                collection.Add(AllCameras);
                foreach (SingleCameraViewModel scvm in SimpleIoc.Default.GetAllInstances<SingleCameraViewModel>())
                {
                    collection.Add(scvm);
                }
                IEditableCollectionView itemsView = (IEditableCollectionView) CollectionViewSource.GetDefaultView(collection);
                itemsView.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;
                return collection;
            }
            private set
            {
                Set(() => CameraTabs, ref _cameraTabs, value);
            }
        }

        public ObservableCollection<SingleCameraViewModel> SingleCameras
        {
            get
            {
                ObservableCollection<SingleCameraViewModel> collection = new ObservableCollection<SingleCameraViewModel>();
                foreach(SingleCameraViewModel scvm in SimpleIoc.Default.GetAllInstances<SingleCameraViewModel>())
                {
                    collection.Add(scvm);
                }
                return collection;
            }
            private set
            {
                Set(() => SingleCameras, ref _singleCameras, value);
            }
        }

        public AllCamerasViewModel AllCameras
        {
            get
            {
                return SimpleIoc.Default.GetInstance<AllCamerasViewModel>();
            }
        }

        public ObservableCollection<object> MotionControllerTabs
        {
            get
            {
                if(_motionControllerTabs == null)
                {
                    MotionControllerTabs = new ObservableCollection<object>();
                }
                return _motionControllerTabs;
            }
            private set
            {
                ObservableCollection<object> collection = new ObservableCollection<object>();
                collection.Add(AllCameras);
                foreach (MotionControllerViewModel mcvm in SimpleIoc.Default.GetAllInstances<MotionControllerViewModel>())
                {
                    collection.Add(mcvm);
                }
                IEditableCollectionView itemsView = (IEditableCollectionView)CollectionViewSource.GetDefaultView(collection);
                itemsView.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;

                Set(() => MotionControllerTabs, ref _motionControllerTabs, collection);
            }
        }

        #endregion

        #region Relay Commands
        private RelayCommand<MetroTabControl> _tabSelectedCommand;

        /// <summary>
        /// Gets the TabSelectedCommand.
        /// </summary>
        public RelayCommand<MetroTabControl> TabSelectedCommand
        {
            get
            {
                return _tabSelectedCommand
                    ?? (_tabSelectedCommand = new RelayCommand<MetroTabControl>(
                    tabControl =>
                    {
                        if(tabControl.SelectedIndex == tabControl.Items.Count - 1)
                        {
                            tabControl.SelectedIndex = lastSelectedIndex;
                        }
                        else
                        {
                            lastSelectedIndex = tabControl.SelectedIndex;
                        }
                        Console.WriteLine(tabControl.SelectedIndex);
                    }));
            }
        }

        private RelayCommand<Object> _addCommand;

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
                            ViewModelLocator.Instance.Main.DoToggleFlyout(ViewModelLocator.Instance.AddMotionController);
                            ViewModelLocator.Instance.AddMotionController.DoRefreshCommand();
                        }
                    }));
            }
        }


        #endregion
    }
}