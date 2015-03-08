using System;

namespace UniMoveStation.NitoMessages
{
    /// <summary>
    /// A message containing a single string.
    /// </summary>
    [Serializable]
    public class StringMessage
    {
        /// <summary>
        /// The string.
        /// </summary>
        public string Message { get; set; }
    }
}
