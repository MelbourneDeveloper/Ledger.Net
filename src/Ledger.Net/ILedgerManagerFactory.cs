using Device.Net;

namespace Ledger.Net
{
    public interface ILedgerManagerFactory
    {
        ILedgerManager GetNewLedgerManager(IDevice ledgerHidDevice, ICoinUtility coinUtility, ErrorPromptDelegate errorPrompt);
    }
}
