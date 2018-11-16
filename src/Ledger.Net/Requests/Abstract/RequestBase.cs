using Ledger.Net.Devices;
using System;

namespace Ledger.Net.Requests
{
    public abstract class RequestBase
    {
        #region Public Abstract Properties
        public abstract byte Argument1 { get; }
        public abstract byte Argument2 { get; }
        public abstract byte Cla { get; }
        public abstract byte Ins { get; }
        #endregion

        #region Public Properties
        public byte[] Data { get; }
        public virtual bool Chunked => false;
        #endregion

        #region Constructor
        protected RequestBase(byte[] data)
        {
            Data = data;
        }
        #endregion

        #region Internal Methods
        internal System.Collections.Generic.IEnumerable<byte[]> ToAPDU(Device device)
        {
            if (device.SupportsMessageChunking && Chunked)
            {
                int offset = 0;
                while (offset < Data.Length - 1)
                    yield return GetApduChain(ref offset);
            } else
            {
                var apdu = new byte[Data.Length + 5];
                apdu[0] = Cla;
                apdu[1] = Ins;
                apdu[2] = Argument1;
                apdu[3] = Argument2;
                apdu[4] = (byte)(Data.Length);
                Array.Copy(Data, 0, apdu, 5, Data.Length);
                yield return apdu;
            }
        }
        #endregion

        #region Protected Methods
        protected abstract byte[] GetApduChain(ref int offset);
        
        #endregion
    }
}
