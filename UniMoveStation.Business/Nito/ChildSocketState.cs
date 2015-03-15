namespace UniMoveStation.Business.Nito
{
    /// <summary>
    /// The state of a child socket connection.
    /// </summary>
    public enum ChildSocketState
    {
        /// <summary>
        /// The child socket has an established connection.
        /// </summary>
        Connected,

        /// <summary>
        /// The child socket is disconnecting.
        /// </summary>
        Disconnecting
    }
}
