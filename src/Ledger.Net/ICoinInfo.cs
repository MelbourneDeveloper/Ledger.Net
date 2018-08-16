namespace Ledger.Net
{
    public interface ICoinInfo
    {
        App App { get;  }
        uint CoinNumber { get;  }
        string FullName { get; }
        bool IsSegwit { get;  }
        string ShortName { get;  }
    }
}