using System;

namespace UniMoveStation.NitoMessages
{
    [Serializable]
    public class PositionRequest
    {
        public PositionType Type { get; set; }

        public long StartTick { get; set; }

        public int CameraIndex { get; set; }

        public int ControllerIndex { get; set; }

        public string CameraName { get; set; }

        public string ControllerName { get; set; }

        /// <summary>
        /// requests bundled position
        /// </summary>
        /// <param name="controllerIndex"></param>
        public PositionRequest(int controllerIndex)
        {
            Type = PositionType.Bundled;
            ControllerIndex = controllerIndex;
        }

        /// <summary>
        /// requests any position type by indices
        /// </summary>
        /// <param name="cameraIndex"></param>
        /// <param name="controllerIndex"></param>
        /// <param name="type"></param>
        public PositionRequest(int cameraIndex, int controllerIndex, PositionType type)
        {
            Type = type;
            CameraIndex = cameraIndex;
            ControllerIndex = controllerIndex;
        }

        /// <summary>
        /// requests any position type by name
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="controllerName"></param>
        /// <param name="type"></param>
        public PositionRequest(string cameraName, string controllerName, PositionType type)
        {
            Type = type;
            CameraName = cameraName;
            ControllerName = controllerName;
        }

        public PositionRequest() { }
    }
}
