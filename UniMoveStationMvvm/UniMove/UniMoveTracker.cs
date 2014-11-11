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

using io.thp.psmove;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UniMove
{
    public class UniMoveTracker : PSMoveTracker
    {
        private float timeElapsed = 0.0f;
        private float updateRate = 0.05f;	// The default update rate is 50 milliseconds
        public int id;

        public List<TrackedController> controllers = new List<TrackedController>();

        public UniMoveTracker(int id)
        {
            this.id = id;
        }

        //public IntPtr fusion = IntPtr.Zero;

        [Serializable]
        public class TrackedController
        {
            public TrackedController(UniMoveController move)
            {
                this.move = move;
                trackerStatus = PSMoveTrackerStatus.NotCalibrated;
            }

            public UniMoveController move;

            public Color color;

            public PSMoveTrackerStatus trackerStatus;

            public Vector3 m_position = Vector3.zero;
            public Vector3[] m_positionHistory = new Vector3[5];
            public Vector3 m_positionFix = Vector3.zero;
            public Vector3 m_positionScalePos = Vector3.one * 0.01f;
            public Vector3 m_positionScaleNeg = -Vector3.one * 0.01f;

            public Vector3 Position
            {
                get { return m_position; }
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            timeElapsed += Time.deltaTime;

            // Here we manually enforce updates only every updateRate amount of time
            // The reason we don't just do this in FixedUpdate is so the main program's FixedUpdate rate 
            // can be set independently of the controllers' update rate.

            if (timeElapsed < updateRate) return;
            else timeElapsed = 0.0f;

            UpdateTracker();
        }

        public bool UpdateTracker()
        {
            if (tracker.Handle != IntPtr.Zero)
            {
                update_image();
                int result = update();
                ProcessData();
                return result != 0;
            }
            return false;
        }

        public bool StartTracker(int camera)
        {
            if (tracker.Handle == IntPtr.Zero)
            {
                new_with_camera(camera);
                dimming = 1f;
                //			fusion = psmove_fusion_new(tracker,0.001f,1.0f);
            }
            return tracker.Handle != IntPtr.Zero;
        }

        /// <summary>
        /// Initialize and calibrate the tracker.
        /// </summary>
        /// <returns>
        /// Calibration and tracking status;
        /// </returns>
        public PSMoveTrackerStatus EnableTracking(UniMoveController controller, Color color)
        {
            if (tracker.Handle == IntPtr.Zero)
            {
                Console.WriteLine("start tracker " + id + ": " + StartTracker(id) + "\n");
            }

            TrackedController tc = new TrackedController(controller);
            tc.color = color;

            set_auto_update_leds(controller, PSMoveBool.False);

            int attempts = 0;
            while (true && attempts < 3)
            {
                Console.WriteLine("Calibrating controller " + controllers.Count + " on tracker " + id + "...\n");
                byte r = (byte)((color.r * 255) + 0.5f);
                byte g = (byte)((color.g * 255) + 0.5f);
                byte b = (byte)((color.b * 255) + 0.5f);
                tc.trackerStatus = enable_with_color(controller, r, g, b);

                if (tc.trackerStatus == PSMoveTrackerStatus.Calibrated)
                {
                    PSMoveBool auto_update_leds = get_auto_update_leds(controller);

                    controllers.Add(tc);

                    Console.WriteLine("OK, auto_update_leds is " + auto_update_leds + "\n");
                    break;
                }
                else
                {
                    attempts++;
                    Console.WriteLine("ERROR - retrying\n");
                }
            }

            if (tc.trackerStatus == PSMoveTrackerStatus.Tracking || tc.trackerStatus == PSMoveTrackerStatus.Calibrated)
            {
                update_image();
                update(controller);
                tc.trackerStatus = get_status(controller);
            }
            tc.m_positionFix = Vector3.zero;
            tc.m_positionScalePos = Vector3.one * 0.1f;
            tc.m_positionScaleNeg = -Vector3.one * 0.1f;
            for (int i = 0; i < tc.m_positionHistory.Length; ++i)
            {
                tc.m_positionHistory[i] = Vector3.zero;
            }
            return tc.trackerStatus;
        }

        public void DisableTracking()
        {
            if (tracker.Handle != IntPtr.Zero)
            {
                foreach (TrackedController tc in controllers)
                {
                    if (!tc.move.Disconnected)
                    {
                        disable(tc.move);
                    }
                }
                DestroyTracker();
            }
        }

        public void DestroyTracker()
        {
            if (tracker.Handle != IntPtr.Zero)
            {
                Dispose();
                Console.WriteLine("tracker destroyed");
            }
            //if (fusion != IntPtr.Zero)
            //{
            //    psmove_fusion_free(fusion);
            //    fusion = IntPtr.Zero;
            //    Console.WriteLine("fusion destroyed");
            //}
        }

        public void OnApplicationQuit()
        {
            DisableTracking();
        }

        public void OnDestroy()
        {
            DisableTracking();
        }

        //	/// <summary>
        //    /// Sets the LED color
        //    /// </summary>
        //    /// <param name="r">Red value of the LED color (0-255)</param>
        //    /// <param name="g">Green value of the LED color (0-255)</param>
        //    /// <param name="b">Blue value of the LED color (0-255)</param>
        //    public void SetLED(byte r, byte g, byte b)
        //    {
        //		if(trackerStatus == PSMoveTracker_Status.Tracker_TRACKING || trackerStatus == PSMoveTracker_Status.Tracker_CALIBRATED )
        //		{
        //			psmove_tracker_get_color(tracker, handle, ref r, ref g, ref b);
        //			Debug.Log(r + " " + g + " " + b);
        //			psmove_set_leds(handle, (char)r, (char)g, (char)b);
        //		}
        //	}
        //	
        //	public PSMoveBool RenormalizeTracker(UniMoveController controller)
        //	{
        //		if( tracker != IntPtr.Zero )
        //		{
        //			// only orient when get tracking going
        //			if( trackerStatus != PSMoveTracker_Status.Tracker_TRACKING )
        //			{
        //				float rx = 0.0f, ry = 0.0f, rrad = 0.0f;
        //				psmove_tracker_get_position(tracker, handle, ref rx, ref ry, ref rrad );
        //				
        //				float rz = psmove_tracker_distance_from_radius(tracker, rrad);
        //	
        //				m_positionFix = new Vector3(-rx,-ry,-rz);
        //				m_positionScaleNeg = -Vector3.one*0.01f;
        //				m_positionScalePos = Vector3.one*0.01f;
        //				
        //				for( int i = 0; i < m_positionHistory.Length ; ++i )
        //				{
        //					m_positionHistory[i] = Vector3.zero;
        //				}
        //				
        //				psmove_set_leds(controller.handle,(char)0,(char)255,(char)0);
        //				psmove_update_leds(controller.handle);
        //				return true;
        //			}
        //		}
        //		psmove_set_leds(handle,(char)255,(char)0,(char)0);
        //		psmove_update_leds(handle);
        //		return false;
        //	}

        public void ProcessData()
        {
            if (tracker.Handle != IntPtr.Zero)
            {
                foreach (TrackedController tc in controllers)
                {
                    tc.trackerStatus = get_status(tc.move);
                    if (tc.trackerStatus == PSMoveTrackerStatus.Tracking)
                    {
                        float rx = 0.0f, ry = 0.0f, rrad = 0.0f;
                        get_position(tc.move, out rx, out ry, out rrad);

                        //Console.WriteLine(rx + " " + ry + " " + rrad);

                        float rz = distance_from_radius(rrad);
                        Vector3 vec = new Vector3(rx, ry, rz) + tc.m_positionFix;
                        //#if YISUP
                        //vec.x = -vec.x;
                        //vec.y = -vec.y;
                        //vec.z = -vec.z;
                        //#endif
                        //tc.m_positionScalePos = Vector3.Max(vec, tc.m_positionScalePos);
                        //tc.m_positionScaleNeg = Vector3.Min(vec, tc.m_positionScaleNeg);

                        //Vector3 extents = tc.m_positionScalePos - tc.m_positionScaleNeg;

                        //vec = vec - tc.m_positionScaleNeg;
                        //vec.x = vec.x/extents.x;
                        //vec.y = vec.y/extents.y;
                        //vec.z = vec.z/extents.z;
                        //vec = vec*2.0f - Vector3.one;

                        for (int i = tc.m_positionHistory.Length - 1; i > 0; --i)
                        {
                            tc.m_positionHistory[i] = tc.m_positionHistory[i - 1];
                        }
                        tc.m_positionHistory[0] = vec;

                        //vec = m_positionHistory[0]*0.3f + m_positionHistory[1]*0.5f + m_positionHistory[2]*0.1f + m_positionHistory[3]*0.05f + m_positionHistory[4]*0.05f;

                        tc.m_position = vec;
                    }
                    else
                    {
                        tc.m_position = Vector3.zero;
                    }
                }
            }
        }
    }//UniMoveTracker
} //namespace