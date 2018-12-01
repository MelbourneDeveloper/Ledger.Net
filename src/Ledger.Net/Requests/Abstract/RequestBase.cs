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

        #region Private Methods
        private byte[] GetNextApduCommand(ref int offset)
        {
            var chunkSize = offset + Constants.LEDGER_MAX_DATA_SIZE > Data.Length ?
                Data.Length - offset :
                Constants.LEDGER_MAX_DATA_SIZE;

            byte[] buffer = new byte[5 + chunkSize];
            buffer[0] = Cla;
            buffer[1] = Ins;
            buffer[2] = (byte)(offset == 0 ? Argument1 : Constants.P1_MORE);
            buffer[3] = Argument2;
            buffer[4] = (byte)(chunkSize);
            Array.Copy(Data, offset, buffer, 5, chunkSize);

            offset += chunkSize;
            return buffer;
        }
        #endregion

        #region Internal Methods
        internal System.Collections.Generic.IEnumerable<byte[]> ToAPDU()
        {
            int offset = 0;
            while (offset < Data.Length - 1)
                yield return GetNextApduCommand(ref offset);
        }
        #endregion
    }
}
