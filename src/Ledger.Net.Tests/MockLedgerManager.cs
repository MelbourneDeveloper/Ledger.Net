using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ledger.Net.Tests
{
    public class MockLedgerManager : LedgerManagerBase
    {
        public override string DeviceId => throw new NotImplementedException();

        public override Task ReconnectAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task<byte[]> ReadAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task<IEnumerable<byte[]>> WriteRequestAndReadAsync<TRequest>(TRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
