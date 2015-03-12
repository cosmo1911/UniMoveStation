using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UniMoveStation.Business.PsMove
{
    public class PsMoveApi
    {
        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_init@4")]
        public static extern int psmove_init(int jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_set_remote_config@4")]
        public static extern void psmove_set_remote_config(PSMoveRemoteConfig jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_count_connected@0")]
        public static extern int psmove_count_connected();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_connect@0")]
        public static extern IntPtr psmove_connect();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_connect_by_id@4")]
        public static extern IntPtr psmove_connect_by_id(int jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_connection_type@4")]
        public static extern PSMoveConnectionType psmove_connection_type(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_is_remote@4")]
        public static extern int psmove_is_remote(IntPtr jarg1);

        [DllImport("libpsmoveapi", EntryPoint = "psmove_get_serial", CallingConvention = CallingConvention.Cdecl)]
        public static extern string psmove_get_serial(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_pair@4")]
        public static extern int psmove_pair(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_pair_custom@8")]
        public static extern int psmove_pair_custom(IntPtr jarg1, string jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_set_rate_limiting@8")]
        public static extern void psmove_set_rate_limiting(IntPtr jarg1, int jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_set_leds@16")]
        public static extern void psmove_set_leds(IntPtr jarg1, byte jarg2, byte jarg3, byte jarg4);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_set_rumble@8")]
        public static extern void psmove_set_rumble(IntPtr jarg1, byte jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_update_leds@4")]
        public static extern int psmove_update_leds(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_poll@4")]
        public static extern int psmove_poll(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_buttons@4")]
        public static extern uint psmove_get_buttons(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_button_events@12")]
        public static extern void psmove_get_button_events(IntPtr jarg1, out uint pressed, out uint released);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_battery@4")]
        public static extern PSMoveBatteryLevel psmove_get_battery(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_temperature@4")]
        public static extern int psmove_get_temperature(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_temperature_in_celsius@4")]
        public static extern float psmove_get_temperature_in_celsius(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_trigger@4")]
        public static extern byte psmove_get_trigger(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_accelerometer@16")]
        public static extern void psmove_get_accelerometer(IntPtr move, out int x, out int y, out int z);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_gyroscope@16")]
        public static extern void psmove_get_gyroscope(IntPtr move, out int x, out int y, out int z);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_magnetometer@16")]
        public static extern void psmove_get_magnetometer(IntPtr move, out int x, out int y, out int z);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_get_accelerometer_frame@20")]
        public static extern void psmove_get_accelerometer_frame(IntPtr move, PSMoveFrame frame, out float jarg3, out float jarg4, out float jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_get_gyroscope_frame@20")]
        public static extern void psmove_get_gyroscope_frame(IntPtr move, PSMoveFrame frame, out float gx, out float gy, out float gz);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_get_magnetometer_vector@16")]
        public static extern void psmove_get_magnetometer_vector(IntPtr move, out float x, out float y, out float z);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_has_calibration@4")]
        public static extern int psmove_has_calibration(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_dump_calibration@4")]
        public static extern void psmove_dump_calibration(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_enable_orientation@8")]
        public static extern void psmove_enable_orientation(IntPtr jarg1, PSMoveBool jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_has_orientation@4")]
        public static extern PSMoveBool psmove_has_orientation(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_orientation@20")]
        public static extern void psmove_get_orientation(IntPtr jarg1, out float w, out float x, out float y, out float z);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_reset_orientation@4")]
        public static extern void psmove_reset_orientation(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_reset_magnetometer_calibration@4")]
        public static extern void psmove_reset_magnetometer_calibration(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_save_magnetometer_calibration@4")]
        public static extern void psmove_save_magnetometer_calibration(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_magnetometer_calibration_range@4")]
        public static extern int psmove_get_magnetometer_calibration_range(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_disconnect@4")]
        public static extern void psmove_disconnect(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_reinit@0")]
        public static extern void psmove_reinit();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_util_get_ticks@0")]
        public static extern int psmove_util_get_ticks();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_util_get_data_dir@0")]
        public static extern string psmove_util_get_data_dir();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_util_get_file_path@4")]
        public static extern string psmove_util_get_file_path(string jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_util_get_env_int@4")]
        public static extern int psmove_util_get_env_int(string jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_util_get_env_string@4")]
        public static extern string psmove_util_get_env_string(string jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_connection_type_get@4")]
        public static extern PSMoveConnectionType PSMove_connection_type_get(IntPtr move);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_ax_get@4")]
        public static extern int PSMove_ax_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_ay_get@4")]
        public static extern int PSMove_ay_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_az_get@4")]
        public static extern int PSMove_az_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_gx_get@4")]
        public static extern int PSMove_gx_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_gy_get@4")]
        public static extern int PSMove_gy_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_gz_get@4")]
        public static extern int PSMove_gz_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_mx_get@4")]
        public static extern int PSMove_mx_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_my_get@4")]
        public static extern int PSMove_my_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMove_mz_get@4")]
        public static extern int PSMove_mz_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_new_PSMove__SWIG_0@0")]
        public static extern IntPtr new_PSMove__SWIG_0();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_new_PSMove__SWIG_1@4")]
        public static extern IntPtr new_PSMove__SWIG_1(int jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_accelerometer_frame@20")]
        public static extern void PSMove_get_accelerometer_frame(IntPtr jarg1, int jarg2, out float jarg3, out float jarg4, out float jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_gyroscope_frame@20")]
        public static extern void PSMove_get_gyroscope_frame(IntPtr jarg1, int jarg2, out float jarg3, out float jarg4, out float jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_magnetometer_vector@16")]
        public static extern void PSMove_get_magnetometer_vector(IntPtr jarg1, out float jarg2, out float jarg3, out float jarg4);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_enable_orientation@8")]
        public static extern void PSMove_enable_orientation(IntPtr move, PSMoveBool enabled);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_has_orientation@4")]
        public static extern PSMoveBool PSMove_has_orientation(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_has_calibration@4")]
        public static extern PSMoveBool PSMove_has_calibration(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_orientation@20")]
        public static extern void PSMove_get_orientation(IntPtr jarg1, out float jarg2, out float jarg3, out float jarg4, out float jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_reset_orientation@4")]
        public static extern void PSMove_reset_orientation(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_set_leds@16")]
        public static extern void PSMove_set_leds(IntPtr jarg1, int jarg2, int jarg3, int jarg4);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_set_rumble@8")]
        public static extern void PSMove_set_rumble(IntPtr jarg1, int jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_update_leds@4")]
        public static extern int PSMove_update_leds(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_set_rate_limiting@8")]
        public static extern void PSMove_set_rate_limiting(IntPtr jarg1, int jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_pair@4")]
        public static extern int PSMove_pair(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_pair_custom@8")]
        public static extern int PSMove_pair_custom(IntPtr jarg1, string jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_get_serial@8")]
        public static extern void get_serial(IntPtr jarg1, StringBuilder jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_get_moved_host@8")]
        public static extern void get_moved_host(IntPtr jarg1, StringBuilder jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_is_remote@4")]
        public static extern int PSMove_is_remote(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_poll@4")]
        public static extern int PSMove_poll(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_buttons@4")]
        public static extern uint PSMove_get_buttons(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_button_events@12")]
        public static extern void PSMove_get_button_events(IntPtr jarg1, out uint jarg2, out uint jarg3);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_battery@4")]
        public static extern PSMoveBatteryLevel PSMove_get_battery(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_temperature@4")]
        public static extern int PSMove_get_temperature(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_temperature_in_celsius@4")]
        public static extern float PSMove_get_temperature_in_celsius(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_get_trigger@4")]
        public static extern int PSMove_get_trigger(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_delete_PSMove@4")]
        public static extern void delete_PSMove(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_TRACKER_DEFAULT_WIDTH_get@0")]
        public static extern int PSMOVE_TRACKER_DEFAULT_WIDTH_get();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_TRACKER_DEFAULT_HEIGHT_get@0")]
        public static extern int PSMOVE_TRACKER_DEFAULT_HEIGHT_get();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_TRACKER_MAX_CONTROLLERS_get@0")]
        public static extern int PSMOVE_TRACKER_MAX_CONTROLLERS_get();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_TRACKER_CAMERA_ENV_get@0")]
        public static extern string PSMOVE_TRACKER_CAMERA_ENV_get();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_TRACKER_FILENAME_ENV_get@0")]
        public static extern string PSMOVE_TRACKER_FILENAME_ENV_get();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_TRACKER_ROI_SIZE_ENV_get@0")]
        public static extern string PSMOVE_TRACKER_ROI_SIZE_ENV_get();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_TRACKER_COLOR_ENV_get@0")]
        public static extern string PSMOVE_TRACKER_COLOR_ENV_get();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_TRACKER_WIDTH_ENV_get@0")]
        public static extern string PSMOVE_TRACKER_WIDTH_ENV_get();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_TRACKER_HEIGHT_ENV_get@0")]
        public static extern string PSMOVE_TRACKER_HEIGHT_ENV_get();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharpTrackerRGBImage_data_set@8")]
        public static extern void PSMoveTrackerRGBImage_data_set(IntPtr jarg1, IntPtr jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTrackerRGBImage_data_get@4")]
        public static extern IntPtr PSMoveTrackerRGBImage_data_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTrackerRGBImage_width_set@8")]
        public static extern void PSMoveTrackerRGBImage_width_set(IntPtr jarg1, int jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTrackerRGBImage_width_get@4")]
        public static extern int PSMoveTrackerRGBImage_width_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTrackerRGBImage_height_set@8")]
        public static extern void PSMoveTrackerRGBImage_height_set(IntPtr jarg1, int jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTrackerRGBImage_height_get@4")]
        public static extern int PSMoveTrackerRGBImage_height_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTrackerRGBImage_size_get@4")]
        public static extern int PSMoveTrackerRGBImage_size_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_new_PSMoveTrackerRGBImage@0")]
        public static extern IntPtr new_PSMoveTrackerRGBImage();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_delete_PSMoveTrackerRGBImage@4")]
        public static extern void delete_PSMoveTrackerRGBImage(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_new@0")]
        public static extern IntPtr psmove_tracker_new();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_new_with_camera@4")]
        public static extern IntPtr psmove_tracker_new_with_camera(int camera);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_set_auto_update_leds@12")]
        public static extern void psmove_tracker_set_auto_update_leds(IntPtr tracker, IntPtr move, int jarg3);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_get_auto_update_leds@8")]
        public static extern int psmove_tracker_get_auto_update_leds(IntPtr jarg1, IntPtr jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_set_dimming@8")]
        public static extern void psmove_tracker_set_dimming(IntPtr jarg1, float jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_get_dimming@4")]
        public static extern float psmove_tracker_get_dimming(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_set_exposure@8")]
        public static extern void psmove_tracker_set_exposure(IntPtr tracker, PSMoveTrackerExposure exposure);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_get_exposure@4")]
        public static extern int psmove_tracker_get_exposure(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_enable_deinterlace@8")]
        public static extern void psmove_tracker_enable_deinterlace(IntPtr jarg1, int jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_set_mirror@8")]
        public static extern void psmove_tracker_set_mirror(IntPtr jarg1, int jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_get_mirror@4")]
        public static extern int psmove_tracker_get_mirror(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_enable@8")]
        public static extern PSMoveTrackerStatus psmove_tracker_enable(IntPtr jarg1, IntPtr jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_enable_with_color@20")]
        public static extern PSMoveTrackerStatus psmove_tracker_enable_with_color(IntPtr jarg1, IntPtr jarg2, byte jarg3, byte jarg4, byte jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_disable@8")]
        public static extern void psmove_tracker_disable(IntPtr jarg1, IntPtr jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_get_color@20")]
        public static extern int psmove_tracker_get_color(IntPtr jarg1, IntPtr jarg2, HandleRef jarg3, HandleRef jarg4, HandleRef jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_get_camera_color@20")]
        public static extern int psmove_tracker_get_camera_color(IntPtr jarg1, IntPtr jarg2, HandleRef jarg3, HandleRef jarg4, HandleRef jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_set_camera_color@20")]
        public static extern int psmove_tracker_set_camera_color(IntPtr jarg1, IntPtr jarg2, byte jarg3, byte jarg4, byte jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_get_status@8")]
        public static extern PSMoveTrackerStatus psmove_tracker_get_status(IntPtr jarg1, IntPtr jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_update_image@4")]
        public static extern void psmove_tracker_update_image(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_update@8")]
        public static extern int psmove_tracker_update(IntPtr jarg1, IntPtr jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_annotate@4")]
        public static extern void psmove_tracker_annotate(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_get_frame@4")]
        public static extern IntPtr psmove_tracker_get_frame(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_get_image@4")]
        public static extern IntPtr psmove_tracker_get_image(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_get_position@20")]
        public static extern int psmove_tracker_get_position(IntPtr jarg1, IntPtr jarg2, out float jarg3, out float jarg4, out float jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_get_size@12")]
        public static extern void psmove_tracker_get_size(IntPtr jarg1, HandleRef jarg2, HandleRef jarg3);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_distance_from_radius@8")]
        public static extern float psmove_tracker_distance_from_radius(IntPtr jarg1, float jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_set_distance_parameters@20")]
        public static extern void psmove_tracker_set_distance_parameters(IntPtr jarg1, float jarg2, float jarg3, float jarg4, float jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_tracker_free@4")]
        public static extern void psmove_tracker_free(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "PSEYE_FOV_BLUE_DOT_get@0")]
        public static extern int PSEYE_FOV_BLUE_DOT_get();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "PSEYE_FOV_RED_DOT_get@0")]
        public static extern int PSEYE_FOV_RED_DOT_get();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveMatrix4x4_m_set@8")]
        public static extern void PSMoveMatrix4x4_m_set(HandleRef jarg1, HandleRef jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveMatrix4x4_m_get@4")]
        public static extern IntPtr PSMoveMatrix4x4_m_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveMatrix4x4_at@8")]
        public static extern float PSMoveMatrix4x4_at(IntPtr jarg1, int jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_new_PSMoveMatrix4x4@0")]
        public static extern IntPtr new_PSMoveMatrix4x4();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_delete_PSMoveMatrix4x4@4")]
        public static extern void delete_PSMoveMatrix4x4(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_fusion_new@12")]
        public static extern IntPtr psmove_fusion_new(IntPtr jarg1, float jarg2, float jarg3);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_fusion_get_projection_matrix@4")]
        public static extern IntPtr psmove_fusion_get_projection_matrix(IntPtr fusion);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_fusion_get_modelview_matrix@8")]
        public static extern IntPtr psmove_fusion_get_modelview_matrix(IntPtr fusion, IntPtr move);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_fusion_get_position@20")]
        public static extern void psmove_fusion_get_position(IntPtr fusion, IntPtr move, out float jarg3, out float jarg4, out float jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_fusion_free@4")]
        public static extern void psmove_fusion_free(IntPtr fusion);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_dimming_set@8")]
        public static extern void PSMoveTracker_dimming_set(IntPtr jarg1, float jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_dimming_get@4")]
        public static extern float PSMoveTracker_dimming_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_exposure_set@8")]
        public static extern void PSMoveTracker_exposure_set(IntPtr jarg1, int jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_exposure_get@4")]
        public static extern int PSMoveTracker_exposure_get(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_new_PSMoveTracker__SWIG_0@0")]
        public static extern IntPtr new_PSMoveTracker__SWIG_0();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_new_PSMoveTracker__SWIG_1@4")]
        public static extern IntPtr new_PSMoveTracker__SWIG_1(int jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_enable@8")]
        public static extern int PSMoveTracker_enable(IntPtr jarg1, IntPtr jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_enable_with_color@20")]
        public static extern PSMoveTrackerStatus PSMoveTracker_enable_with_color(IntPtr tracker, IntPtr move, int jarg3, int jarg4, int jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_annotate@4")]
        public static extern void PSMoveTracker_annotate(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_disable@8")]
        public static extern void PSMoveTracker_disable(IntPtr jarg1, IntPtr jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_set_auto_update_leds@12")]
        public static extern void PSMoveTracker_set_auto_update_leds(IntPtr tracker, IntPtr move, PSMoveBool autoUpdateLeds);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_get_auto_update_leds@8")]
        public static extern PSMoveBool PSMoveTracker_get_auto_update_leds(IntPtr jarg1, IntPtr jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_get_color@20")]
        public static extern void PSMoveTracker_get_color(IntPtr jarg1, IntPtr jarg2, out byte jarg3, out byte jarg4, out byte jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_get_camera_color@20")]
        public static extern void PSMoveTracker_get_camera_color(IntPtr jarg1, IntPtr jarg2, out byte jarg3, out byte jarg4, out byte jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_set_camera_color@20")]
        public static extern int PSMoveTracker_set_camera_color(IntPtr jarg1, IntPtr jarg2, byte jarg3, byte jarg4, byte jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_enable_deinterlace@8")]
        public static extern void PSMoveTracker_enable_deinterlace(IntPtr jarg1, int jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_set_mirror@8")]
        public static extern void PSMoveTracker_set_mirror(IntPtr jarg1, int jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_get_mirror@4")]
        public static extern int PSMoveTracker_get_mirror(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_get_status@8")]
        public static extern int PSMoveTracker_get_status(IntPtr jarg1, IntPtr jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_update_image@4")]
        public static extern void PSMoveTracker_update_image(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_update__SWIG_0@4")]
        public static extern int PSMoveTracker_update__SWIG_0(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_update__SWIG_1@8")]
        public static extern int PSMoveTracker_update__SWIG_1(IntPtr jarg1, IntPtr jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_get_image@4")]
        public static extern IntPtr PSMoveTracker_get_image(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_get_position@20")]
        public static extern void PSMoveTracker_get_position(IntPtr jarg1, IntPtr jarg2, out float jarg3, out float jarg4, out float jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_get_size@12")]
        public static extern void PSMoveTracker_get_size(IntPtr jarg1, out int jarg2, out int jarg3);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_distance_from_radius@8")]
        public static extern float PSMoveTracker_distance_from_radius(IntPtr jarg1, float jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveTracker_set_distance_parameters@20")]
        public static extern void PSMoveTracker_set_distance_parameters(IntPtr jarg1, float jarg2, float jarg3, float jarg4, float jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_delete_PSMoveTracker@4")]
        public static extern void delete_PSMoveTracker(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_new_PSMoveFusion@12")]
        public static extern IntPtr new_PSMoveFusion(IntPtr jarg1, float jarg2, float jarg3);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_delete_PSMoveFusion@4")]
        public static extern void delete_PSMoveFusion(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveFusion_get_position@20")]
        public static extern void PSMoveFusion_get_position(IntPtr jarg1, IntPtr jarg2, out float jarg3, out float jarg4, out float jarg5);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveFusion_get_projection_matrix@4")]
        public static extern IntPtr PSMoveFusion_get_projection_matrix(IntPtr jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_PSMoveFusion_get_modelview_matrix@8")]
        public static extern IntPtr PSMoveFusion_get_modelview_matrix(IntPtr jarg1, IntPtr jarg2);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_init@4")]
        public static extern int init(int jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_set_remote_config@4")]
        public static extern void set_remote_config(int jarg1);

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_psmove_count_connected@0")]
        public static extern int count_connected();

        [DllImport("libpsmoveapi_csharp", EntryPoint = "CSharp_reinit@0")]
        public static extern void reinit();
    }
}
