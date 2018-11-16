using System;
using System.Collections.Generic;
using System.Text;

namespace Ledger.Net.Devices.Concrete
{
    public class LedgerDevice: Device
    {
        #region Public Properties
        public override bool SupportsMessageChunking => true;
        #endregion
        #region Constructor
        internal LedgerDevice(Hid.Net.IHidDevice device): base(device)
        {

        }
        #endregion
    }
}
