namespace SETech.Messaging.MessageBus.AzcoBus.MessageProcessing;

/// <summary>
///     The <see cref="MessageNotFoundException"/> indicates that a message with a given sequence number was not found.
/// </summary>
public class MessageNotFoundException : Exception
{
    /// <param cref="sequenceNumber">The sequence number of the message that was not found.</param>
    public MessageNotFoundException(long sequenceNumber)
    {
        SequenceNumber = sequenceNumber;
    }

    /// <summary>The sequence number of the message that was not found.</summary>
    public long SequenceNumber { get; set; }

    /// <inheritdoc />
    public override string Message => $"The message with sequence number {SequenceNumber} was not found.";
}
