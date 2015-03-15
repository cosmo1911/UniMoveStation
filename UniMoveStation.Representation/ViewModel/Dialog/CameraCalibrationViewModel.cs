using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;
using UniMoveStation.Common;

namespace UniMoveStation.Representation.ViewModel.Dialog
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class CameraCalibrationViewModel : ViewModelBase
    {
        private CameraModel _camera;
        private CameraCalibrationService _calibrationService;
        private RelayCommand _cancelCalibrationCommand;
        private RelayCommand _startCalibrationCommand;
        private RelayCommand _saveCalibrationCommand;

        public CameraModel Camera
        {
            get { return _camera; }
            set
            {
                if (_calibrationService != null)
                {
                    _calibrationService.Initialize(value);
                }
                Set(() => Camera, ref _camera, value);
            }
        }

        public CameraCalibrationService CalibrationService
        {
            get { return _calibrationService; }
            set { Set(() => CalibrationService, ref _calibrationService, value); }
        }
        
        public CameraCalibrationViewModel()
        {
            if (IsInDesignMode)
            {
                Camera = new CameraModel();
                Camera.Calibration = new CameraCalibrationModel
                {
                    CurrentMode = CameraCalibrationMode.Calibrated,
                    FrameBufferSize = 100,
                    Error = 0.0,
                };
            }
            else
            {
                if (SimpleIoc.Default.IsRegistered<CameraCalibrationService>())
                {
                    CalibrationService = SimpleIoc.Default.GetInstance<CameraCalibrationService>();
                }
                else
                {
                    CalibrationService = new CameraCalibrationService(new JsonSettingsService());
                }
            }
        }

        #region Commands
        /// <summary>
        /// Gets the ButtonCommand.
        /// </summary>
        public RelayCommand CancelCalibrationCommand
        {
            get
            {
                return _cancelCalibrationCommand
                    ?? (_cancelCalibrationCommand = new RelayCommand(DoCancelCalibration));
            }
        }

        /// <summary>
        /// Gets the StartCalibrationCommand.
        /// </summary>
        public RelayCommand StartCalibrationCommand
        {
            get
            {
                return _startCalibrationCommand
                    ?? (_startCalibrationCommand = new RelayCommand(
                        DoStartCalibration,
                        () => Camera.Calibration.StartFlag == false && Camera.TrackerId == 0));
            }
        }

        /// <summary>
        /// Gets the SaveCommand.
        /// </summary>
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCalibrationCommand
                    ?? (_saveCalibrationCommand = new RelayCommand(
                        DoSaveCalibration,
                        () => Camera.Calibration.CurrentMode == CameraCalibrationMode.Calibrated));
            }
        }
        #endregion

        #region Command Executions

        public void DoCancelCalibration()
        {
            _calibrationService.CancelCalibration();
        }

        public void DoStartCalibration()
        {
            _calibrationService.StartCalibration();
        }

        public void DoSaveCalibration()
        {
            _calibrationService.SaveCalibration();
        }
        #endregion
    }
}