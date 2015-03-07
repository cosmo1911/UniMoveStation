using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniMoveStation.SharpMove;
using UniMoveStation.Model;
using UnityEngine;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;

namespace UniMoveStation.Service
{
    public class MotionControllerService : DependencyObject, IMotionControllerService
    {
        private MotionControllerModel _motionController;
        private PSMoveRemoteConfig _remoteConfig = PSMoveRemoteConfig.LocalAndRemote;
        private CancellationTokenSource _ctsUpdate;
        private CancellationTokenSource _ctsMagnetometerCalibration;

        public MotionControllerService()
        {
            PsMoveApi.psmove_set_remote_config(_remoteConfig);
        }

        public MotionControllerModel Initialize(int id)
        {
            _motionController = new MotionControllerModel();
            Init(id);

            return _motionController;
        }

        public MotionControllerModel Initialize(MotionControllerModel motionController)
        {
            return _motionController = motionController;
        }

        public void Start()
        {
            OnControllerDisconnected += MotionControllerService_OnControllerDisconnected;
            StartUpdateTask();
        }

        public void Stop()
        {
            OnControllerDisconnected -= MotionControllerService_OnControllerDisconnected;
            CancelUpdateTask();
            CancelMagnetometerCalibrationTask();
        }

        public void SetColor(Color color)
        {
            SetLED(color);
        }

        private void MotionControllerService_OnControllerDisconnected(object sender, EventArgs e)
        {
            Stop();
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
                        UpdateController();
                        if(GetButtonUp(PSMoveButton.PS))
                        {
                            Orient(PSMoveBool.True);
                        }
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
            Enabled = false;
        }
        #endregion

        #region Magnetometer Calibration Task
        public async void StartMagnetometerCalibrationTask(MetroWindow window)
        {
            CancelUpdateTask();
            _ctsMagnetometerCalibration = new CancellationTokenSource();

            var controller = await window.ShowProgressAsync("Magnetometer Calibration", null, true);
            CancellationToken token = _ctsMagnetometerCalibration.Token;
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
                            SetLED(new Color(r, g, b));
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
            if(_ctsMagnetometerCalibration != null)
            {
                _ctsMagnetometerCalibration.Cancel();
            }
        }

        public void CalibrateMagnetometer(MetroWindow window)
        {
            StartMagnetometerCalibrationTask(window);
        }
        #endregion

        #region Dependency Properties
        /// <summary>
        /// The <see cref="Enabled" /> dependency property's name.
        /// </summary>
        public const string EnabledPropertyName = "Enabled";

        /// <summary>
        /// Gets or sets the value of the <see cref="Enabled" />
        /// property. This is a dependency property.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Enabled" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.Register(
            EnabledPropertyName,
            typeof(bool),
            typeof(MotionControllerService),
            new UIPropertyMetadata(default(bool)));

        #endregion

        #region SharpMotionController
        private uint prevButtons;
        private uint currentButtons;
        private float timeElapsed;
        private bool disconnected;

        public Vector3 m_position = Vector3.zero;
        public Vector3[] m_positionHistory = new Vector3[5];
        public Vector3 m_positionFix = Vector3.zero;
        public Vector3 m_positionScalePos = Vector3.one * 0.01f;
        public Vector3 m_positionScaleNeg = -Vector3.one * 0.01f;

        private Quaternion m_orientation = Quaternion.identity;
        private Quaternion m_orientationFix = Quaternion.identity;

        /// <summary>
        /// Event fired when the controller disconnects unexpectedly (i.e. on going out of range).
        /// </summary>
        public event EventHandler OnControllerDisconnected;

        /// <summary>
        /// Returns whether the connecting succeeded or not.
        /// 
        /// NOTE! This function does NOT pair the controller by Bluetooth.
        /// If the controller is not already paired, it can only be connected by USB.
        /// See README for more information.
        /// </summary>
        public PSMoveConnectStatus Init(int index)
        {
            _motionController.Handle = PsMoveApi.psmove_connect_by_id(index);

            // Error check the result!
            if (_motionController.Handle == IntPtr.Zero)
                return _motionController.ConnectStatus = PSMoveConnectStatus.Error;

            // Make sure the connection is actually sending data. If not, this is probably a controller 
            // you need to remove manually from the OSX Bluetooth Control Panel, then re-connect.
            if (PsMoveApi.psmove_update_leds(_motionController.Handle) == 0)
                return _motionController.ConnectStatus = PSMoveConnectStatus.NoData;

            _motionController.Id = index;
            _motionController.Remote = PsMoveApi.psmove_is_remote(_motionController.Handle) > 0;
            _motionController.ConnectionType = PsMoveApi.psmove_connection_type(_motionController.Handle); ;

            StringBuilder builder = new StringBuilder(64);
            PsMoveApi.get_moved_host(_motionController.Handle, builder);
            _motionController.HostIp = builder.ToString();
            PsMoveApi.get_serial(_motionController.Handle, builder);
            _motionController.Serial = builder.ToString();
            UpdateController();

            return _motionController.ConnectStatus = PSMoveConnectStatus.OK;
        }

        public void Update()
        {
            UpdateControllerRateLimited();
        }

        public void Disconnect()
        {
            if (_motionController.Handle != IntPtr.Zero)
            {
                SetLED(0, 0, 0);
                _motionController.Rumble = 0;
                PsMoveApi.psmove_disconnect(_motionController.Handle);
                _motionController.Handle = IntPtr.Zero;
                disconnected = true;
                Console.WriteLine("move destroyed");
            }
        }

        void UpdateControllerRateLimited()
        {
            if (disconnected) return;

            // we want to update the previous buttons outside the update restriction so,
            // we only get one button event pr. unity update frame
            prevButtons = currentButtons;

            timeElapsed += Time.deltaTime;

            // Here we manually enforce updates only every updateRate amount of time
            // The reason we don't just do this in FixedUpdate is so the main program's FixedUpdate rate 
            // can be set independently of the controllers' update rate.

            if (timeElapsed < _motionController.UpdateRate) return;
            timeElapsed = 0.0f;
            UpdateController();
        }

        public void UpdateController()
        {
            uint buttons = 0;

            prevButtons = currentButtons;
            // NOTE! There is potentially data waiting in queue. 
            // We need to poll *all* of it by calling psmove_poll() until the queue is empty. Otherwise, data might begin to build up.
            while (PsMoveApi.psmove_poll(_motionController.Handle) > 0)
            {
                // We are interested in every button press between the last update and this one:
                buttons = buttons | PsMoveApi.psmove_get_buttons(_motionController.Handle);

                // The events are not really working from the PS Move Api. So we do our own with the prevButtons
                //psmove_get_button_events(handle, ref pressed, ref released);
            }
            currentButtons = buttons;

            // For acceleration, gyroscope, and magnetometer values, we look at only the last value in the queue.
            // We could in theory average all the acceleration (and other) values in the queue for a "smoothing" effect, but we've chosen not to.
            ProcessData();

            // Send a report to the controller to update the LEDs and rumble.
            if (PsMoveApi.psmove_update_leds(_motionController.Handle) == 0)
            {
                //			Debug.Log ("led set");
                // If it returns zero, the controller must have disconnected (i.e. out of battery or out of range),
                // so we should fire off any events and disconnect it.
                if (this != null)
                {
                    OnControllerDisconnected(this, new EventArgs());
                }
                Disconnect();
            }
        }


        /// <summary>
        /// Sets the LED color
        /// </summary>
        /// <param name="color">Unity's Color type</param>
        public void SetLED(Color color)
        {
            SetLED((int)(color.r * 255), (int)(color.g * 255), (int)(color.b * 255));
        }

        /// <summary>
        /// Sets the LED color
        /// </summary>
        /// <param name="r">Red value of the LED color (0-255)</param>
        /// <param name="g">Green value of the LED color (0-255)</param>
        /// <param name="b">Blue value of the LED color (0-255)</param>
        public void SetLED(int r, int g, int b)
        {
            _motionController.Color = new Color(r / 255f, g / 255f, b / 255f);
            if (!disconnected)
            {
                PsMoveApi.psmove_set_leds(_motionController.Handle, (byte)r, (byte)g, (byte)b);
            }
        }

        public bool Orient(PSMoveBool enable)
        {
            bool oriented = false;
            PsMoveApi.psmove_enable_orientation(_motionController.Handle, enable);
            if (enable == PSMoveBool.True)
            {
                PsMoveApi.psmove_reset_orientation(_motionController.Handle);

                oriented = PsMoveApi.psmove_has_orientation(_motionController.Handle) == PSMoveBool.True;
                if (oriented)
                {
                    float qw = 0.0f, qx = 0.0f, qy = 0.0f, qz = 0.0f;
                    PsMoveApi.psmove_get_orientation(_motionController.Handle, out qw, out qx, out qy, out qz);
                    m_orientationFix = new Quaternion(-qx, -qy, -qz, qw);
                }
            }
            return oriented;
        }

        private void UpdateButtons()
        {
            _motionController.Circle = GetButton(PSMoveButton.Circle);
            _motionController.Cross = GetButton(PSMoveButton.Cross);
            _motionController.Triangle = GetButton(PSMoveButton.Triangle);
            _motionController.Square = GetButton(PSMoveButton.Square);
            _motionController.Move = GetButton(PSMoveButton.Move);
            _motionController.PS = GetButton(PSMoveButton.PS);
            _motionController.Start = GetButton(PSMoveButton.Start);
            _motionController.Select = GetButton(PSMoveButton.Select);
            _motionController.Trigger = (PsMoveApi.psmove_get_trigger(_motionController.Handle));
        }

        /// <summary>
        /// Returns true if "button" is currently down.
        /// </summary
        public bool GetButton(PSMoveButton b)
        {
            if (disconnected) return false;

            return ((currentButtons & (uint)b) != 0);
        }

        /// <summary>
        /// Returns true if "button" is pressed down this instant.
        /// </summary
        public bool GetButtonDown(PSMoveButton b)
        {
            if (disconnected) return false;
            return ((prevButtons & (uint)b) == 0) && ((currentButtons & (uint)b) != 0);
        }

        /// <summary>
        /// Returns true if "button" is released this instant.
        /// </summary
        public bool GetButtonUp(PSMoveButton b)
        {
            if (disconnected) return false;

            return ((prevButtons & (uint)b) != 0) && ((currentButtons & (uint)b) == 0);
        }

        /// <summary>
        /// Process all the raw data on the Playstation Move controller
        /// </summary>
        protected virtual void ProcessData()
        {
            UpdateButtons();
            {
                int x = 0, y = 0, z = 0;

                PsMoveApi.psmove_get_accelerometer(_motionController.Handle, out x, out y, out z);

                _motionController.RawAccelerometer = new Vector3(x, y, z);
            }

            {
                float x = 0, y = 0, z = 0;
                PsMoveApi.psmove_get_accelerometer_frame(_motionController.Handle, PSMoveFrame.SecondHalf, out x, out y, out z);

                _motionController.Accelerometer = new Vector3(x, y, z);
            }

            {
                int x = 0, y = 0, z = 0;
                PsMoveApi.psmove_get_gyroscope(_motionController.Handle, out x, out y, out z);


                _motionController.RawGyroscope = new Vector3(x, y, z);
            }

            {
                float x = 0, y = 0, z = 0;
                PsMoveApi.psmove_get_gyroscope_frame(_motionController.Handle, PSMoveFrame.SecondHalf, out x, out y, out z);

                _motionController.Gyroscope = new Vector3(x, y, z);
            }

            if (PsMoveApi.psmove_has_orientation(_motionController.Handle) == PSMoveBool.True)
            {
                float w = 0.0f, x = 0.0f, y = 0.0f, z = 0.0f;
                PsMoveApi.psmove_get_orientation(_motionController.Handle, out w, out x, out y, out z);
                Quaternion rot = new Quaternion(x, y, z, w);
                rot = rot * m_orientationFix;
#if YISUP
                Vector3 euler = rot.eulerAngles;
                rot = Quaternion.Euler(-euler.x, -euler.y, euler.z);
#endif

                m_orientation = rot;
                _motionController.Orientation = rot;
            }
            else
            {
                m_orientation = Quaternion.identity;
            }

            {
                int x = 0, y = 0, z = 0;
                PsMoveApi.psmove_get_magnetometer(_motionController.Handle, out x, out y, out z);

                // TODO: Should these values be converted into a more human-understandable range?
                _motionController.Magnetometer = new Vector3(x, y, z);
            }

            _motionController.BatteryLevel = PsMoveApi.psmove_get_battery(_motionController.Handle);

            _motionController.Temperature = PsMoveApi.psmove_get_temperature_in_celsius(_motionController.Handle);
            PsMoveApi.psmove_set_rumble(_motionController.Handle, (byte)(_motionController.Rumble / 255f));
        }
        #endregion
    } // MotionControllerService
} // namespace
