using UnityEngine;
using Privy;
using System.Threading.Tasks;
using System.Collections;

public class PrivyController : MonoBehaviour
{
    public TextAsset environmentVariables;

    public string appId;

    public string webClientId;
    public string mobileClientId;


    void Start()
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

    public string clientId;

}
