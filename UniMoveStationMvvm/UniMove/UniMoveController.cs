/**
 * PS Move API - An interface for the PS Move Motion Controller
 * Copyright (c) 2012 Benjamin Venditti <benjamin.venditti@gmail.com>
 * Copyright (c) 2012 Thomas Perl <m@thp.io>
 * 
 * UniMove API - A Unity plugin for the PlayStation Move motion controller
 * Copyright (C) 2012, 2013, Copenhagen Game Collective (http://www.cphgc.org)
 * 					         Patrick Jarnfelt
 * 					         Douglas Wilson (http://www.doougle.net)
 * 					         
 * UniMoveExtended
 * Copyright (c) 2013 Eric Itomura (http://eric.itomura.org/unimovex)
 * 
 * Additional Fixes
 * Copyright (c) 2014 Johannes Hoffmann
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *    1. Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *
 *    2. Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 **/

#define YISUP

using io.thp.psmove;
using System;
using System.Runtime.InteropServices;
using UnityEngine;


namespace UniMove
{
    public class UniMoveController : PSMove
    {
        #region private instance variables
        /// <summary>
        /// The handle for this controller. This pointer is what the psmove library uses for reading data via the hid library.
        /// </summary>
        private bool disconnected = false;

        private float timeElapsed = 0.0f;
        private float updateRate = 0.05f;	// The default update rate is 50 milliseconds

        private float MIN_UPDATE_RATE = 0.02f; // You probably don't want to update the controller more frequently than every 20 milliseconds

        private float trigger = 0f;
        private uint currentButtons = 0;
        private uint prevButtons = 0;

        private Color color;
        private PSMoveBatteryLevel battery = PSMoveBatteryLevel.TwentyPercent;
        private float temperature = 0f;

        private Vector3 rawAccel = Vector3.down;
        private Vector3 accel = Vector3.down;
        private Vector3 magnet = Vector3.zero;
        private Vector3 rawGyro = Vector3.zero;
        private Vector3 gyro = Vector3.zero;

        private Quaternion m_orientation = Quaternion.identity;
        private Quaternion m_orientationFix = Quaternion.identity;

        public Quaternion Orientation
        {
            get { return m_orientation; }
        }

        public Vector3 Up
        {
            get { return m_orientation * Vector3.up; }
        }

        public Vector3 Forward
        {
            get { return m_orientation * Vector3.forward; }
        }

        public Vector3 Right
        {
            get { return m_orientation * Vector3.right; }
        }

        public void ReinitializeLibrary()
        {
            reinit();
        }

        /// <summary>
        /// Event fired when the controller disconnects unexpectedly (i.e. on going out of range).
        /// </summary>
        public event EventHandler OnControllerDisconnected;

        #endregion
        /// <summary>
        /// Returns whether the connecting succeeded or not.
        /// 
        /// NOTE! This function does NOT pair the controller by Bluetooth.
        /// If the controller is not already paired, it can only be connected by USB.
        /// See README for more information.
        /// </summary>
        public PSMoveConnectStatus Init(int index)
        {
            connect_by_id(index);

            // Error check the result!
            if (move.Handle == IntPtr.Zero) return PSMoveConnectStatus.Error;

            // Make sure the connection is actually sending data. If not, this is probably a controller 
            // you need to remove manually from the OSX Bluetooth Control Panel, then re-connect.
            if (update_leds() == 0) return PSMoveConnectStatus.NoData;
            return PSMoveConnectStatus.OK;
        }

        public virtual void OnDestroy()
        {
            if (move.Handle != IntPtr.Zero)
            {
                Disconnect();
            }
        }

        /// <summary>
        /// static function that returns the number of *all* controller connections.
        /// This count will tally both USB and Bluetooth connections.
        /// Note that one physical controller, then, might register multiple connections.
        /// To discern between different connection types, see the ConnectionType property below.
        /// </summary>
        public static int GetNumConnected()
        {
            return count_connected();
        }

        /// <summary>
        /// The amount of time, in seconds, between update calls.
        /// The faster this rate, the more responsive the controllers will be.
        /// However, update too fast and your computer won't be able to keep up (see below).
        /// You almost certainly don't want to make this faster than 20 milliseconds (0.02f).
        /// 
        /// NOTE! We find that slower/older computers can have trouble keeping up with a fast update rate,
        /// especially the more controllers that are connected. See the README for more information.
        /// </summary>
        public float UpdateRate
        {
            get { return this.updateRate; }
            set { updateRate = Math.Max(value, MIN_UPDATE_RATE); }	// Clamp negative values up to 0
        }

        public void Update()
        {
            UpdateControllerRateLimited();
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

            if (timeElapsed < updateRate) return;
            else timeElapsed = 0.0f;
            UpdateController();
        }

        public void UpdateController()
        {

            uint buttons = 0;

            // NOTE! There is potentially data waiting in queue. 
            // We need to poll *all* of it by calling psmove_poll() until the queue is empty. Otherwise, data might begin to build up.
            while (poll() > 0)
            {
                // We are interested in every button press between the last update and this one:
                buttons = buttons | get_buttons();

                // The events are not really working from the PS Move Api. So we do our own with the prevButtons
                //psmove_get_button_events(handle, ref pressed, ref released);
            }
            currentButtons = buttons;

            // For acceleration, gyroscope, and magnetometer values, we look at only the last value in the queue.
            // We could in theory average all the acceleration (and other) values in the queue for a "smoothing" effect, but we've chosen not to.
            ProcessData();

            // Send a report to the controller to update the LEDs and rumble.
            if (update_leds() == 0)
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
        /// Disconnect the controller
        /// </summary>

        public void Disconnect()
        {
            if (move.Handle != IntPtr.Zero)
            {
                SetLED(0, 0, 0);
                SetRumble(0);
                disconnect();
                move = new HandleRef(this, IntPtr.Zero);
                disconnected = true;
                Console.WriteLine("move destroyed");
            }
        }

        /// <summary>
        /// Whether or not the controller has been disconnected
        /// </summary
        public bool Disconnected
        {
            get { return disconnected; }
        }

        /// <summary>
        /// Sets the amount of rumble
        /// </summary>
        /// <param name="rumble">the rumble amount (0-1)</param>
        public void SetRumble(float rumble)
        {
            if (disconnected) return;

            // Clamp to [0,255], rounded to nearest whole number
            rumble = Mathf.Clamp01(rumble) * 255.0f;

            set_rumble((int)(rumble + 0.5f));
        }

        /// <summary>
        /// Sets the LED color
        /// </summary>
        /// <param name="color">Unity's Color type</param>
        public void SetLED(Color color)
        {
            SetLED((byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255));
        }

        /// <summary>
        /// Sets the LED color
        /// </summary>
        /// <param name="r">Red value of the LED color (0-255)</param>
        /// <param name="g">Green value of the LED color (0-255)</param>
        /// <param name="b">Blue value of the LED color (0-255)</param>
        public void SetLED(int r, int g, int b)
        {
            if (disconnected) return;
            else
            {
                color = new Color(r / 255f, g / 255f, b / 255f);
                set_leds(r, g, b);
            }
        }

        /// <summary>
        /// Value of the analog trigger button (between 0 and 1)
        /// </summary
        public float Trigger
        {
            get { return trigger; }
        }

        /// <summary>
        /// The 3-axis acceleration values. 
        /// </summary>
        public Vector3 RawAcceleration
        {
            get { return rawAccel; }
        }

        /// <summary>
        /// The 3-axis acceleration values, roughly scaled between -3g to 3g (where 1g is Earth's gravity).
        /// </summary>
        public Vector3 Acceleration
        {
            get { return accel; }
        }

        /// <summary>
        /// The raw values of the 3-axis gyroscope. 
        /// </summary>
        public Vector3 RawGyroscope
        {
            get { return rawGyro; }
        }
        /// <summary>
        /// The raw values of the 3-axis gyroscope. 
        /// </summary>
        public Vector3 Gyro
        {
            get { return gyro; }
        }

        /// <summary>
        /// The raw values of the 3-axis magnetometer.
        /// To be honest, we don't fully understand what the magnetometer does.
        /// The C API on which this code is based warns that this isn't fully tested.
        /// </summary>
        public Vector3 Magnetometer
        {
            get { return magnet; }
        }

        /// <summary>
        /// The battery level
        /// </summary>
        public PSMoveBatteryLevel Battery
        {
            get { return battery; }
        }

        /// <summary>
        /// The temperature in Celcius
        /// </summary>
        public float Temperature
        {
            get { return temperature; }
        }

        public Color Color
        {
            get { return color; }
        }

        public string StatusInfo()
        {
            int r = (int) (Color.r * 255f);
            int g = (int) (Color.g * 255f);
            int b = (int) (Color.b * 255f);

            object[] objects = new object[]
            {
                r, g, b,
                Gyro,
                RawGyroscope,
                Acceleration,
                RawGyroscope,
                Magnetometer,
                IsOriented(),
                Temperature,
                Battery
            };
            
            string text = string.Format(
                "Color:\t\t{0}, {1}, {2}" + Environment.NewLine +
                "Gyroscope:\t{3}" + Environment.NewLine +
                "Gyroscope Raw:\t{4}" + Environment.NewLine +
                "Acceleration:\t{5}" + Environment.NewLine +
                "Acceleration Raw:\t{6}" + Environment.NewLine +
                "Magnetometer:\t{7}" + Environment.NewLine +
                "Orientation:\t{8}" + Environment.NewLine +
                "Temperature:\t{9}" + Environment.NewLine +
                "Battery:\t\t{10}"
                , objects);

            return text;
        }

        public bool IsOriented()
        {
            return has_orientation() == PSMoveBool.True;
        }

        public bool Orient(PSMoveBool enable)
        {
            bool oriented = false;
            enable_orientation(enable);
            if (enable == PSMoveBool.True)
            {
                reset_orientation();

                oriented = has_orientation() == PSMoveBool.True;
                if (oriented)
                {
                    float qw = 0.0f, qx = 0.0f, qy = 0.0f, qz = 0.0f;
                    get_orientation(out qw, out qx, out qy, out qz);

                    Quaternion rot = new Quaternion(qx, qy, qz, qw);
                    m_orientationFix = Quaternion.Inverse(rot);
                }
            }
            return oriented;
        }

        #region private methods

        /// <summary>
        /// Process all the raw data on the Playstation Move controller
        /// </summary>
        protected virtual void ProcessData()
        {
            trigger = (get_trigger()) / 255f;

            {
                int x = 0, y = 0, z = 0;

                get_accelerometer(out x, out y, out z);

                rawAccel.x = x;
                rawAccel.y = y;
                rawAccel.z = z;
            }

            {
                float ax = 0, ay = 0, az = 0;
                get_accelerometer_frame(PSMoveFrame.SecondHalf, out ax, out ay, out az);

                accel.x = ax;
                accel.y = ay;
                accel.z = az;
            }

            {
                int x = 0, y = 0, z = 0;
                get_gyroscope(out x, out y, out z);

                rawGyro.x = x;
                rawGyro.y = y;
                rawGyro.z = z;
            }

            {
                float gx = 0, gy = 0, gz = 0;
                get_gyroscope_frame(PSMoveFrame.SecondHalf, out gx, out gy, out gz);

                gyro.x = gx;
                gyro.y = gy;
                gyro.z = gz;
            }

            if (has_orientation() == PSMoveBool.True)
            {
                float qw = 0.0f, qx = 0.0f, qy = 0.0f, qz = 0.0f;
                get_orientation(out qw, out qx, out qy, out qz);

                Quaternion rot = new Quaternion(qx, qy, qz, qw);
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
                get_magnetometer(out x, out y, out z);

                // TODO: Should these values be converted into a more human-understandable range?
                magnet.x = x;
                magnet.y = y;
                magnet.z = z;
            }

            battery = get_battery();

            temperature = get_temperature();

        }
        #endregion

    } //UniMoveController
} //namespace