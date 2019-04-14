using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{

    public interface IBusConfiguration
    {
        string[] DirectoryServiceEndPoints { get; }

        TimeSpan RegistrationTimeout { get; }

        TimeSpan StartReplayTimeout { get; }

        bool IsPersistent { get; }

        bool IsErrorPublicationEnabled { get; }

        int MessagesBatchSize { get; }
    }
}
