using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class CommandResult : ICommandResult
    {
        public static CommandResult Empty => new CommandResult(0, "empty", null);

        public CommandResult(int errorCode, string responseMessage, object response)
        {
            ErrorCode = errorCode;
            ResponseMessage = responseMessage;
            Response = response;
        }

        public int ErrorCode { get; }
        public string ResponseMessage { get; }
        public object Response { get; }

        public bool IsSuccess => ErrorCode == 0;

    }
}
