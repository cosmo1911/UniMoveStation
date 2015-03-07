using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniMoveStation.NitoMessages
{
    /// <summary>
    /// A message with more information.
    /// </summary>
    [Serializable]
    public class ComplexMessage
    {
        /// <summary>
        /// The user-defined string.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The time this message was created.
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// The unique identifier for this message.
        /// </summary>
        public Guid UniqueID { get; set; }
    }
}
