using UniMoveStation.Business.Model;

namespace UniMoveStation.Business.Service.Interfaces
{
    public interface ISettingsService
    {
        /// <summary>
        /// reloads all saved settings, i.e. motion controller configs, camera configs, 
        /// camera calibration configs, general settings
        /// </summary>
        /// <returns>new settings loaded from the data storage</returns>
        SettingsModel ReloadSettings();

        /// <summary>
        /// persistently saves the data of a camera
        /// </summary>
        /// <param name="camera">camera data to be saved</param>
        void SaveCamera(CameraModel camera);

        /// <summary>
        /// persistently saves the calibration data of a camera
        /// </summary>
        /// <param name="calibration">calibration data belonging to a camera</param>
        void SaveCalibration(CameraCalibrationModel calibration);

        /// <summary>
        /// reloads the persistenly saved camera calibration data
        /// </summary>
        /// <param name="guid">identifier of the camera for which the data is loaded</param>
        /// <returns>new calibration data loaded from the data storage</returns>
        CameraCalibrationModel LoadCalibration(string guid);

        /// <summary>
        /// persistenly saves the general settings
        /// </summary>
        /// <param name="settings">general settings to be saved</param>
        void SaveSettings(SettingsModel settings);
    }
}
