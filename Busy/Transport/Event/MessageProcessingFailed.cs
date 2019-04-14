using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class MessageProcessingFailed : IEvent
    {
        public readonly TransportMessage FailingMessage;

        public readonly string FailingMessageJson;

        public readonly string ExceptionMessage;

        public readonly DateTime ExceptionUtcTime;

        public readonly string[] FailingHandlerNames;

        public MessageProcessingFailed(TransportMessage failingMessage, string failingMessageJson, string exceptionMessage, DateTime exceptionUtcTime, string[] failingHandlerNames)
        {
            FailingMessage = failingMessage;
            FailingMessageJson = failingMessageJson;
            ExceptionMessage = exceptionMessage;
            ExceptionUtcTime = exceptionUtcTime;
            FailingHandlerNames = failingHandlerNames ?? Array.Empty<string>();
        }
    }
}
