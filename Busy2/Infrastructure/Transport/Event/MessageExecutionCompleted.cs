﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Busy.Infrastructure
{
    [Transient, Infrastructure]
    public class MessageExecutionCompleted : IMessage
    {
        public static readonly MessageTypeId TypeId = new MessageTypeId(typeof(MessageExecutionCompleted));

        public MessageId SourceCommandId { get; private set; }

        public int ErrorCode { get; private set; }

        public MessageTypeId? PayloadTypeId { get; private set; }

        public byte[] Payload { get; private set; }

        public string ResponseMessage { get; private set; } = string.Empty;

        public MessageExecutionCompleted(MessageId sourceCommandId, int errorCode, string responseMessage)
        {
            SourceCommandId = sourceCommandId;
            ErrorCode = errorCode;
            ResponseMessage = responseMessage ?? string.Empty;
        }

        public MessageExecutionCompleted(MessageId sourceCommandId, MessageTypeId payloadTypeId, byte[] payload)
        {
            SourceCommandId = sourceCommandId;
            ErrorCode = 0;
            PayloadTypeId = payloadTypeId;
            Payload = payload;
        }

        public override string ToString()
        {
            return ErrorCode == 0
                ? $"CommandId: {SourceCommandId}"
                : $"CommandId: {SourceCommandId}, ErrorCode: {ErrorCode} ({ResponseMessage})";
        }

        public static MessageExecutionCompleted Create(MessageContext messageContext, DispatchResult dispatchResult, IMessageSerializer serializer)
        {
            if (dispatchResult.Errors.Any())
                return Failure(messageContext.MessageId, dispatchResult.Errors);

            if (messageContext.ReplyResponse != null)
                return Success(messageContext.MessageId, messageContext.ReplyResponse, serializer);

            return new MessageExecutionCompleted(messageContext.MessageId, messageContext.ReplyCode, messageContext.ReplyMessage);
        }

        public static MessageExecutionCompleted Success(MessageId sourceCommandId, IMessage payload, IMessageSerializer serializer)
        {
            var payloadStream = serializer.Serialize(payload);
            var payloadBytes = ToBytes(payloadStream);

            return new MessageExecutionCompleted(sourceCommandId, payload.TypeId(), payloadBytes);
        }

        public static MessageExecutionCompleted Failure(MessageId sourceCommandId, IEnumerable<Exception> exceptions)
        {
            var errorStatus = CommandResult.GetErrorStatus(exceptions);

            return new MessageExecutionCompleted(sourceCommandId, errorStatus.Code, errorStatus.Message);
        }

        private static byte[] ToBytes(Stream payloadStream)
        {
            var memoryStream = payloadStream as MemoryStream;
            if (memoryStream == null)
            {
                memoryStream = new MemoryStream();
                payloadStream.CopyTo(memoryStream);
            }
            return memoryStream.ToArray();
        }
    }
}
