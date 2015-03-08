using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;

namespace UniMoveStation.Representation.ViewModel.Dialog
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class CameraPositioningCalibrationViewModel : ViewModelBase
    {
        private RelayCommand _applyCommand;
        private RelayCommand _saveCommand;
        private bool _inputAnglesManually;

        public ObservableCollection<CameraViewModel> Cameras { get; private set; }

        public CameraPositioningCalibrationService PositioningService { get; set; }

        public JsonSettingsService SettingsService { get; set; }

        /// <summary>
        /// Initializes a new instance of the CameraPositioningCalibrationViewModel class.
        /// </summary>
        public CameraPositioningCalibrationViewModel(ObservableCollection<CameraViewModel> cameras)
        {
            Cameras = cameras;
            PositioningService = new CameraPositioningCalibrationService(
                new ObservableCollection<CameraModel>(Cameras.Select(element => element.Camera)));
            SettingsService = new JsonSettingsService();
        }


        public bool InputAnglesManually
        {
            get { return _inputAnglesManually; }
            set { Set(() => InputAnglesManually, ref _inputAnglesManually, value); }
        }

        #region Commands
        /// <summary>
        /// Gets the ApplyCommand.
        /// </summary>
        public RelayCommand ApplyCommand
        {
            get
            {
                return _applyCommand
                    ?? (_applyCommand = new RelayCommand(DoApply));
            }
        }

        /// <summary>
        /// Gets the SaveCommand.
        /// </summary>
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand
                    ?? (_saveCommand = new RelayCommand(DoSave));
            }
        }
        #endregion

        #region Command Executions
        public void DoSave()
        {
            foreach (CameraViewModel cameraViewModel in Cameras)
            {
                SettingsService.SaveCalibration(cameraViewModel.Camera);
            }
        }

        public void DoApply()
        {
            PositioningService.ApplyInput();
        }
        #endregion
    }
}