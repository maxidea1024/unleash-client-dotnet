namespace Ganpa.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Ganpa.Internal;
    using Ganpa.Logging;
    using Ganpa.Utilities;

    public class RemoteAddressStrategy : IStrategy
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(DefaultGanpa));

        private const string PARAMETER_NAME = "IPs";

        public string Name => "remoteAddress";

        public bool IsEnabled(Dictionary<string, string> parameters, GanpaContext? context = null)
        {
            var remoteAddress = context?.RemoteAddress;

            if (string.IsNullOrEmpty(remoteAddress) || !IPAddress.TryParse(remoteAddress, out var remoteIpAddress))
            {
                return false;
            }

            if (!parameters.TryGetValue(PARAMETER_NAME, out var remoteAddresses))
            {
                return false;
            }

            var addresses = remoteAddresses
                .Split(',')
                .Select(x => x.Trim())
                .ToList();

            if (addresses.Contains(remoteAddress))
            {
                return true;
            }

            var addressRanges = ToAddressRanges(addresses);

            if (!addressRanges.Any())
            {
                return false;
            }

            return addressRanges
                .Any(range => range.Contains(remoteIpAddress));
        }

        private List<IPCIDRAddressRange> ToAddressRanges(List<string> ipAddresses)
        {
            var addressRanges = new List<IPCIDRAddressRange>(ipAddresses.Count);
            foreach (var address in ipAddresses.Where(address => address.IndexOf('/') > -1))
            {
                try
                {
                    addressRanges.Add(new IPCIDRAddressRange(address));
                }
                catch (Exception ex)
                {
                    Logger.Error(() =>
                        $"GANPA: RemoteAddressStrategy->ToAddressRanges threw exception: {ex.Message}. (Badly formatted IP/CIDR?)");
                }
            }

            return addressRanges;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, GanpaContext context,
            IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}