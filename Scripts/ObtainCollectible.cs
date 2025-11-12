using System;
using System.Globalization;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Privy;
using UnityEngine;

public class ObtainCollectible : MonoBehaviour
{

    public async void SendMe()
    {
        Debug.Log("Step 1");
        var user = await PrivyManager.Instance.GetUser();
        Debug.Log("Step 2");
        var hash = await Send(user.EmbeddedWallets[0], "0x2974cdd03d02c3027ad58c5d9e530cd4b5534b0e", "0x0000000000000000000000000000000000000000", "0x2105", "0x1249c58b84ff771f36a0d1d2bf0b42e48832b1567c4213f113d3990903cea57d");
        Debug.Log(hash);
    }

    static BigInteger ParseRpcUint256(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return BigInteger.Zero;
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) hex = hex.Substring(2);
        if (hex.Length == 0) return BigInteger.Zero;

        // Interpret the string as big-endian hex
        return BigInteger.Parse(hex, NumberStyles.AllowHexSpecifier);
    }

    public async void Call()
    {
        Debug.Log("Step 1");
        var user = await PrivyManager.Instance.GetUser();
        Debug.Log("Step 2");

        // Encode balanceOf(address) function call with user's wallet address
        string balanceOfData = SmartContractHelper.CreateERC20BalanceOfData(user.EmbeddedWallets[0].Address);
        Debug.Log($"Encoded balanceOf data: {balanceOfData}");

        var hash = await Call("0x2974cdd03d02c3027ad58c5d9e530cd4b5534b0e", balanceOfData);

        string hex = ExtractResultHex(hash);

        if (!string.IsNullOrEmpty(hex))
        {
            BigInteger balance = ParseRpcUint256(hex);
            Debug.Log($"Balance: {balance}");
        }
        Debug.Log(hash);
    }


    [System.Serializable]
    public class SignTransactionData
    {
        public string to; public string value; public string chainId; public string data; public string from;
    }

    [System.Serializable]
    public class CallTransactionData
    {
        public string to; public string chainId; public string data; public string from;
    }


    [Serializable]
    public class EthCallParams
    {
        public string to;
        public string data;
    }

    [Serializable]
    public class RpcRequest2
    {
        public string jsonrpc = "2.0";
        public string method = "eth_call";
        public object[] @params;
        public int id = 1;
    }


    public async Task<string> Call(string to, string data)
    {
        // token contract on Base (replace with your actual contract)
        string token = to;
        string json = BuildEthCallJson(token, data);

        string rpcUrl = "https://base.llamarpc.com";
        using var client = new HttpClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var res = await client.PostAsync(rpcUrl, content);
        return await res.Content.ReadAsStringAsync();


        // var rpcUrl = "https://base.llamarpc.com";

        // // 1️⃣  Build call parameters
        // var callParams = new EthCallParams
        // {
        //     to = to, // DAI contract
        //     data = data
        // };

        // // 2️⃣  Wrap in JSON-RPC request
        // var rpc = new RpcRequest2
        // {
        //     @params = new object[] { callParams, "latest" }
        // };

        // var jsonBody = JsonUtility.ToJson(rpc);
        // using var client = new HttpClient();
        // var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        // try
        // {
        //     var response = await client.PostAsync(rpcUrl, content);
        //     var result = await response.Content.ReadAsStringAsync();

        //     Debug.Log("RPC Response: " + result);
        //     return result;
        // }
        // catch (Exception ex)
        // {
        //     Debug.LogError("RPC call failed: " + ex.Message);
        //     return "";
        // }
    }

    private static string BuildEthCallJson(string to, string data)
    {
        // Manually compose the JSON so "params" is a proper heterogeneous array
        return
            "{\"jsonrpc\":\"2.0\",\"method\":\"eth_call\",\"params\":[{\"to\":\"" + to +
            "\",\"data\":\"" + data + "\"},\"latest\"],\"id\":1}";
    }

    public async Task<string> Send(IEmbeddedEthereumWallet embeddedWallet, string to, string value, string chainId, string data)
    {
        Debug.Log("Step 3");

        Debug.Log(chainId);

        var sign = new SignTransactionData()
        {
            to = to,
            data = data, //Compiled code of a contract OR hash of the invoked method signature and encoded parameters.
            value = value, //Example: "0x186a0", // 100000 in hex
            chainId = chainId, // Example: "0x2105", // 8453 (Base) in hex
            from = embeddedWallet.Address
        };

        // Create transaction JSON
        string transactionJson = JsonUtility.ToJson(sign);

        Debug.Log("Step 4");

        Debug.Log(transactionJson);

        try
        {
            // Create RPC request
            var rpcRequest = new RpcRequest
            {
                Method = "eth_sendTransaction",
                Params = new string[] { transactionJson }
            };

            // Sign transaction
            RpcResponse transactionResponse = await embeddedWallet.RpcProvider.Request(rpcRequest);

            Debug.Log("Step 5");

            // Transaction hash is in the response data
            string transactionHash = transactionResponse.Data;
            return transactionHash;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return "";
        }
    }

    // Very lightweight extractor; for production use a JSON lib like Newtonsoft.Json
    private static string ExtractResultHex(string rpcResponse)
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

    private static BigInteger HexToBigInt(string hex)
    {
        // strip 0x
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hex = hex.Substring(2);

        // pad even length
        if (hex.Length % 2 == 1) hex = "0" + hex;

        // big-endian bytes
        int len = hex.Length / 2;
        byte[] bytes = new byte[len];
        for (int i = 0; i < len; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

        // BigInteger expects little-endian; reverse and add a 0-byte to force positive
        Array.Reverse(bytes);
        var pos = new byte[bytes.Length + 1];
        Array.Copy(bytes, 0, pos, 1, bytes.Length);
        return new BigInteger(pos);
    }
}
