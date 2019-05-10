namespace Busy
{
    public interface ICommandResult
    {
        int ErrorCode { get; }
        bool IsSuccess { get; }
        object Response { get; }
        string ResponseMessage { get; }
    }
}