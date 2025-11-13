using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Privy;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NFTInteractionExample : MonoBehaviour
{
    [System.Serializable]
    public class SignOrSendValues
    {
        public string to;
        public string value;
        public string chain;
        public string data;
    }

    [System.Serializable]
    public class CallValues
    {
        public string rpc;
        public string to;
    }

    public CallValues callValues = new()
    {
        rpc = "https://base.llamarpc.com",
        to = "0x2974cdd03d02c3027ad58c5d9e530cd4b5534b0e"
    };

    public SignOrSendValues signOrSendValues = new()
    {
        to = "0x2974cdd03d02c3027ad58c5d9e530cd4b5534b0e",
        value = "0x0000000000000000000000000000000000000000",
        chain = "0x2105",
        data = "0x1249c58b84ff771f36a0d1d2bf0b42e48832b1567c4213f113d3990903cea57d"
    };

    public UnityEvent<int> OnBalanceOf;
    public UnityEvent<string> OnSign;
    public UnityEvent<string> OnSend;

    public GameObject go;
    public Text text;

    public async void Call()
    {
        var user = await PrivyManager.Instance.GetUser();

        // Encode balanceOf(address) function call with user's wallet address
        string balanceOfData = SmartContractHelper.CreateERC20BalanceOfData(user.EmbeddedWallets[0].Address);

        var hash = await ContractInteractionHelpers.Call(callValues.rpc, callValues.to, balanceOfData);

        string hex = ExtractResultHex(hash);

        if (!string.IsNullOrEmpty(hex))
        {
            BigInteger balance = ParseRpcUint256(hex);
            OnBalanceOf?.Invoke((int)balance);

            go.SetActive((int)balance > 0);
            text.text = ((int)balance).ToString();
        }
    }

    public async void Sign()
    {
        var user = await PrivyManager.Instance.GetUser();
        var hash = await ContractInteractionHelpers.Sign(user.EmbeddedWallets[0], signOrSendValues.to, signOrSendValues.value, signOrSendValues.chain, signOrSendValues.data);
        OnSign?.Invoke(hash);
        Debug.Log(hash);
    }

    public async void Send()
    {
        var user = await PrivyManager.Instance.GetUser();
        var hash = await ContractInteractionHelpers.Send(user.EmbeddedWallets[0], signOrSendValues.to, signOrSendValues.value, signOrSendValues.chain, signOrSendValues.data);

        OnSend?.Invoke(hash);
        Debug.Log(hash);
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(3f);
        Call();
    }

    static BigInteger ParseRpcUint256(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return BigInteger.Zero;
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) hex = hex.Substring(2);
        if (hex.Length == 0) return BigInteger.Zero;

        // Interpret the string as big-endian hex
        return BigInteger.Parse(hex, NumberStyles.AllowHexSpecifier);
    }

    // Very lightweight extractor; for production use a JSON lib like Newtonsoft.Json
    static string ExtractResultHex(string rpcResponse)
    {
        // Expecting: {"jsonrpc":"2.0","id":1,"result":"0x..."}
        const string key = "\"result\":\"";
        int i = rpcResponse.IndexOf(key, StringComparison.Ordinal);
        if (i < 0) return null;
        int start = i + key.Length;
        int end = rpcResponse.IndexOf('"', start);
        if (end < 0) return null;
        return rpcResponse.Substring(start, end - start);
    }

}
