using Device.Net;
using System;
using System.Threading.Tasks;

namespace Ledger.Net.Tests
{
    public class MockLedgerDevice : IDevice
    {
        public MockLedgerDevice(string deviceId)
        {
            DeviceId = deviceId;
        }

        public bool IsInitialized { get; private set; }

        public string DeviceId { get; private set; }

        public ConnectedDeviceDefinitionBase ConnectedDeviceDefinition => throw new NotImplementedException();

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task InitializeAsync()
        {
            IsInitialized = true;
        }

        public Task<byte[]> ReadAsync()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> WriteAndReadAsync(byte[] writeBuffer)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
