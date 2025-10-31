namespace Privy
{
    public struct WalletApiBalanceRequest
    {
        public enum Asset { usdc, eth, pol, usdt, sol };
        public Asset asset;

        public enum Chain { ethereum, arbitrum, base_mainnet, linea, optimism, polygon, solana, zksync_era, sepolia, arbitrum_sepolia, base_sepolia, linea_testnet, optimism_sepolia, polygon_amoy }
        public Chain chain;

        public enum IncludeCurrency { none, usd }
        public IncludeCurrency include_currency;
    }
}