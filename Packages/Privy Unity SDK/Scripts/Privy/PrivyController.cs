using UnityEngine;
using Privy;
using System.Threading.Tasks;
using System;
using UnityEngine.Events;

public class PrivyController : MonoBehaviour
{
    [Header("Configuration")]
    public bool createWalletOnLogin;

    public string appId;
    public string mobileClientId;
    public string webClientId;

    [Header("Events")]

    public UnityEvent onInitialize;

    public UnityEvent onSuccess;
    public UnityEvent onFailure;

    public UnityEvent<string> onSendCodeCaughtError;

    public UnityEvent onError;

    void Awake()
    {
        var config = new PrivyConfig
        {
            AppId = appId,
#if UNITY_EDITOR
            ClientId = mobileClientId

#elif UNITY_WEBGL
                ClientId = webClientId
#endif
        };

        PrivyManager.Initialize(config);
        onInitialize?.Invoke();
    }

    struct PrivyExceptionHandlerData
    {
        public string identifier;
        public string message;
        public string type;
    }

    readonly PrivyExceptionHandlerData[] privySendCodeExceptionHandlersData = new PrivyExceptionHandlerData[]{
            new() {
                identifier = "has not been set as an allowed app identifier in the Privy dashboard",
                message = "Please add the app identifier to the Privy dashboard.",
                type = "LogWarning"
            },
            new() {
                identifier = "Invalid Privy app ID",
                message = "Invalid Privy app ID.",
                type = "LogWarning"
            },
            new() {
                identifier = "Invalid app client ID",
                message = "Invalid App Client ID.",
                type = "LogWarning"
            },
            new() {
                identifier = "Invalid email address",
                message = "Invalid Email Address.",
                type = "CaughtEvent"
            }
        };

    public async Task SendCode(string email)
    {
        try
        {
            bool success = await PrivyManager.Instance.Email.SendCode(email);

            if (success) onSuccess?.Invoke();
            else onFailure?.Invoke();
        }
        catch (Exception e)
        {
            bool isErrorAccountedFor = false;
            for (var i = 0; i < privySendCodeExceptionHandlersData.Length; i++)
            {
                if (e.Message.Contains(privySendCodeExceptionHandlersData[i].identifier))
                {
                    if (privySendCodeExceptionHandlersData[i].type == "LogWarning")
                        Debug.LogWarning(privySendCodeExceptionHandlersData[i].message);

                    if (privySendCodeExceptionHandlersData[i].type == "CaughtEvent")
                        onSendCodeCaughtError?.Invoke(privySendCodeExceptionHandlersData[i].message);

                    isErrorAccountedFor = true;
                    break;
                }
            }

            if (!isErrorAccountedFor)
            {
                Debug.LogError(e.Message);
            }

            onError?.Invoke();
        }
    }

    public UnityEvent<PrivyUser> onLoginWithCodeSuccess;
    public UnityEvent onLoginWithCodeFailure;
    public UnityEvent onLoginWithCodeError;
    public UnityEvent<string> onLoginWithCodeCaughtError;

    readonly PrivyExceptionHandlerData[] privyLoginWithCodeExceptionHandlersData = new PrivyExceptionHandlerData[]{
            new() {
                identifier = "Invalid email and code combination",
                message = "Invalid or expired verification code.",
                type = "CaughtEvent"
            }
        };

    public async Task LoginWithCode(string email, string code)
    {
        try
        {
            var result = await PrivyManager.Instance.Email.LoginWithCode(email, code);

            if (result == AuthState.Authenticated)
            {
                var privyUser = await PrivyManager.Instance.GetUser();

                if (createWalletOnLogin)
                {
                    if (privyUser.EmbeddedWallets.Length == 0)
                    {
                        await privyUser.CreateWallet();
                    }
                }

                onLoginWithCodeSuccess?.Invoke(privyUser);
            }
            else onLoginWithCodeFailure?.Invoke();
        }
        catch (Exception e)
        {
            bool isErrorAccountedFor = false;

            for (var i = 0; i < privyLoginWithCodeExceptionHandlersData.Length; i++)
            {
                if (e.Message.Contains(privyLoginWithCodeExceptionHandlersData[i].identifier))
                {
                    isErrorAccountedFor = true;

                    if (privyLoginWithCodeExceptionHandlersData[i].type == "LogWarning")
                        Debug.LogWarning(privyLoginWithCodeExceptionHandlersData[i].message);

                    if (privyLoginWithCodeExceptionHandlersData[i].type == "CaughtEvent")
                        onLoginWithCodeCaughtError?.Invoke(privyLoginWithCodeExceptionHandlersData[i].message);

                    break;
                }
            }

            if (!isErrorAccountedFor)
            {
                Debug.LogError(e.Message);
            }

            onLoginWithCodeError?.Invoke();
        }
    }

    public UnityEvent<string> onSign;
    public UnityEvent<string> onTransactionSent;
    public UnityEvent<string> onContractCallResult;

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

            onSign?.Invoke(personalSignResponse.Data);
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

    public async void Sign2(string to, string value, string chainId)
    {
        PrivyUser privyUser = await PrivyManager.Instance.GetUser();
        IEmbeddedEthereumWallet embeddedWallet = privyUser.EmbeddedWallets[0];

        string transactionJson = JsonUtility.ToJson(new
        {
            to, // "0xE3070d3e4309afA3bC9a6b057685743CF42da77C",
            value, // "0x186a0", // 100000 in hex
            chainId, // "0x2105", // 8453 (Base) in hex
            from = embeddedWallet.Address
        });

        var rpcRequest = new RpcRequest
        {
            Method = "personal_sign", //a supported method
            Params = new string[] { transactionJson } //an array of strings, with the message + address
        };

        try
        {
            //Now that the response has been constructed, we try to execute the request
            RpcResponse personalSignResponse = await embeddedWallet.RpcProvider.Request(rpcRequest);

            //If response is successful, we can parse out the data
            Debug.Log(personalSignResponse.Data);

            onSign?.Invoke(personalSignResponse.Data);
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

    /// <summary>
    /// Send a transaction to a smart contract
    /// </summary>
    /// <param name="to">Contract address</param>
    /// <param name="data">Encoded function call data</param>
    /// <param name="value">ETH value to send (in wei, as hex string)</param>
    /// <param name="gasLimit">Gas limit (optional)</param>
    public async void SendTransaction(string to, string data, string value = "0x0", string gasLimit = null)
    {
        try
        {
            PrivyUser privyUser = await PrivyManager.Instance.GetUser();
            IEmbeddedEthereumWallet embeddedWallet = privyUser.EmbeddedWallets[0];

            // Build transaction object
            string transactionJson;
            if (!string.IsNullOrEmpty(gasLimit))
            {
                transactionJson = $"{{\"from\":\"{embeddedWallet.Address}\",\"to\":\"{to}\",\"data\":\"{data}\",\"value\":\"{value}\",\"gas\":\"{gasLimit}\"}}";
            }
            else
            {
                transactionJson = $"{{\"from\":\"{embeddedWallet.Address}\",\"to\":\"{to}\",\"data\":\"{data}\",\"value\":\"{value}\"}}";
            }

            var rpcRequest = new RpcRequest
            {
                Method = "eth_sendTransaction",
                Params = new string[] { transactionJson }
            };

            RpcResponse response = await embeddedWallet.RpcProvider.Request(rpcRequest);
            Debug.Log($"Transaction sent: {response.Data}");
            onTransactionSent?.Invoke(response.Data);
        }
        catch (PrivyException.EmbeddedWalletException ex)
        {
            Debug.LogError($"Could not send transaction due to error: {ex.Error} {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Could not send transaction exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Call a smart contract function (read-only)
    /// </summary>
    /// <param name="to">Contract address</param>
    /// <param name="data">Encoded function call data</param>
    /// <param name="blockTag">Block tag (default: "latest")</param>
    public async void CallContract(string to, string data, string blockTag = "latest")
    {
        try
        {
            PrivyUser privyUser = await PrivyManager.Instance.GetUser();
            IEmbeddedEthereumWallet embeddedWallet = privyUser.EmbeddedWallets[0];

            string callParamsJson = $"{{\"to\":\"{to}\",\"data\":\"{data}\"}}";

            var rpcRequest = new RpcRequest
            {
                Method = "eth_call",
                Params = new string[] { callParamsJson, blockTag }
            };

            RpcResponse response = await embeddedWallet.RpcProvider.Request(rpcRequest);
            Debug.Log($"Contract call result: {response.Data}");
            onContractCallResult?.Invoke(response.Data);
        }
        catch (PrivyException.EmbeddedWalletException ex)
        {
            Debug.LogError($"Could not call contract due to error: {ex.Error} {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Could not call contract exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the current gas price
    /// </summary>
    public async Task<string> GetGasPrice()
    {
        try
        {
            PrivyUser privyUser = await PrivyManager.Instance.GetUser();
            IEmbeddedEthereumWallet embeddedWallet = privyUser.EmbeddedWallets[0];

            var rpcRequest = new RpcRequest
            {
                Method = "eth_gasPrice",
                Params = new string[] { }
            };

            RpcResponse response = await embeddedWallet.RpcProvider.Request(rpcRequest);
            return response.Data;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Could not get gas price: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get the current balance of the wallet
    /// </summary>
    public async Task<string> GetBalance()
    {
        try
        {
            Debug.Log("AYE1");
            PrivyUser privyUser = await PrivyManager.Instance.GetUser();
            Debug.Log("AYE2");
            IEmbeddedEthereumWallet embeddedWallet = privyUser.EmbeddedWallets[0];

            Debug.Log("AYE3");

            var rpcRequest = new RpcRequest
            {
                Method = "eth_getBalance",
                Params = new string[] { embeddedWallet.Address, "latest" }
            };
            Debug.Log("AYE4");


            RpcResponse response = await embeddedWallet.RpcProvider.Request(rpcRequest);
            Debug.Log("AYE5");

            return response.Data;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Could not get balance: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get the current chain ID of the wallet
    /// </summary>
    public async Task<string> GetCurrentChainId()
    {
        try
        {
            PrivyUser privyUser = await PrivyManager.Instance.GetUser();
            IEmbeddedEthereumWallet embeddedWallet = privyUser.EmbeddedWallets[0];
            return embeddedWallet.ChainId;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Could not get chain ID: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Validate that the wallet is on the expected chain before sending transaction
    /// </summary>
    /// <param name="expectedChainId">The expected chain ID (e.g., "0x1" for Ethereum mainnet, "0x89" for Polygon)</param>
    /// <returns>True if on correct chain, false otherwise</returns>
    public async Task<bool> ValidateChain(string expectedChainId)
    {
        try
        {
            string currentChainId = await GetCurrentChainId();
            bool isValid = currentChainId?.ToLower() == expectedChainId.ToLower();

            if (!isValid)
            {
                Debug.LogWarning($"Chain mismatch! Expected: {expectedChainId}, Current: {currentChainId}");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Could not validate chain: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Send a transaction with chain validation
    /// </summary>
    /// <param name="to">Contract address</param>
    /// <param name="data">Encoded function call data</param>
    /// <param name="value">ETH value to send (in wei, as hex string)</param>
    /// <param name="gasLimit">Gas limit (optional)</param>
    /// <param name="expectedChainId">Expected chain ID for validation (optional)</param>
    public async void SendTransactionWithChainValidation(string to, string data, string value = "0x0", string gasLimit = null, string expectedChainId = null)
    {
        try
        {
            // Validate chain if expected chain ID is provided
            if (!string.IsNullOrEmpty(expectedChainId))
            {
                bool isValidChain = await ValidateChain(expectedChainId);
                if (!isValidChain)
                {
                    Debug.LogError($"Transaction cancelled: Wallet is not on expected chain {expectedChainId}");
                    return;
                }
            }

            // Proceed with transaction
            SendTransaction(to, data, value, gasLimit);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Could not send transaction with chain validation: {ex.Message}");
        }
    }
}
