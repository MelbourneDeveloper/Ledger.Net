using System;

namespace Ledger.Net
{
    public class DefaultCoinUtility : ICoinUtility
    {
        public CoinInfo GetCoinInfo(uint coinNumber)
        {
            var coinInfo = new CoinInfo { CoinNumber = coinNumber };

            switch(coinNumber)
            {
                case 0:
                    coinInfo.App = App.Bitcoin;
                    coinInfo.FullName = "Bitcoin";
                    coinInfo.ShortName = "BTC";
                    coinInfo.IsSegwit = true;
                    return coinInfo;
                case 60:
                    coinInfo.App = App.Ethereum;
                    coinInfo.FullName = "Ethereum";
                    coinInfo.ShortName = "ETC";
                    coinInfo.IsSegwit = false;
                    return coinInfo;
                case 61:
                    coinInfo.App = App.Ethereum;
                    coinInfo.FullName = "Ethereum";
                    coinInfo.ShortName = "ETC";
                    coinInfo.IsSegwit = false;
                    return coinInfo;
                default:
                    throw new NotImplementedException("Coin not implemented. You can implement your own ICoinUtility for other coins.");
            }
        }
    }
}
