namespace Privy
{
    public struct WalletApiBalanceResponse
    {
        public Balance[] balances;
    }

    public struct Balance
    {
        public string chain;
        public string asset;
        public string raw_value;
        public string raw_value_decimals;
        // "display_values": {
        // "eth": "0.001",
        // "usd": "2.56"
        // }
    }
}