using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public class OriginatorInfo
    {
        public readonly PeerId SenderId;

        public readonly string SenderEndPoint;

        internal readonly string SenderMachineName;

        public string InitiatorUserName;

        public OriginatorInfo(PeerId senderId, string senderEndPoint, string senderMachineName, string initiatorUserName)
        {
            SenderId = senderId;
            SenderEndPoint = senderEndPoint;
            SenderMachineName = senderMachineName;
            InitiatorUserName = initiatorUserName;
        }

        internal OriginatorInfo()
        {
        }

        public string GetSenderMachineNameFromEndPoint()
        {
            var uri = new Uri(SenderEndPoint);
            return uri.Host;
        }
    }
}
