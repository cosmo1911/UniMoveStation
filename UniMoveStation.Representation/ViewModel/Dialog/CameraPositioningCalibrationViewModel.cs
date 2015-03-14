using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;
using UniMoveStation.Business.Service.Interfaces;

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

        public ISettingsService SettingsService { get; set; }

        /// <summary>
        /// Initializes a new instance of the CameraPositioningCalibrationViewModel class.
        /// </summary>
        public CameraPositioningCalibrationViewModel(ObservableCollection<CameraViewModel> cameras)
        {
            if (IsInDesignMode)
            {
                Cameras = cameras;
                SettingsService = new JsonSettingsService();
            }
            else
            {
                Cameras = cameras;
                ObservableCollection<CameraModel> cameraModels = new ObservableCollection<CameraModel>(cameras.Select(viewModel => viewModel.Camera));
                PositioningService = new CameraPositioningCalibrationService(cameraModels);
                SettingsService = SimpleIoc.Default.GetInstance<ISettingsService>();
            }
        }

        /// <summary>
        /// for design purposes only
        /// </summary>
        public CameraPositioningCalibrationViewModel()
            : this(new ObservableCollection<CameraViewModel>
            {
                new CameraViewModel(),
                new CameraViewModel(),
                new CameraViewModel(),
                new CameraViewModel()
            })
        {
            // empty
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
                SettingsService.SaveCalibration(cameraViewModel.Camera.Calibration);
            }
        }

        public void DoApply()
        {
            PositioningService.ApplyInput();
        }
        #endregion
    }
}