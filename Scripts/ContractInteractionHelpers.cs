using System;
using System.Globalization;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Privy;
using UnityEngine;

public static class ContractInteractionHelpers
{
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

    public static async Task<string> Call(string rpc, string to, string data)
    {
        // token contract on Base (replace with your actual contract)
        string token = to;
        string json = BuildEthCallJson(token, data);

        string rpcUrl = rpc;
        using var client = new HttpClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var res = await client.PostAsync(rpcUrl, content);
        return await res.Content.ReadAsStringAsync();
    }

    private static string BuildEthCallJson(string to, string data)
    {
        // Manually compose the JSON so "params" is a proper heterogeneous array
        return
            "{\"jsonrpc\":\"2.0\",\"method\":\"eth_call\",\"params\":[{\"to\":\"" + to +
            "\",\"data\":\"" + data + "\"},\"latest\"],\"id\":1}";
    }

    public static async Task<string> Sign(IEmbeddedEthereumWallet embeddedWallet, string to, string value, string chainId, string data)
    {
        var rpcRequest = BuildSignOrSend(embeddedWallet, to, value, chainId, data);
        rpcRequest.Method = "eth_signTransaction";

        try
        {
            RpcResponse transactionResponse = await embeddedWallet.RpcProvider.Request(rpcRequest);
            string transactionHash = transactionResponse.Data;
            return transactionResponse.Data;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return "";
        }
    }

    public static async Task<string> Send(IEmbeddedEthereumWallet embeddedWallet, string to, string value, string chainId, string data)
    {
        var rpcRequest = BuildSignOrSend(embeddedWallet, to, value, chainId, data);
        rpcRequest.Method = "eth_sendTransaction";

        try
        {
            RpcResponse transactionResponse = await embeddedWallet.RpcProvider.Request(rpcRequest);
            string transactionHash = transactionResponse.Data;
            return transactionResponse.Data;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return "";
        }
    }

    private static RpcRequest BuildSignOrSend(IEmbeddedEthereumWallet embeddedWallet, string to, string value, string chainId, string data)
    {
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

        // Create RPC request
        var rpcRequest = new RpcRequest
        {
            Params = new string[] { transactionJson }
        };

        return rpcRequest;
    }
}
