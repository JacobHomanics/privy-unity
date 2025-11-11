using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Privy
{
    internal class WalletApiRepository
    {
        private PrivyConfig _privyConfig;
        private IHttpRequestHandler _httpRequestHandler;
        private AuthorizationKey _authorizationKey;

        internal WalletApiRepository(PrivyConfig privyConfig, IHttpRequestHandler httpRequestHandler,
            AuthorizationKey authorizationKey)
        {
            _privyConfig = privyConfig;
            _httpRequestHandler = httpRequestHandler;
            _authorizationKey = authorizationKey;
        }

        internal async Task<WalletApiCreateResponse> CreateWallet(WalletApiCreateRequest request, string accessToken)
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {accessToken}" }
            };

            string serializedRequest = JsonConvert.SerializeObject(request);

            try
            {
                string jsonResponse =
                    await _httpRequestHandler.SendRequestAsync("wallets", serializedRequest,
                        customHeaders: headers, method: "POST");

                var response = JsonConvert.DeserializeObject<WalletApiCreateResponse>(jsonResponse);
                return response;
            }
            catch (Exception errorResponse)
            {
                throw new PrivyException.EmbeddedWalletException($"Failed to create wallet: {errorResponse.Message}",
                    EmbeddedWalletError.CreateFailed);
            }
        }

        internal async Task<WalletApiRpcResponse> Rpc(WalletApiRpcRequest request, string walletId, string accessToken)
        {
            var path = $"wallets/{walletId}/rpc";
            var payload = new WalletApiPayload
            {
                Version = 1,
                Url = _httpRequestHandler.GetFullUrl(path),
                Method = "POST",
                Headers = new Dictionary<string, string> { { Constants.PRIVY_APP_ID_HEADER, _privyConfig.AppId } },
                Body = request
            };

            byte[] encodedPayload = payload.EncodePayload();
            byte[] signature = await _authorizationKey.Signature(encodedPayload);

            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {accessToken}" },
                { "privy-authorization-signature", Convert.ToBase64String(signature) }
            };

            string serializedRequest = JsonConvert.SerializeObject(request);

            try
            {
                string jsonResponse =
                    await _httpRequestHandler.SendRequestAsync($"wallets/{walletId}/rpc", serializedRequest,
                        customHeaders: headers, method: "POST");

                var response = JsonConvert.DeserializeObject<WalletApiRpcResponse>(jsonResponse);
                return response;
            }
            catch (Exception errorResponse)
            {
                UnityEngine.Debug.Log("AYE YO");
                UnityEngine.Debug.Log(request.Params.ToString());
                throw new PrivyException.EmbeddedWalletException($"Failed to execute RPC: {errorResponse.Message}",
                    EmbeddedWalletError.RpcRequestFailed);
            }
        }

        public async Task<WalletApiBalanceResponse> GetWalletBalance(WalletApiBalanceRequest request, string walletId, string accessToken)
        {
            var baseURL = "https://api.privy.io/v1/";
            HttpClient client = new()
            {
                BaseAddress = new Uri(baseURL)
            };

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Add(Constants.PRIVY_APP_ID_HEADER, _privyConfig.AppId);

            var queryString = string.Empty;

            for (var i = 0; i < request.asset.Length; i++)
            {
                var end = string.Empty;
                if (request.asset.Length > 1 && i != request.asset.Length - 1)
                    end = "&";

                queryString += $"asset={request.asset[i]}{end}";
            }

            if (request.chain.Length > 0)
                queryString += "&";

            for (var i = 0; i < request.chain.Length; i++)
            {
                var end = string.Empty;

                if (request.chain.Length > 1 && i != request.chain.Length - 1)
                    end = "&";

                string chainQuery;
                if (request.chain[i] == Chain.base_mainnet)
                    chainQuery = "base";
                else
                    chainQuery = request.chain[i].ToString();

                queryString += $"chain={chainQuery}{end}";
            }

            if (request.include_currency == IncludeCurrency.usd)
                queryString += "&include_currency=usd";


            var endPointUrl = $"wallets/{walletId}/balance?";

            try
            {
                var response = await client.GetAsync(endPointUrl + queryString);
                string responseBody = await response.Content.ReadAsStringAsync();
                var responseObj = JsonConvert.DeserializeObject<WalletApiBalanceResponse>(responseBody);
                return responseObj;
            }
            catch (Exception errorResponse)
            {
                throw new PrivyException.EmbeddedWalletException($"Failed to execute RPC: {errorResponse.Message}",
                    EmbeddedWalletError.RpcRequestFailed);
            }

            // var headers = new Dictionary<string, string>
            // {
            //     { "Authorization", $"Bearer {accessToken}" },
            //     { Constants.PRIVY_APP_ID_HEADER, _privyConfig.AppId }
            // };

            // string serializedRequest = JsonConvert.SerializeObject(request);

            // try
            // {
            //     string jsonResponse =
            //         await _httpRequestHandler.SendRequestAsync($"wallets/{walletId}/balance", serializedRequest,
            //             customHeaders: headers, method: "GET");

            //     var response = JsonConvert.DeserializeObject<object[]>(jsonResponse);
            //     return response;
            // }
            // catch (Exception errorResponse)
            // {
            //     throw new PrivyException.EmbeddedWalletException($"Failed to execute RPC: {errorResponse.Message}",
            //         EmbeddedWalletError.RpcRequestFailed);
            // }
        }
    }
}