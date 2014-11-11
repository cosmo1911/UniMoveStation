using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
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
        private CompositeCollection _cameras;
        private ObservableCollection<SingleCameraViewModel> _singleCameras;

        public CompositeCollection Cameras
        {
            get
            {
                CompositeCollection collection = new CompositeCollection();
                collection.Add(AllCameras);
                foreach (SingleCameraViewModel scvm in SimpleIoc.Default.GetAllInstances<SingleCameraViewModel>())
                {
                    collection.Add(scvm);
                }
                return collection;
            }
            private set
            {
                Set(() => Cameras, ref _cameras, value);
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

        /// <summary>
        /// Initializes a new instance of the NavigationViewModel class.
        /// </summary>
        [PreferredConstructor]
        public NavigationViewModel()
        {
            Cameras = new CompositeCollection();
            SelectTabCommand = new RelayCommand<MetroTabItem>(DoSelectTab);
            AddCameraCommand = new RelayCommand(DoAddCamera);

            new SingleCameraViewModel(0);
            new SingleCameraViewModel(1337);
            new SingleCameraViewModel(1);
        }

        /// <summary>
        /// Gets the SelectTabCommand.
        /// </summary>
        public RelayCommand<MetroTabItem> SelectTabCommand
        {
            get;
            private set;
        }

        private void DoSelectTab(MetroTabItem tabItem)
        {
            tabItem.IsSelected = true;
            MetroTabItem parent = (MetroTabItem)tabItem.OwningTabControl.Parent;
            parent.IsSelected = true;
        }

        /// <summary>
        /// Gets the AddCameraCommand.
        /// </summary>
        public RelayCommand AddCameraCommand
        {
            get;
            private set;
        }

        private void DoAddCamera()
        {
            Cameras.Add(new SingleCameraModel());
        }
    }
}