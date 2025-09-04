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

    public UnityEvent onSuccess;
    public UnityEvent onFailure;
    public UnityEvent onError;

    void Start()
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
    }

    struct PrivySendCodeExceptions
    {
        public string identifier;
        public string message;
    }

    readonly PrivySendCodeExceptions[] privySendCodeExceptions = new PrivySendCodeExceptions[]{
        new() {
            identifier = "has not been set as an allowed app identifier in the Privy dashboard",
            message = "Please add the app identifier to the Privy dashboard."
        },
        new() {
            identifier = "Invalid Privy app ID",
            message = "Invalid Privy app ID."
        },
        new() {
            identifier = "Invalid app client ID",
            message = "Invalid App Client ID."
        },
        new() {
            identifier = "Invalid email address",
            message = "Invalid Email Address."
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
            for (var i = 0; i < privySendCodeExceptions.Length; i++)
            {
                if (e.Message.Contains(privySendCodeExceptions[i].identifier))
                {
                    Debug.LogWarning(privySendCodeExceptions[i].message);
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

    struct PrivyLoginWithCodeExceptions
    {
        public string identifier;
        public string message;
    }

    readonly PrivyLoginWithCodeExceptions[] privyLoginWithCodeExceptions = new PrivyLoginWithCodeExceptions[]{
        new() {
            identifier = "Invalid email and code combination",
            message = "Invalid or expired verification code."
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

            for (var i = 0; i < privyLoginWithCodeExceptions.Length; i++)
            {
                if (e.Message.Contains(privyLoginWithCodeExceptions[i].identifier))
                {
                    isErrorAccountedFor = true;
                    onLoginWithCodeCaughtError?.Invoke(privyLoginWithCodeExceptions[i].message);
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
}
