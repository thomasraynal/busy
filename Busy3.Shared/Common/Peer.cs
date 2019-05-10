using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class Peer
    {
        public PeerId Id { get; set; }

        public string EndPoint { get; set; }

        public bool IsUp { get; set; }

        public bool IsResponding { get; set; }

        public Peer(PeerId id, string endPoint, bool isUp = true) : this(id, endPoint, isUp, isUp)
        {
        }

        public Peer(Peer other) : this(other.Id, other.EndPoint, other.IsUp, other.IsResponding)
        {
        }

        public Peer(PeerId id, string endPoint, bool isUp, bool isResponding)
        {
            Id = id;
            EndPoint = endPoint;
            IsUp = isUp;
            IsResponding = isResponding;
        }

        public Peer()
        {
        }

        public override string ToString() => $"{Id}, {EndPoint}";

        public string GetMachineNameFromEndPoint()
        {
            var uri = new Uri(EndPoint);
            return uri.Host;
        }
    }
}
