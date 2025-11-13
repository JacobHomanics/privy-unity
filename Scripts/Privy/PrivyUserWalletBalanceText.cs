using UnityEngine;
using UnityEngine.UI;
using System;
using Privy;

public class PrivyUserWalletBalanceText : MonoBehaviour
{
    public Text text;

    public Asset asset;
    public Chain chain;

    [ContextMenu("Set")]
    public async void SetText()
    {
        try
        {
            PrivyUser privyUser = await PrivyManager.Instance.GetUser();
            IEmbeddedEthereumWallet embeddedWallet = privyUser.EmbeddedWallets[0];

            var results = await privyUser.GetWalletBalance(privyUser.EmbeddedWallets[0].Id, new Asset[1] { asset }, new Chain[1] { chain }, IncludeCurrency.usd);

            long.TryParse(results.balances[0].raw_value, out long a);
            long.TryParse(results.balances[0].raw_value_decimals, out long b);

            float result = a / (float)Math.Pow(10, b);

            text.text = $"{result} {asset}";

            /////////////////////////////////
            // Keep in case we want to implement a method like this: 
            /////////////////////////////////

            // var result = await PrivyManager.Instance.GetWalletBalanceAsync(embeddedWallet.Address);

            // var provider = embeddedWallet.RpcProvider;
            // // build a req
            // var rpcRequest = new RpcRequest
            // {
            //     Method = "eth_getBalance",
            //     Params = new string[] { embeddedWallet.Address, "latest" }
            // };

            // RpcResponse response = await provider.Request(rpcRequest);



            // RpcResponse response = await embeddedWallet.RpcProvider.Request(rpcRequest);

            // text.text = result;

            // var balance = await controller.GetBalance();
        }
        catch (Exception e)
        {
            if (e.Message == "Call PrivyManager.Initialize before attempting to get the Privy instance.")
            {
                Debug.LogError(e.Message);
            }
        }
    }

    public async void SetText(string asset, string chain)
    {
        try
        {
            PrivyUser privyUser = await PrivyManager.Instance.GetUser();
            IEmbeddedEthereumWallet embeddedWallet = privyUser.EmbeddedWallets[0];

            if (!Enum.TryParse(asset, out Asset assetEnum))
                Debug.LogError("Unable to parse asset enum");

            if (!Enum.TryParse(chain, out Chain chainEnum))
                Debug.LogError("Unable to parse chain enum");

            var results = await privyUser.GetWalletBalance(privyUser.EmbeddedWallets[0].Id, new Asset[1] { assetEnum }, new Chain[1] { chainEnum }, IncludeCurrency.usd);

            text.text = "";
        }
        catch (Exception e)
        {
            Debug.Log(e);
            if (e.Message == "Call PrivyManager.Initialize before attempting to get the Privy instance.")
            {
                Debug.LogError(e.Message);
            }
        }
    }
}
