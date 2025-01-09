using System.Net;

namespace Unleash.Utilities
{
    internal class IPCIDRAddressRange
    {
        private readonly int _cidrCount;
        private readonly IPAddress _baseIPAddress;

        public IPCIDRAddressRange(string address)
        {
            var ipAndCidrPair = address.Split('/');
            _cidrCount = int.Parse(ipAndCidrPair[1]);
            _baseIPAddress = IPAddress.Parse(ipAndCidrPair[0]);
            var baseIpBytes = _baseIPAddress.GetAddressBytes();

            if (_cidrCount > (baseIpBytes.Length * 8))
            {
                _cidrCount = baseIpBytes.Length * 8;
            }
        }

        public bool Contains(IPAddress remoteAddress)
        {
            var baseIpBytes = _baseIPAddress.GetAddressBytes();
            var remoteBytes = remoteAddress.GetAddressBytes();

            if (remoteBytes.Length != baseIpBytes.Length)
            {
                return false;
            }

            var remaining = _cidrCount;
            var currentByte = 0;

            // Compare all bytes fully part of the subnet mask
            while (remaining > 8)
            {
                if (remoteBytes[currentByte] != baseIpBytes[currentByte])
                {
                    return false;
                }

                remaining -= 8;
                currentByte++;
            }

            // We've reached the end of the CIDR subnet mask
            if (remaining == 0)
            {
                return true;
            }

            // Blank out all variable bits so we can compare the bytes to each other
            for (var shift = 0; shift + remaining < 8; shift++)
            {
                byte mask = (byte)(1 << shift);
                remoteBytes[currentByte] &= (byte)~mask;
                baseIpBytes[currentByte] &= (byte)~mask;
            }

            // Done blanking out, compare
            return remoteBytes[currentByte] == baseIpBytes[currentByte];
        }
    }
}