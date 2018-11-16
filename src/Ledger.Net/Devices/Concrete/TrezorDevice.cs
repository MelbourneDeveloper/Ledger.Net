using System;
using System.Collections.Generic;
using System.Text;

namespace Ledger.Net.Devices.Concrete
{
    public class TrezorDevice: Device
    {
        // Not yet implemented. Does trezor need special behaviors?
        // If needed, implement them here and make sure to instantiate this instead of
        // Device
        internal TrezorDevice(Hid.Net.IHidDevice device) : base(device)
        {

        }
    }
}
