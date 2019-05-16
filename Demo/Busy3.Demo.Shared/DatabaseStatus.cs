using Busy;
using System;

namespace Busy3.Demo.Shared
{
    [Routable]
    [Asynchronous]
    public class DatabaseStatus : IEvent
    {
        public static Random Rand = new Random();

        public static DatabaseStatus Create(string peer)
        {
            return new DatabaseStatus()
            {
                DatacenterName = peer,
                Status = Rand.Next(0, 2) == 0 ? "Ok" : "Ko"
            };
        }

        [RoutingPosition(1)]
        public string DatacenterName;

        [RoutingPosition(2)]
        public string Status;

        public override string ToString()
        {
            return $"{DatacenterName} {Status}";
        }
    }

}
