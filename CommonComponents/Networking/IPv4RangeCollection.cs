using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Stores a list of iPranges and provides function to check if an ip exists in any of the ranges
    /// </summary>
    public class IPv4RangeCollection
    {
        readonly IEnumerable<IPv4Range> _ipRanges;

        public IPv4RangeCollection(IEnumerable<string> ipRanges)
        {
            _ipRanges = ipRanges.Select(cidr => new IPv4Range(cidr));
        }

        public IPv4RangeCollection(IEnumerable<IPv4Range> ipRanges)
        {
            _ipRanges = ipRanges;
        }

        /// <summary>
        /// Returns the range to which the IpAddress belongs, or null if not found.
        /// </summary>
        /// <param name="IPAddress"></param>
        /// <returns></returns>
        public IPv4Range GetRange(string IPAddress)
        {
            foreach(var ipRange in _ipRanges)
            {
                if(ipRange.ContainsIp(IPAddress))
                    return ipRange;
            }

            return null;
        }
    }
}
