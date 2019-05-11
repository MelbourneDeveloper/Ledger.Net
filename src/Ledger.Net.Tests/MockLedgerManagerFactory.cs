using Device.Net;

namespace Ledger.Net.Tests
{
    public class MockLedgerManagerFactory : ILedgerManagerFactory
    {
        public ILedgerManager GetNewLedgerManager(IDevice ledgerHidDevice, ICoinUtility coinUtility, ErrorPromptDelegate errorPrompt)
        {
            return new MockLedgerManager();
        }
    }
}
