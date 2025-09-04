using System;
using Privy;
using TMPro;
using UnityEngine;

public class SignButton : MonoBehaviour
{

    public TMP_Text signedText;

    public async void Sign()
    {
        PrivyUser privyUser = await PrivyManager.Instance.GetUser();
        IEmbeddedEthereumWallet embeddedWallet = privyUser.EmbeddedWallets[0];

        var rpcRequest = new RpcRequest
        {
            Method = "personal_sign", //a supported method
            Params = new string[] { "A message to sign", embeddedWallet.Address } //an array of strings, with the message + address
        };

        try
        {
            //Now that the response has been constructed, we try to execute the request
            RpcResponse personalSignResponse = await embeddedWallet.RpcProvider.Request(rpcRequest);

            //If response is successful, we can parse out the data
            Debug.Log(personalSignResponse.Data);
            signedText.text = personalSignResponse.Data;
        }
        catch (PrivyException.EmbeddedWalletException ex)
        {
            //If the request method fails, we catch it here
            Debug.LogError($"Could not sign message due to error: {ex.Error} {ex.Message}");
        }
        catch (Exception ex)
        {
            //If there's some other error, unrelated to the request, catch this here
            Debug.LogError($"Could not sign message exception {ex.Message}");
        }
    }
}
