using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Common;

namespace UniMoveStation.Representation.ViewModel.Dialog
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class StereoCameraCalibrationViewModel  : ViewModelBase
    {
        private ObservableCollection<CameraModel> _cameras;
        private StereoCameraCalibrationModel _calibration;
        private StereoCameraCalibrationService _calibrationService;
        private IConsoleService _consoleService;

        private RelayCommand<object> _selectCamera1Command;
        private RelayCommand<object> _selectCamera2Command;
        private RelayCommand _cancelCalibrationCommand;
        private RelayCommand _startCalibrationCommand;
        private RelayCommand _saveCalibrationCommand;

        public ObservableCollection<CameraModel> _cameras1;
        public ObservableCollection<CameraModel> _cameras2;

        public ObservableCollection<CameraModel> Cameras1
        {
            get
            {
                if (_cameras1 == null)
                {
                    _cameras1 = new ObservableCollection<CameraModel>(_cameras);
                    _cameras1.Remove(Calibration.Camera2);
                }
                return _cameras1;
            }
        }

        public ObservableCollection<CameraModel> Cameras2
        {
            get
            {
                if (_cameras2 == null)
                {
                    _cameras2 = new ObservableCollection<CameraModel>(_cameras);
                    _cameras2.Remove(Calibration.Camera1);
                }
                return _cameras2;
            }
        }

        public StereoCameraCalibrationViewModel(ObservableCollection<CameraModel> cameras)
        {
            Cameras = cameras;
            if (IsInDesignMode)
            {

            }
            else
            {
                CalibrationService = new StereoCameraCalibrationService();
                ConsoleService = new ConsoleService();
                
                Calibration = new StereoCameraCalibrationModel()
                {
                    Camera1 = Cameras[0],
                    Camera2 = Cameras[1],
                };
                CalibrationService.Initialize(Calibration, ConsoleService);
            }
        }

        public ObservableCollection<CameraModel> Cameras
        {
            get { return _cameras; }
            set { Set(() => Cameras, ref _cameras, value); }
        }

        public StereoCameraCalibrationModel Calibration
        {
            get { return _calibration; }
            set { Set(() => Calibration, ref _calibration, value); }
        }

        public IConsoleService ConsoleService
        {
            get { return _consoleService; }
            set { Set(() => ConsoleService, ref _consoleService, value); }
        }

        public StereoCameraCalibrationService CalibrationService
        {
            get { return _calibrationService; }
            set { Set(() => CalibrationService, ref _calibrationService, value); }
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
                        () => true));
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
                        () => true));
            }
        }

        /// <summary>
        /// Gets the SelectCamera1Command.
        /// </summary>
        public RelayCommand<object> SelectCamera1Command
        {
            get
            {
                return _selectCamera1Command
                    ?? (_selectCamera1Command = new RelayCommand<object>(DoSelectCamera1));
            }
        }

        /// <summary>
        /// Gets the SelectCamera2Command.
        /// </summary>
        public RelayCommand<object> SelectCamera2Command
        {
            get
            {
                return _selectCamera2Command
                    ?? (_selectCamera2Command = new RelayCommand<object>(DoSelectCamera2));
            }
        }
        #endregion

        #region Command Executions

        public void DoCancelCalibration()
        {
            _calibrationService.StopCapture();
        }

        public void DoStartCalibration()
        {
            _calibrationService.Initialize(Calibration, new ConsoleService());
            _calibrationService.StartCapture();
        }

        public void DoSaveCalibration()
        {
            //_calibrationService.SaveCalibration();
        }

        private void DoSelectCamera1(object obj)
        {
            CameraModel model = obj as CameraModel;
            if (model != null)
            {
                Cameras2.Add(Calibration.Camera1);
                Cameras2.Remove(model);
                Calibration.Camera1 = model;
            }
        }

        private void DoSelectCamera2(object obj)
        {
            CameraModel model = obj as CameraModel;
            if (model != null)
            {
                Cameras1.Add(Calibration.Camera2);
                Cameras1.Remove(model);
                Calibration.Camera2 = model;
            }
        }

        #endregion
    }
}