using UnityEngine;
using Privy;
using System.Threading.Tasks;
using CandyCoded.env;

public class PrivyController : MonoBehaviour
{
    private string appId;

    private string webClientId;
    private string mobileClientId;

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

    public async Awaitable<bool> SendCodeAwaitable(string email)
    {
        bool success = await PrivyManager.Instance.Email.SendCode(email);
        return success;
    }

    public async Task<bool> SendCodeAsync(string email)
    {
        bool success = await PrivyManager.Instance.Email.SendCode(email);
        return success;
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
