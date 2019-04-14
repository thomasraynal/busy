using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class CustomProcessingFailed : IEvent
    {
        public string SourceTypeFullName { get; set; }

        public string ExceptionMessage { get; set; }

        public DateTime ExceptionUtcTime { get; set; }

        public PeerId? FailingPeerIdOverride { get; set; }

        public string DetailsJson { get; set; }

        public CustomProcessingFailed(string sourceTypeFullName, string exceptionMessage)
            : this(sourceTypeFullName, exceptionMessage, SystemDateTime.UtcNow)
        {
        }

        public CustomProcessingFailed(string sourceTypeFullName, string exceptionMessage, DateTime exceptionUtcTime)
        {
            SourceTypeFullName = sourceTypeFullName;
            ExceptionMessage = exceptionMessage;
            ExceptionUtcTime = exceptionUtcTime;
        }

        public CustomProcessingFailed WithDetails(object details)
        {
            DetailsJson = details != null ? JsonConvert.SerializeObject(details) : null;
            return this;
        }
    }
}
