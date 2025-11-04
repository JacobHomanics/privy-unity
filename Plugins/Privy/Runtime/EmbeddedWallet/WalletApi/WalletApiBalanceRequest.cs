namespace Privy
{
    public enum Asset { usdc, eth, pol, usdt, sol };
    public enum Chain { ethereum, arbitrum, base_mainnet, linea, optimism, polygon, solana, zksync_era, sepolia, arbitrum_sepolia, base_sepolia, linea_testnet, optimism_sepolia, polygon_amoy }
    public enum IncludeCurrency { none, usd }

    public struct WalletApiBalanceRequest
    {
        public Asset[] asset;

        public Chain[] chain;

        public IncludeCurrency include_currency;
    }
}