using System;

namespace UniMoveStation.Business.PsMove
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
