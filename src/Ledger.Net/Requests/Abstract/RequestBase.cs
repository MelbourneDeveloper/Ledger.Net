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
        #endregion

        #region Constructor
        protected RequestBase(byte[] data)
        {
            Data = data;
        }
        #endregion

        #region Internal Methods
        internal byte[] ToAPDU()
        {
            var apdu = new byte[Data.Length + 5];
            apdu[0] = Cla;
            apdu[1] = Ins;
            apdu[2] = Argument1;
            apdu[3] = Argument2;
            apdu[4] = (byte)(Data.Length);
            Array.Copy(Data, 0, apdu, 5, Data.Length);
            return apdu;
        }
        #endregion
    }
}
