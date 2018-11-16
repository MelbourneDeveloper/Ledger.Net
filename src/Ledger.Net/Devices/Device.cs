using Hid.Net;
using Ledger.Net.Devices.Concrete;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ledger.Net.Devices
{
    public class Device
    {
        #region Public Properties
        public virtual bool SupportsMessageChunking => false;
        public IHidDevice LedgerHidDevice { get; }
        #endregion
        #region Factory Methods
        public static Device GetDevice(IHidDevice device)
        {
            if (Array.Exists<int>(Constants.LedgerVendorIds, id => id == device.VendorId))
                return new LedgerDevice(device);
            // Add TrezorDevice instantiation when apply
            return new Device(device);
        }
        #endregion
        #region Constructor
        internal Device(IHidDevice device)
        {
            LedgerHidDevice = device;
        }
        #endregion
        #region Public Methods
        public Task InitializeAsync()
        {
            return LedgerHidDevice.InitializeAsync();
        }

        public Task WriteAsync(byte[] data)
        {
            return LedgerHidDevice.WriteAsync(data);
        }

        public Task<byte[]> ReadAsync()
        {
            return LedgerHidDevice.ReadAsync();
        }
        #endregion
    }
}
