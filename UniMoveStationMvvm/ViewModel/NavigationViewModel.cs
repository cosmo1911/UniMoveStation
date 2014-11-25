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
        private int lastSelectedIndex = 0;

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the NavigationViewModel class.
        /// </summary>
        [PreferredConstructor]
        public NavigationViewModel()
        {
            CameraTabs = new ObservableCollection<object>();
            MotionControllerTabs = new ObservableCollection<object>();

            CameraTabs.Add(new SingleCameraViewModel());
            CameraTabs.Add(new SingleCameraViewModel());
            CameraTabs.Add(new SingleCameraViewModel());

            MotionControllerTabs.Add(new MotionControllerViewModel());
        }
        #endregion

        #region Properties


        public ObservableCollection<object> CameraTabs
        {
            get
            {
                if(_motionControllerTabs == null)
                {
                    CameraTabs = new ObservableCollection<object>();
                }
                return _cameraTabs;
            }
            private set
            {
                ObservableCollection<object> collection = new ObservableCollection<object>();
                collection.Add(SimpleIoc.Default.GetInstance<AllCamerasViewModel>());
                foreach (SingleCameraViewModel scvm in SimpleIoc.Default.GetAllInstances<SingleCameraViewModel>())
                {
                    collection.Add(scvm);
                }
                IEditableCollectionView itemsView = (IEditableCollectionView)CollectionViewSource.GetDefaultView(collection);
                itemsView.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;

                Set(() => CameraTabs, ref _cameraTabs, collection);
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
                //TODO replace with all motion controllers view model
                collection.Add(SimpleIoc.Default.GetInstance<AllCamerasViewModel>());
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
                            ViewModelLocator.Instance.AddMotionController.DoRefresh();
                        }
                        else if (tag.ToString().Equals("cameras"))
                        {
                            ViewModelLocator.Instance.Main.DoToggleFlyout(ViewModelLocator.Instance.AddCamera);
                            ViewModelLocator.Instance.AddCamera.DoRefreshCommand();
                        }
                    }));
            }
        }
        #endregion
    }
}