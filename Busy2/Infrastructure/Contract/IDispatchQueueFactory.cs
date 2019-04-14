using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public interface IDispatchQueueFactory
    {
        DispatchQueue Create(string queueName);
    }
}
