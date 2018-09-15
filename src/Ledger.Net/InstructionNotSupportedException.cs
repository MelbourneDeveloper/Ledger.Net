using System;

namespace Ledger.Net
{
    public class InstructionNotSupportedException : Exception
    {
        public byte[] Data { get; }

        public InstructionNotSupportedException(byte[] data) : base("The instruction sent to the device is not supported. This probably means that the instruction sent to the device is not implemented by the app that is currently loaded on the Ledger.")
        {
            Data = data;
        }
    }
}
