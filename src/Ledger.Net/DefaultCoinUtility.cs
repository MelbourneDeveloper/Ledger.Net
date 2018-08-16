using System;

namespace Ledger.Net
{
    public class DefaultCoinUtility : ICoinUtility
    {
        public CoinInfo GetCoinInfo(uint coinNumber)
        {
            switch (coinNumber)
            {
                case 0:
                    return new CoinInfo(App.Bitcoin, "BTC", "Bitcoin", coinNumber, true);
                case 60:
                    return new CoinInfo(App.Ethereum, "ETH", "Ethereum", coinNumber, false);
                case 61:
                    return new CoinInfo(App.Ethereum, "ETC", "Ethereum Classic", coinNumber, false);
                default:
                    throw new NotImplementedException("Coin not implemented. You can implement your own ICoinUtility for other coins and set CoinUtility in the LedgerManager constructor.");
            }
        }
    }
}
