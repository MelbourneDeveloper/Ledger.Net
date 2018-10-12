namespace Ledger.Net
{
    public partial class LedgerManager
    {
        public class CallAndPromptArgs<T>
        {
            public string MemberName { get; set; }
            public T Args { get; set; }
            public LedgerManager LedgerManager { get; set; }
        }
    }
}
