﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Busy
{
    public class CommandResult
    {
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

        public string GetErrorMessageFromEnum<T>(params object[] formatValues) where T : struct, IConvertible, IFormattable, IComparable
        {
            if (IsSuccess)
                return string.Empty;

            var value = (T)Enum.Parse(typeof(T), ErrorCode.ToString());

            return string.Format(((Enum)(object)value).GetAttributeDescription(), formatValues);
        }

        internal static ErrorStatus GetErrorStatus(IEnumerable<Exception> exceptions)
        {
            var domainException = exceptions.FirstOrDefault() as DomainException;
            if (domainException == null)
                return ErrorStatus.UnknownError;

            return new ErrorStatus(domainException.ErrorCode != 0 ? domainException.ErrorCode : ErrorStatus.UnknownError.Code, domainException.Message);
        }
    }
}
