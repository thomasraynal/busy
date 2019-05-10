namespace Busy
{
    public interface IMessageContext
    {
        string InitiatorUserName { get; }
        //MessageId MessageId { get; }
        //OriginatorInfo Originator { get; }
        int ReplyCode { get; }
        string ReplyMessage { get; }
        //IMessage ReplyResponse { get; }
        string SenderEndPoint { get; }
        //PeerId SenderId { get; }

        //Peer GetSender();
    }
}