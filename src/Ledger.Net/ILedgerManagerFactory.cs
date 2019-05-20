using Device.Net;

namespace Ledger.Net
{
    public interface IManagesLedgerFactory
    {
        IManagesLedger GetNewLedgerManager(IDevice ledgerHidDevice, ICoinUtility coinUtility, ErrorPromptDelegate errorPrompt);
    }
}
