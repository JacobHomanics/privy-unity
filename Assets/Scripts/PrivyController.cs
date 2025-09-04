using UnityEngine;
using Privy;
using System.Threading.Tasks;
using CandyCoded.env;
using System;
using UnityEngine.Events;

public class PrivyController : MonoBehaviour
{
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
        public string warning;
    }

    readonly PrivySendCodeExceptions[] privySendCodeExceptions = new PrivySendCodeExceptions[]{
        new() {
            identifier = "has not been set as an allowed app identifier in the Privy dashboard",
            warning = "Please add the app identifier to the Privy dashboard."
        },
        new() {
            identifier = "Invalid Privy app ID",
            warning = "Invalid Privy app ID."
        },
        new() {
            identifier = "Invalid app client ID",
            warning = "Invalid App Client ID."
        },
        new() {
            identifier = "Invalid email address",
            warning = "Invalid Email Address."
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
            for (var i = 0; i < privySendCodeExceptions.Length; i++)
            {
                if (e.Message.Contains(privySendCodeExceptions[i].identifier))
                {
                    Debug.LogWarning(privySendCodeExceptions[i].warning);
                    break;
                }
            }

            onError?.Invoke();
        }
    }

    public async Task<AuthState> LoginWithCodeAsync(string email, string code)
    {
        var result = await PrivyManager.Instance.Email.LoginWithCode(email, code);
        return result;
    }

    public async Task GetAuthState()
    {
        var authState = await PrivyManager.Instance.GetAuthState();

        switch (authState)
        {
            case AuthState.Authenticated:
                // User is authenticated. Grab the user's linked accounts
                var privyUser = await PrivyManager.Instance.GetUser();
                var linkedAccounts = privyUser.LinkedAccounts;
                break;
            case AuthState.Unauthenticated:
                // User is not authenticated.
                Debug.Log("User is currently logged out.");

                break;

        }
    }
}
