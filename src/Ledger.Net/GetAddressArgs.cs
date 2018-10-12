namespace Ledger.Net
{
    public partial class LedgerManager
    {
        public class GetAddressArgs
        {
            public uint Account { get; set; }
            public uint Index { get; set; }
            public bool IsChange { get; set; }
            public bool ShowDisplay { get; set; }

            public GetAddressArgs(uint account, uint index, bool isChange, bool showDisplay)
            {
                Account = account;
                Index = index;
                IsChange = isChange;
                ShowDisplay = showDisplay;
            }
        }
    }
}
