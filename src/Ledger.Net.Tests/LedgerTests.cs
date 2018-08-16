using Hid.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Ledger.Net.Tests
{
    public class LedgerTests
    {
        public static VendorProductIds[] WellKnownLedgerWallets = new VendorProductIds[]
        {
            new VendorProductIds(0x2c97),
            new VendorProductIds(0x2581, 0x3b7c)
        };

        static readonly UsageSpecification[] _UsageSpecification = new[] { new UsageSpecification(0xffa0, 0x01) };

        [Fact]
        public async Task GetAddress()
        {
            var devices = new List<DeviceInformation>();

            var collection = WindowsHidDevice.GetConnectedDeviceInformations();

            foreach (var ids in WellKnownLedgerWallets)
            {
                if (ids.ProductId == null)
                    devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId));
                else
                    devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId && c.ProductId == ids.ProductId));
            }

            var retVal = devices
                .FirstOrDefault(d =>
                _UsageSpecification == null ||
                _UsageSpecification.Length == 0 ||
                _UsageSpecification.Any(u => d.UsagePage == u.UsagePage && d.Usage == u.Usage));

            var ledgerHidDevice = new WindowsHidDevice(retVal);
            await ledgerHidDevice.InitializeAsync();
            var ledgerManager = new LedgerManager(ledgerHidDevice, new DefaultCoinUtility());

            var address = await ledgerManager.GetAddressAsync(0, false, 0, true);
            if (address == null)
            {
                throw new Exception("Address not returned");
            }
        }

        [Fact]
        public async Task GetEthereumAddress()
        {
            var devices = new List<DeviceInformation>();

            var collection = WindowsHidDevice.GetConnectedDeviceInformations();

            foreach (var ids in WellKnownLedgerWallets)
            {
                if (ids.ProductId == null)
                    devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId));
                else
                    devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId && c.ProductId == ids.ProductId));
            }

            var retVal = devices
                .FirstOrDefault(d =>
                _UsageSpecification == null ||
                _UsageSpecification.Length == 0 ||
                _UsageSpecification.Any(u => d.UsagePage == u.UsagePage && d.Usage == u.Usage));

            var ledgerHidDevice = new WindowsHidDevice(retVal);
            await ledgerHidDevice.InitializeAsync();
            var ledgerManager = new LedgerManager(ledgerHidDevice, new DefaultCoinUtility());
            ledgerManager.SetCoinNumber(60);
            var address = await ledgerManager.GetAddressAsync(0, false, 0, true);

            if (address == null)
            {
                throw new Exception("Address not returned");
            }
        }
    }

    public class VendorProductIds
    {
        public VendorProductIds(int vendorId)
        {
            VendorId = vendorId;
        }
        public VendorProductIds(int vendorId, int? productId)
        {
            VendorId = vendorId;
            ProductId = productId;
        }
        public int VendorId
        {
            get;
        }
        public int? ProductId
        {
            get;
        }
    }

    public class UsageSpecification
    {
        public UsageSpecification(ushort usagePage, ushort usage)
        {
            UsagePage = usagePage;
            Usage = usage;
        }

        public ushort Usage
        {
            get;
        }
        public ushort UsagePage
        {
            get;
        }
    }
}
