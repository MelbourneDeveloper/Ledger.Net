namespace Ledger.Net
{
    public static class Constants
    {
        #region Generic App Constants
        public const byte CLA = 0xE0;

        public const byte P1_MORE = 0x80;
        public const byte P1_LAST = 0x90;
        public const byte P1_SIGN = 0x10;

        public const int DEFAULT_CHANNEL = 0x0101;
        public const int LEDGER_HID_PACKET_SIZE = 64;
        public const int LEDGER_MAX_DATA_SIZE = 255;
        public const int TAG_APDU = 0x05;
        #endregion

        #region Bitcoin Blue Instructions
        public const byte BTCHIP_INS_GET_WALLET_PUBLIC_KEY = 0x40;
        public const byte BTCHIP_INS_GET_COIN_VER = 22;
        #endregion

        #region Ethereum Blue Instructions
        public const byte ETHEREUM_GET_WALLET_PUBLIC_KEY = 0x02;
        public const byte ETHEREUM_SIGN_TX = 0x04;
        public const byte ETHEREUM_SIGN_MESSAGE = 0x08;
        #endregion

        #region Tron Instructions
        public const byte TRON_SIGN_TX = 0x04;
        #endregion

        #region Status Codes
        public const int SuccessStatusCode = 0x9000;
        public const int SecurityNotValidStatusCode = 0x6982;
        public const int InstructionNotSupportedStatusCode = 0x6D00;
        public const int IncorrectLengthStatusCode = 0x6700;
        #endregion
    }
}
