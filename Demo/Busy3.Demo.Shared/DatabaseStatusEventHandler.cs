using Busy;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy3.Demo.Shared
{
    public class DatabaseStatusEventHandler : IMessageHandler<DatabaseStatus>
    {
        public void Handle(DatabaseStatus message)
        {
            Console.WriteLine(message);
        }

    }
}
