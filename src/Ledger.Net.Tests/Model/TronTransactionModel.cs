using System.Collections.Generic;

namespace Ledger.Net.Tests.Model
{
    public class Value
    {
        public int amount { get; set; }
        public string asset_name { get; set; }
        public string owner_address { get; set; }
        public string to_address { get; set; }
    }

    public class Parameter
    {
        public Value value { get; set; }
        public string type_url { get; set; }
    }

    public class Contract
    {
        public Parameter parameter { get; set; }
        public string type { get; set; }
    }

    public class RawData
    {
        public List<Contract> contract { get; set; }
        public string ref_block_bytes { get; set; }
        public string ref_block_hash { get; set; }
        public long expiration { get; set; }
        public long timestamp { get; set; }
    }

    public class TronTransactionModel
    {
        public List<string> signature { get; set; }
        public string txID { get; set; }
        public RawData raw_data { get; set; }
        public string raw_data_hex { get; set; }
    }
}
