using Device.Net;

namespace Ledger.Net.Tests
{
    public class MockLedgerManagerFactory : IManagesLedgerFactory
    {
        public IManagesLedger GetNewLedgerManager(IDevice ledgerHidDevice, ICoinUtility coinUtility, ErrorPromptDelegate errorPrompt)
        {
            return new LedgerManager(new MockLedgerManagerTransport());
        }
    }
}
