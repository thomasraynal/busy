using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busy.Infrastructure
{
    public interface IAsyncMessageHandler { }
    public interface IAsyncMessageHandler<T> : IAsyncMessageHandler where T : class
    {
        Task Handle(T message);
    }
}
