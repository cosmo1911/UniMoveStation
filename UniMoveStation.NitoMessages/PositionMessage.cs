using System;

namespace UniMoveStation.NitoMessages
{
    [Serializable]
    public class PositionMessage
    {
        public PositionType Type { get; set; }

        public Float3 Position { get; set; }

        public long StartTick { get; set; }

        public int CameraIndex { get; set; }

        public int ControllerIndex { get; set; }
    }
}
