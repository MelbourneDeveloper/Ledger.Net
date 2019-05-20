using Device.Net;

namespace Ledger.Net
{
    public interface ILedgerManagerFactory
    {
        IManagesLedger GetNewLedgerManager(IDevice ledgerHidDevice, ICoinUtility coinUtility, ErrorPromptDelegate errorPrompt);
    }
}
