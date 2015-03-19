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

        public CamerasModel CamerasModel { get; private set; }

        public CameraPositioningCalibrationService PositioningService { get; private set; }

        public ISettingsService SettingsService { get; private set; }

        /// <summary>
        /// Initializes a new instance of the CameraPositioningCalibrationViewModel class.
        /// </summary>
        public CameraPositioningCalibrationViewModel(CamerasModel camerasModel)
        {
            if (IsInDesignMode)
            {
                CamerasModel = camerasModel;
                SettingsService = new JsonSettingsService();
            }
            else
            {
                CamerasModel = camerasModel;
                PositioningService = new CameraPositioningCalibrationService(camerasModel.Cameras);
                SettingsService = SimpleIoc.Default.GetInstance<ISettingsService>();
            }
        }

        /// <summary>
        /// for design purposes only
        /// </summary>
        public CameraPositioningCalibrationViewModel() :
            this(new CamerasModel
            {
                
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
            foreach (CameraModel camera in CamerasModel.Cameras)
            {
                SettingsService.SaveCalibration(camera.Calibration);
            }
        }

        public void DoApply()
        {
            PositioningService.ApplyInput();
        }
        #endregion
    }
}