using Amazon.Runtime.Internal.Util;
using System;
using System.Net;

namespace AvePoint.RA.Common.Helper
{
    public class IPHelper
    {
        public static bool ValidIPv4Format(string ipSegment)
        {
            ipSegment = ipSegment.Trim();
            var splitIpFirstPath = ipSegment.Split('.');
            if (splitIpFirstPath.Length != 4) return false;
            if (IPAddress.TryParse(ipSegment, out var ip))
            {
                foreach (var part in splitIpFirstPath)
                {
                    if (part.Length > 1 && part.StartsWith("0"))
                    {
                        return false;
                    }
                }
                return ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
            }
            return false;
        }

        public static bool IsInSameSegment(string ipAddress, string range)
        {
            if (string.IsNullOrWhiteSpace(ipAddress) ||
                string.IsNullOrWhiteSpace(range))
            {
                return false;
            }

            // Single IP
            if (!range.Contains('/'))
            {
                return string.Equals(ipAddress, range, StringComparison.OrdinalIgnoreCase);
            }

            var parts = range.Split('/');
            if (parts.Length != 2)
            {
                return false;
            }

            if (!IPAddress.TryParse(ipAddress, out var clientIp) ||
                !IPAddress.TryParse(parts[0], out var startIp))
            {
                return false;
            }

            var clientBytes = clientIp.GetAddressBytes();
            var startBytes = startIp.GetAddressBytes();

            if (clientBytes.Length != 4 || startBytes.Length != 4)
            {
                return false;
            }

            // First 3 octets must match
            for (int i = 0; i < 3; i++)
            {
                if (clientBytes[i] != startBytes[i])
                {
                    return false;
                }
            }

            if (!int.TryParse(parts[1], out int endOctet))
            {
                return false;
            }

            int startOctet = startBytes[3];
            int clientOctet = clientBytes[3];

            return clientOctet >= startOctet &&
                   clientOctet <= endOctet;
        }
    }
}
