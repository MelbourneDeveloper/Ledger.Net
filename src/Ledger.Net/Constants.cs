namespace Ledger.Net
{
    public static class Constants
    {
        #region Generic App Constants
        public const byte CLA = 0xE0;
        public const int DEFAULT_CHANNEL = 0x0101;
        public const int LEDGER_HID_PACKET_SIZE = 64;
        public const int TAG_APDU = 0x05;
        #endregion

        #region Bitcoin Blue Instructions
        public const byte BTCHIP_INS_GET_WALLET_PUBLIC_KEY = 0x40;
        public const byte BTCHIP_INS_GET_COIN_VER = 22;
        #endregion

        #region Ethereum Blue Instructions
        public const byte ETHEREUM_GET_WALLET_PUBLIC_KEY = 0x02;
        public const byte ETHEREUM_SIGN_TX = 0x04;
        #endregion

        #region Other Constants
        public const uint HARDENING_CONSTANT = 0x80000000;
        #endregion
    }
}
