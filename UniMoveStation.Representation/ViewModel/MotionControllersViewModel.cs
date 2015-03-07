using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Common.SharpMove;
using UniMoveStation.Representation.MessengerMessage;
using UniMoveStation.Representation.ViewModel.Flyout;

namespace UniMoveStation.Representation.ViewModel
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
        private RelayCommand<bool> _toggleControllersCommand;

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
        /// Initializes a new instance of the MotionControllersViewModel class.
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


            if (IsInDesignMode)
            {
                Controllers.Add(new MotionControllerViewModel());
                Controllers.Add(new MotionControllerViewModel());
                Controllers.Add(new MotionControllerViewModel());
            }
            else
            {
                if (SimpleIoc.Default.GetInstance<SettingsViewModel>().Settings.LoadControllersOnStartUp)
                {
                    AddAvailableMotionControllers();
                }
                if(SimpleIoc.Default.GetInstance<SettingsViewModel>().Settings.Debug)
                {   
                    new MotionControllerViewModel();
                    new MotionControllerViewModel();
                }
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
        /// <summary>
        /// Gets the ToggleControllersCommand.
        /// </summary>
        public RelayCommand<bool> ToggleControllersCommand
        {
            get
            {
                return _toggleControllersCommand
                    ?? (_toggleControllersCommand = new RelayCommand<bool>(DoToggleControllers));
            }
        }
        #endregion

        #region Command Exeuctions
        public void DoToggleControllers(bool enabled)
        {
            foreach(MotionControllerViewModel mcvw in Controllers)
            {
                mcvw.DoToggleConnection(enabled);
            }
        }
        #endregion
    } // CamerasViewModel
} // namespace