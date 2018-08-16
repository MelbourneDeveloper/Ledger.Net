using System;
using System.Collections.Generic;
using System.Text;

namespace Ledger.Net
{
    public interface ICoinUtility
    {
        CoinInfo GetCoinInfo(uint coinNumber);
    }
}
