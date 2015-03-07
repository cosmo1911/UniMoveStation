using System;

namespace UniMoveStation.NitoMessages
{
    [Serializable]
    public class PositionMessage
    {
        public Float3 Message { get; set; }

        public long StartTick { get; set; }

        public int TrackerIndex { get; set; }

        public int MoveIndex { get; set; }
    }
}
