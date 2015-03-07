/**
 * PS Move API - An interface for the PS Move Motion Controller
 * Copyright (c) 2012 Benjamin Venditti <benjamin.venditti@gmail.com>
 * Copyright (c) 2012 Thomas Perl <m@thp.io>
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

/**
 * UniMove API - A Unity plugin for the PlayStation Move motion controller
 * Copyright (C) 2012, 2013, Copenhagen Game Collective (http://www.cphgc.org)
 * 					         Patrick Jarnfelt
 * 					         Douglas Wilson (http://www.doougle.net				         
 */

/**		         
 * UniMoveExtended
 * Copyright (c) 2013 Eric Itomura (http://eric.itomura.org/unimovex)
 */

/*
 * UniMoveStation
 * Copyright (c) 2014 Johannes Hoffmann
 */

using System;

namespace UniMoveStation.Common.SharpMove
{
    /// <summary>
    /// The Move controller can be connected by USB and/or Bluetooth.
    /// </summary>
    public enum PSMoveConnectionType
    {
        Bluetooth,
        USB,
        Unknown
    };

    public enum PSMoveRemoteConfig
    {
        LocalAndRemote = 0,
        OnlyLocal = 1,
        OnlyRemote = 2
    }

    public enum PSMoveBool
    {
        False = 0,
        True = 1
    }

    // Not entirely sure why some of these buttons (R3/L3) are exposed...
    public enum PSMoveButton
    {
        L2 = 1 << 0x00,
        R2 = 1 << 0x01,
        L1 = 1 << 0x02,
        R1 = 1 << 0x03,
        Triangle = 1 << 0x04,
        Circle = 1 << 0x05,
        Cross = 1 << 0x06,
        Square = 1 << 0x07,
        Select = 1 << 0x08,
        L3 = 1 << 0x09,
        R3 = 1 << 0x0A,
        Start = 1 << 0x0B,
        Up = 1 << 0x0C,
        Right = 1 << 0x0D,
        Down = 1 << 0x0E,
        Left = 1 << 0x0F,
        PS = 1 << 0x10,
        Move = 1 << 0x13,
        Trigger = 1 << 0x14	/* We can use this value with IsButtonDown() (or the events) to get 
							 * a binary yes/no answer about if the trigger button is down at all.
							 * For the full integer/analog value of the trigger, see the corresponding property below.
							 */
    };

    // Used by psmove_get_battery().
    public enum PSMoveBatteryLevel
    {
        Min = 0x00, /*!< Battery is almost empty (< 20%) */
        TwentyPercent = 0x01, /*!< Battery has at least 20% remaining */
        FourtyPercent = 0x02, /*!< Battery has at least 40% remaining */
        SixtyPercent = 0x03, /*!< Battery has at least 60% remaining */
        EightyPercent = 0x04, /*!< Battery has at least 80% remaining */
        Max = 0x05, /*!< Battery is fully charged (not on charger) */
        Charging = 0xEE, /*!< Battery is currently being charged */
        ChargingDone = 0xEF /*!< Battery is fully charged (on charger) */
    };

    public enum PSMoveFrame
    {
        FirstHalf = 0, /*!< The older frame */
        SecondHalf /*!< The most recent frame */
    };

    public enum PSMoveLEDAutoOption
    {
        On,
        Off
    };

    public enum PSMoveConnectStatus
    {
        OK,
        Error,
        NoData,
        Unknown
    }

    public class UniMoveButtonEventArgs : EventArgs
    {
        private readonly PSMoveButton button;

        public UniMoveButtonEventArgs(PSMoveButton button)
        {
            this.button = button;
        }
    }

    public enum PSMoveTrackerStatus
    {
        NotCalibrated, /*!< Controller not registered with tracker */
        CalibrationError, /*!< Calibration failed (check lighting, visibility) */
        Calibrated, /*!< Color calibration successful, not currently tracking */
        Tracking /*!< Calibrated and successfully tracked in the camera */
    };

    public enum PSMoveTrackerExposure
    {
        Low, /*!< Very low exposure: Good tracking, no environment visible */
        Medium, /*!< Middle ground: Good tracking, environment visibile */
        High, /*!< High exposure: Fair tracking, but good environment */
        Invalid /*!< Invalid exposure value (for returning failures) */
    };
} //namespace
