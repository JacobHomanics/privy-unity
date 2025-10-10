using UnityEngine;
using Privy;
using System.Threading.Tasks;
using CandyCoded.env;
using System;
using UnityEngine.Events;

public class PrivyController : MonoBehaviour
{
    [Header("Configuration")]
    public bool createWalletOnLogin;

    [Header("Events")]

    public UnityEvent onInitialize;

    public UnityEvent onSuccess;
    public UnityEvent onFailure;

    public UnityEvent<string> onSendCodeCaughtError;

    public UnityEvent onError;

    void Awake()
    {
        env.TryParseEnvironmentVariable("PRIVY_APP_ID", out string appId);
        env.TryParseEnvironmentVariable("PRIVY_WEB_CLIENT_ID", out string webClientId);
        env.TryParseEnvironmentVariable("PRIVY_MOBILE_CLIENT_ID", out string mobileClientId);

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
}
