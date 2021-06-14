using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class IPv4Range
    {
        public UInt32 FirstIp;
        public UInt32 LastIp;

        /// <summary>
        /// Example input: 5.39.40.96/27
        /// </summary>
        /// <param name="CIDR_Range">Example input: 5.39.40.96/27</param>
        public IPv4Range(string CIDR_Range)
        {
            (FirstIp, LastIp) = Networking.IPv4RangeFromCIDR_AsTuple(CIDR_Range);
        }

        public IPv4Range(UInt32 firstIp, UInt32 lastIp)
        {
            FirstIp = firstIp;
            LastIp = lastIp;
        }

        public IPv4Range(string firstIp, string lastIp)
        {
            FirstIp = Networking.ConvertFromIPv4AddressToInteger(firstIp);
            LastIp = Networking.ConvertFromIPv4AddressToInteger(lastIp);
        }

        public bool ContainsIp(string ipAddr)
        {
            UInt32 ip = Networking.ConvertFromIPv4AddressToInteger(ipAddr);
            return (FirstIp <= ip && ip <= LastIp);
        }
    }
}
