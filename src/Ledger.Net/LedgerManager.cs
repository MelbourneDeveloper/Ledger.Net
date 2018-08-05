using Hid.Net;

namespace Ledger.Net
{
    public class LedgerManager
    {
        #region Fields
        private IHidDevice _LedgerHidDevice;
        #endregion

        #region Constructor
        public LedgerManager(IHidDevice ledgerHidDevice)
        {
            _LedgerHidDevice = ledgerHidDevice;
        }
        #endregion
    }
}
