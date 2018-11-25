using Hid.Net;
using System.Collections.Generic;
using System.Linq;

namespace Ledger.Net.Tests
{
    public partial class LedgerTests
    {
        private static WindowsHidDevice GetLedgerDevice()
        {
            var devices = new List<DeviceInformation>();

            var collection = WindowsHidDevice.GetConnectedDeviceInformations();

            foreach (var ids in WellKnownLedgerWallets)
            {
                if (ids.ProductId == null)
                {
                    devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId));
                }
                else
                {
                    devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId && c.ProductId == ids.ProductId));
                }
            }

            var retVal = devices
                .FirstOrDefault(d =>
                _UsageSpecification == null ||
                _UsageSpecification.Length == 0 ||
                _UsageSpecification.Any(u => d.UsagePage == u.UsagePage && d.Usage == u.Usage));

            var ledgerHidDevice = new WindowsHidDevice(retVal);
            return ledgerHidDevice;
        }
    }
}
