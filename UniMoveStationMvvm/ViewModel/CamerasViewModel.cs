using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using UniMoveStation.Helper;
using UniMoveStation.Model;

namespace UniMoveStation.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class CamerasViewModel : ViewModelBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

        public ObservableCollection<SingleCameraViewModel> Cameras
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the AllCamerasViewModel class.
        /// </summary>
        public CamerasViewModel()
        {
            Name = "all";
            Cameras = new ObservableCollection<SingleCameraViewModel>();
            Refresh();

            Messenger.Default.Register<AddCameraMessage>(this, message =>
            {
                Cameras.Add(SimpleIoc.Default.GetInstance<SingleCameraViewModel>(message.Camera.GUID));
            });
            Messenger.Default.Register<RemoveCameraMessage>(this, message =>
            {
                if(SimpleIoc.Default.ContainsCreated<SingleCameraViewModel>(message.Camera.GUID))
                {
                    Cameras.Remove(SimpleIoc.Default.GetInstance<SingleCameraViewModel>(message.Camera.GUID));
                }
            });
        }

        public void Refresh()
        {
            Cameras.Clear();
            foreach (SingleCameraViewModel scvm in SimpleIoc.Default.GetAllCreatedInstances<SingleCameraViewModel>())
            {
                Cameras.Add(scvm);
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }
    } // CamerasViewModel
} // namespace