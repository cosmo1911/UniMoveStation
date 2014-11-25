using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniMoveStation.Model;
using UnityEngine;

namespace UniMoveStation.SharpMove
{
    public class SharpMotionController : MotionControllerModel
    {
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
            Handle = PsMoveApi.psmove_connect_by_id(index);

            // Error check the result!
            if (Handle == IntPtr.Zero) 
                return ConnectStatus = PSMoveConnectStatus.Error;

            // Make sure the connection is actually sending data. If not, this is probably a controller 
            // you need to remove manually from the OSX Bluetooth Control Panel, then re-connect.
            if (PsMoveApi.psmove_update_leds(Handle) == 0) 
                return ConnectStatus = PSMoveConnectStatus.NoData;

            Id = index;
            Remote = PsMoveApi.psmove_is_remote(Handle) > 0;
            ConnectionType = PsMoveApi.psmove_connection_type(Handle); ;

            StringBuilder builder = new StringBuilder(64);
            PsMoveApi.get_moved_host(Handle, builder);
            HostIp = builder.ToString();
            PsMoveApi.get_serial(Handle, builder);
            Serial = builder.ToString();
            UpdateController();

            return ConnectStatus = PSMoveConnectStatus.OK;
        }

        public void Update()
        {
            UpdateControllerRateLimited();
        }

        public void Disconnect()
        {
            if (Handle != IntPtr.Zero)
            {
                SetLED(0, 0, 0);
                Rumble = 0;
                PsMoveApi.psmove_disconnect(Handle);
                Handle = IntPtr.Zero;
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

            if (timeElapsed < UpdateRate) return;
            else timeElapsed = 0.0f;
            UpdateController();
        }

        public void UpdateController()
        {

            uint buttons = 0;

            // NOTE! There is potentially data waiting in queue. 
            // We need to poll *all* of it by calling psmove_poll() until the queue is empty. Otherwise, data might begin to build up.
            while (PsMoveApi.psmove_poll(Handle) > 0)
            {
                // We are interested in every button press between the last update and this one:
                buttons = buttons | PsMoveApi.psmove_get_buttons(Handle);

                // The events are not really working from the PS Move Api. So we do our own with the prevButtons
                //psmove_get_button_events(handle, ref pressed, ref released);
            }
            currentButtons = buttons;

            // For acceleration, gyroscope, and magnetometer values, we look at only the last value in the queue.
            // We could in theory average all the acceleration (and other) values in the queue for a "smoothing" effect, but we've chosen not to.
            ProcessData();

            // Send a report to the controller to update the LEDs and rumble.
            if (PsMoveApi.psmove_update_leds(Handle) == 0)
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
            SetLED((int) (color.r * 255), (int) (color.g * 255), (int) (color.b * 255));
        }

        /// <summary>
        /// Sets the LED color
        /// </summary>
        /// <param name="r">Red value of the LED color (0-255)</param>
        /// <param name="g">Green value of the LED color (0-255)</param>
        /// <param name="b">Blue value of the LED color (0-255)</param>
        public void SetLED(int r, int g, int b)
        {
            Color = new Color(r / 255f, g / 255f, b / 255f);
            if (!disconnected)
            {
                PsMoveApi.psmove_set_leds(Handle, (byte)r, (byte)g, (byte)b);
            }
        }

        public bool Orient(PSMoveBool enable)
        {
            bool oriented = false;
            PsMoveApi.psmove_enable_orientation(Handle, enable);
            if (enable == PSMoveBool.True)
            {
                PsMoveApi.psmove_reset_orientation(Handle);

                oriented = PsMoveApi.psmove_has_orientation(Handle) == PSMoveBool.True;
                if (oriented)
                {
                    float qw = 0.0f, qx = 0.0f, qy = 0.0f, qz = 0.0f;
                    PsMoveApi.psmove_get_orientation(Handle, out qw, out qx, out qy, out qz);

                    Quaternion rot = new Quaternion(qx, qy, qz, qw);
                    m_orientationFix = Quaternion.Inverse(rot);
                }
            }
            return oriented;
        }



        private void UpdateButtons()
        {
            Circle = GetButton(PSMoveButton.Circle);
            Cross = GetButton(PSMoveButton.Cross);
            Triangle = GetButton(PSMoveButton.Triangle);
            Square = GetButton(PSMoveButton.Square);
            Move = GetButton(PSMoveButton.Move);
            PS = GetButton(PSMoveButton.PS);
            Start = GetButton(PSMoveButton.Start);
            Select = GetButton(PSMoveButton.Select);
            Trigger = (PsMoveApi.psmove_get_trigger(Handle));
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

                PsMoveApi.psmove_get_accelerometer(Handle, out x, out y, out z);

                RawAcceleration = new Vector3(x, y, z);
            }

            {
                float x = 0, y = 0, z = 0;
                PsMoveApi.psmove_get_accelerometer_frame(Handle, PSMoveFrame.SecondHalf, out x, out y, out z);

                Acceleration = new Vector3(x, y, z);
            }

            {
                int x = 0, y = 0, z = 0;
                PsMoveApi.psmove_get_gyroscope(Handle, out x, out y, out z);


                RawGyroscope = new Vector3(x, y, z);
            }

            {
                float x = 0, y = 0, z = 0;
                PsMoveApi.psmove_get_gyroscope_frame(Handle, PSMoveFrame.SecondHalf, out x, out y, out z);

                Gyroscope = new Vector3(x, y, z);
            }

            if (PsMoveApi.psmove_has_orientation(Handle) == PSMoveBool.True)
            {
                float w = 0.0f, x = 0.0f, y = 0.0f, z = 0.0f;
                PsMoveApi.psmove_get_orientation(Handle, out w, out x, out y, out z);

                Quaternion rot = new Quaternion(x, y, z, w);
                rot = rot * m_orientationFix;
#if YISUP
                Vector3 euler = rot.eulerAngles;
                rot = Quaternion.Euler(-euler.x, -euler.y, euler.z);
#endif

                m_orientation = rot;
            }
            else
            {
                m_orientation = Quaternion.identity;
            }

            {
                int x = 0, y = 0, z = 0;
                PsMoveApi.psmove_get_magnetometer(Handle, out x, out y, out z);

                // TODO: Should these values be converted into a more human-understandable range?
                Magnetometer = new Vector3(x, y, z);
            }

            BatteryLevel = PsMoveApi.psmove_get_battery(Handle);

            Temperature = PsMoveApi.psmove_get_temperature_in_celsius(Handle);
            PsMoveApi.psmove_set_rumble(Handle, (byte) (Rumble / 255f));
        }
    } // SharpMotionController
} // namespace
