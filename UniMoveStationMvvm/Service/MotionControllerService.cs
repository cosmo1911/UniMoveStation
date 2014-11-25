using io.thp.psmove;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniMoveStation.SharpMove;
using UniMoveStation.Model;
using UnityEngine;
using GalaSoft.MvvmLight.Ioc;
using UniMoveStation.ViewModel;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace UniMoveStation.Service
{
    public class MotionControllerService : IMotionControllerService
    {
        private SharpMotionController _motionController;
        private PSMoveRemoteConfig _remoteConfig = PSMoveRemoteConfig.LocalAndRemote;
        private CancellationTokenSource _ctsUpdate;
        private CancellationTokenSource _ctsMagnetoMeterCalibration;

        public bool Enabled
        {
            get;
            set;
        }

        public MotionControllerModel Initialize(int id)
        {
            pinvoke.set_remote_config((int) _remoteConfig);

            _motionController = new SharpMotionController();

            _motionController.Init(id);

            return _motionController;
        }

        public void Initialize(MotionControllerModel motionController)
        {
            pinvoke.set_remote_config((int) _remoteConfig);

            _motionController = (SharpMotionController) motionController;
        }

        public void Start()
        {
            _motionController.OnControllerDisconnected += MotionControllerService_OnControllerDisconnected;
            StartUpdateTask();
        }

        public void Stop()
        {
            CancelUpdateTask();
            CancelMagnetometerCalibrationTask();
        }

        void MotionControllerService_OnControllerDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine(_motionController.Name + " disconnected");
        }

        #region Update Task
        private async void StartUpdateTask()
        {
            _ctsUpdate = new CancellationTokenSource();
            CancellationToken token = _ctsUpdate.Token;
            try
            {
                await Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        _motionController.UpdateController();
                        SimpleIoc.Default.GetInstance<MotionControllerViewModel>(
                            _motionController.Serial).MotionController = _motionController;
                        Thread.Sleep(new TimeSpan(0, 0, 0, 0, (int)(_motionController.UpdateRate * 1000)));
                    }
                });
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void CancelUpdateTask()
        {
            if (_ctsUpdate != null)
            {
                _ctsUpdate.Cancel();
            }
        }
        #endregion

        #region Magnetometer Calibration Task
        public async void StartMagnetometerCalibrationTask(MetroWindow window)
        {
            CancelUpdateTask();
            _ctsMagnetoMeterCalibration = new CancellationTokenSource();

            var controller = await window.ShowProgressAsync("Magnetometer Calibration", null, true);
            CancellationToken token = _ctsMagnetoMeterCalibration.Token;
            try
            {
                await Task.Run(() =>
                {
                    PsMoveApi.psmove_reset_magnetometer_calibration(_motionController.Handle);
                    int oldRange = 0;
                    bool calibrationFinished = false;
                    Color color = _motionController.Color;
                    while (!token.IsCancellationRequested && !calibrationFinished)
                    {
                        while(PsMoveApi.psmove_poll(_motionController.Handle) > 0)
                        {
                            float ax, ay, az;
                            PsMoveApi.psmove_get_magnetometer_vector(_motionController.Handle, out ax, out ay, out az);

                            int range = PsMoveApi.psmove_get_magnetometer_calibration_range(_motionController.Handle);
                            int percentage = 100 * range / 320;
                            if (percentage > 100) percentage = 100;
                            else if (percentage < 0) percentage = 0;
                            controller.SetProgress(percentage / 100.0);

                            float r = (color.r / 100) * percentage;
                            float g = (color.g / 100) * percentage;
                            float b = (color.b / 100) * percentage;
                            _motionController.SetLED(new UnityEngine.Color(r, g, b));
                            PsMoveApi.psmove_update_leds(_motionController.Handle);

                            if (controller.IsCanceled)
                            {
                                CancelMagnetometerCalibrationTask();
                            }

                            if (range >= 320)
                            {
                                if(oldRange > 0) {
                                    PsMoveApi.psmove_save_magnetometer_calibration(_motionController.Handle);
                                    calibrationFinished = true;
                                    break;
                                }
                            } else if(range > oldRange)
                            {
                                controller.SetMessage(string.Format("Rotate the controller in all directions: {0}%...", percentage));
                                oldRange = range;
                            }
                        }
                    }
                });
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            await controller.CloseAsync();

            if (controller.IsCanceled)
            {
                await window.ShowMessageAsync("Magnetometer Calibration", "Calibration has been cancelled.");
            } else
            {
                await window.ShowMessageAsync("Magnetometer Calibration", "Calibration finished successfully.");
            }
        }

        private void CancelMagnetometerCalibrationTask()
        {
            if(_ctsMagnetoMeterCalibration != null)
            {
                _ctsMagnetoMeterCalibration.Cancel();
            }
        }

        public void CalibrateMagnetometer(MetroWindow window)
        {
            StartMagnetometerCalibrationTask(window);
        }
        #endregion

        public void SetColor(Color color)
        {
            _motionController.SetLED(color);
        }
    } // MotionControllerService
} // namespace
