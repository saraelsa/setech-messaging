namespace SETech.Messaging.MessageBus;

/// <summary>
///     The <see cref="MessageNotFoundException"/> indicates that a message with a given sequence number was not found.
/// </summary>
public class MessageNotFoundException : Exception
{
    /// <param cref="sequenceNumber">The sequence number of the message that was not found.</param>
    public MessageNotFoundException(long sequenceNumber)
        : base(string.Format("The message with sequence number {0} was not found.", sequenceNumber))
    {
        SequenceNumber = sequenceNumber;
    }

    /// <summary>The sequence number of the message that was not found.</summary>
    public long? SequenceNumber { get; init; }
}
