using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using UniMoveStation.Helper;
using UniMoveStation.Model;
using UniMoveStation.Service;
using UniMoveStation.SharpMove;
using UniMoveStation.ViewModel.Flyout;

namespace UniMoveStation.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MotionControllersViewModel : ViewModelBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

        public ObservableCollection<MotionControllerViewModel> Controllers
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the AllCamerasViewModel class.
        /// </summary>
        public MotionControllersViewModel()
        {

            Name = "all";
            Controllers = new ObservableCollection<MotionControllerViewModel>();
            Refresh();

            Messenger.Default.Register<AddMotionControllerMessage>(this,
                message =>
                {
                    Controllers.Add(SimpleIoc.Default.GetInstance<MotionControllerViewModel>(message.MotionController.Serial));
                });

            Messenger.Default.Register<RemoveMotionControllerMessage>(this,
                message =>
                {
                    Controllers.Remove(SimpleIoc.Default.GetInstance<MotionControllerViewModel>(message.MotionController.Serial));
                });

            if (SimpleIoc.Default.GetInstance<SettingsViewModel>().Settings.LoadCamerasOnStartUp)
            {
                AddAvailableMotionControllers();
            }
        }

        public void AddAvailableMotionControllers()
        {

            int count = PsMoveApi.count_connected();

            for(int i = 0; i < count; i++)
            {
                IMotionControllerService mcs = new MotionControllerService();
                MotionControllerModel mc = mcs.Initialize(i);
                mc.Name = "MC " + i;
                new MotionControllerViewModel(mc, mcs);
            }
            Refresh();
        }

        public void Refresh()
        {
            Controllers.Clear();
            foreach (MotionControllerViewModel mcvm in SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>())
            {
                Controllers.Add(mcvm);
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        #region Commands
        
        #endregion

        #region Command Exeuctions
        
        #endregion
    } // CamerasViewModel
} // namespace