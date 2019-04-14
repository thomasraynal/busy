using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public interface ISubscription
    {
        MessageTypeId MessageTypeId { get; }
        bool IsMatchingAllMessages { get; }
        bool Matches(MessageBinding messageBinding);
    }
}
