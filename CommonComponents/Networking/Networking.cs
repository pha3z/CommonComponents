using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Common
{
    public static class Networking
    {
        public static IPv4Range IPv4RangeFromCIDR(string CIDR_Range)
        {
            var (first, last) = IPv4RangeFromCIDR_AsTuple(CIDR_Range);
            return new IPv4Range(first, last);
        }

        public static (UInt32, UInt32) IPv4RangeFromCIDR_AsTuple(string CIDR_Range)
        {
            string[] parts = CIDR_Range.Trim().Split('.', '/');

            if (parts.Length < 5)
                throw new ArgumentException("Invalid CIDR ip range notation.");

            uint ipnum = (Convert.ToUInt32(parts[0]) << 24) |
                (Convert.ToUInt32(parts[1]) << 16) |
                (Convert.ToUInt32(parts[2]) << 8) |
                Convert.ToUInt32(parts[3]);

            int maskbits = Convert.ToInt32(parts[4]);
            uint mask = 0xffffffff;
            mask <<= (32 - maskbits);

            UInt32 first = ipnum & mask;
            UInt32 last = ipnum | (mask ^ 0xffffffff);

            return (first, last);
        }

        public static uint ConvertFromIPv4AddressToInteger(string ipAddress)
        {
            var address = IPAddress.Parse(ipAddress);
            byte[] bytes = address.GetAddressBytes();

            // flip big-endian(network order) to little-endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static string ConvertFromIntegerToIPv4Address(uint ipAddress)
        {
            byte[] bytes = BitConverter.GetBytes(ipAddress);

            // flip little-endian to big-endian(network order)
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return new IPAddress(bytes).ToString();
        }

        public static bool IsValidIpv4(string ipAddress)
        {
            Regex ipRegex = new Regex(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}$");
            return ipRegex.IsMatch(ipAddress);
        }

        public static bool IsValidMacAddress(string macAddr)
        {
            return Regex.IsMatch(macAddr, "([0-9a-fA-F]{2}:){5}[0-9a-fA-F]{2}");
        }
    }
}
