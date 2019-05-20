using Device.Net;

namespace Ledger.Net.Tests
{
    public class MockLedgerManagerFactory : ILedgerManagerFactory
    {
        public IManagesLedger GetNewLedgerManager(IDevice ledgerHidDevice, ICoinUtility coinUtility, ErrorPromptDelegate errorPrompt)
        {
            return new LedgerManager(new MockLedgerManagerTransport());
        }
    }
}
