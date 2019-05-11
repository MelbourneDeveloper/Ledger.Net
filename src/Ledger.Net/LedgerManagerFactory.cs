using Device.Net;

namespace Ledger.Net
{
    public class LedgerManagerFactory : ILedgerManagerFactory
    {
        public ILedgerManager GetNewLedgerManager(IDevice ledgerHidDevice, ICoinUtility coinUtility, ErrorPromptDelegate errorPrompt)
        {
            return new LedgerManager(ledgerHidDevice, coinUtility, errorPrompt);
        }
    }
}
