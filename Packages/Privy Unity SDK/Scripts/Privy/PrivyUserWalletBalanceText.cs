using UnityEngine;
using UnityEngine.UI;
using System;
using Privy;

public class PrivyUserWalletBalanceText : MonoBehaviour
{
    public Text text;

    async void Update()
    {
        try
        {
            PrivyUser privyUser = await PrivyManager.Instance.GetUser();
            IEmbeddedEthereumWallet embeddedWallet = privyUser.EmbeddedWallets[0];

            string transactionJson = JsonUtility.ToJson(new
            {
                chainId = 0x2105, // "0x2105", // 8453 (Base) in hex
                from = embeddedWallet.Address
            });

            var rpcRequest = new RpcRequest
            {
                Method = "eth_getBalance", //a supported method
                Params = new string[] { transactionJson } //an array of strings, with the message + address
            };

            RpcResponse response = await embeddedWallet.RpcProvider.Request(rpcRequest);

            text.text = response.Data;

            // var balance = await controller.GetBalance();
            // text.text = balance;
        }
        catch (Exception e)
        {
            if (e.Message == "Call PrivyManager.Initialize before attempting to get the Privy instance.")
            {
                Debug.LogError(e.Message);
            }
        }
    }
}
