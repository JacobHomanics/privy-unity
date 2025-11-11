using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Privy
{
    internal class RpcProvider : IRpcProvider
    {
        private readonly IRpcExecutor _rpcExecutor;

        private static readonly HashSet<string> _allowedMethods = new HashSet<string>
        {
            "eth_sign",
            "secp256k1_sign",
            "personal_sign",
            "eth_populateTransactionRequest",
            "eth_signTypedData_v4",
            "eth_signTransaction",
            "eth_sendTransaction",
            "eth_call"
        };

        public RpcProvider(IRpcExecutor rpcExecutor)
        {
            _rpcExecutor = rpcExecutor;
        }

        public async Task<RpcResponse> Request(RpcRequest request)
        {
            for (var i = 0; i < request.Params.Length; i++)
                UnityEngine.Debug.Log(request.Params[i]);

            if (_allowedMethods.Contains(request.Method))
            {
                var requestDetails = new RpcRequestData.EthereumRpcRequestDetails
                {
                    Method = request.Method,
                    Params = request.Params
                };
                var responseDetails = await _rpcExecutor.Evaluate(requestDetails);

                if (responseDetails is RpcResponseData.EthereumRpcResponseDetails response)
                {
                    return new RpcResponse
                    {
                        Method = response.Method,
                        Data = response.Data
                    };
                }

                UnityEngine.Debug.Log("UH OG");
                throw new PrivyException.EmbeddedWalletException($"Failed to execute RPC Request",
                    EmbeddedWalletError.RpcRequestFailed);
            }
            else
            {
                return await HandleJsonRpc(request);
            }
        }

        private async Task<RpcResponse> HandleJsonRpc(RpcRequest request)
        {
            PrivyLogger.Debug("Unsupported rpc request type");
            return null;
        }
    }
}