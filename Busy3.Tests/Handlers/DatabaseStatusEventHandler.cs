using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{

    public class DatabaseStatusEventHandler : IMessageHandler<DatabaseStatus>
    {
        public void Handle(DatabaseStatus message)
        {
            TestsBusContext.Increment();
        }

    }

}
