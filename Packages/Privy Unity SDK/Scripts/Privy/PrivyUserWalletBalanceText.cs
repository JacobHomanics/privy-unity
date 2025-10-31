using UnityEngine;
using UnityEngine.UI;
using System;
using Privy;

public class PrivyUserWalletBalanceText : MonoBehaviour
{
    public Text text;

    public async void SetText()
    {
        try
        {
            PrivyUser privyUser = await PrivyManager.Instance.GetUser();
            IEmbeddedEthereumWallet embeddedWallet = privyUser.EmbeddedWallets[0];

            var results = await privyUser.GetWalletBalance(privyUser.EmbeddedWallets[0].Id);

            for (var i = 0; i < results.balances.Length; i++)
            {
                Debug.Log(results.balances[i].chain);
                Debug.Log(results.balances[i].asset);
                Debug.Log(results.balances[i].raw_value);
                Debug.Log(results.balances[i].raw_value_decimals);
            }

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
            // text.text = balance;
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

    async void Update()
    {

    }
}
