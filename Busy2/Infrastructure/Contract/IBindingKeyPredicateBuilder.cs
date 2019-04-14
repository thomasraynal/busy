using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public interface IBindingKeyPredicateBuilder
    {
        Func<IMessage, bool> GetPredicate(Type messageType, BindingKey bindingKey);
    }
}
