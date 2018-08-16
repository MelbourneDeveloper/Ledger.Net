namespace Ledger.Net
{
    public class CoinInfo 
    {
        public App App { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public uint CoinNumber { get; set; }
        public bool IsSegwit { get; set; }
    }
}
