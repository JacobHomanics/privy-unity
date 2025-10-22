using Privy;
using UnityEngine;
using UnityEngine.Events;

public class StartSessionHandler : MonoBehaviour
{
    public UnityEvent onAuthenticated;
    public UnityEvent onUnauthenticated;
    public UnityEvent onNotReady;

    public async void GetAuthState()
    {
        var authState = await PrivyManager.Instance.GetAuthState();

        if (authState == AuthState.Authenticated)
        {
            onAuthenticated?.Invoke();
        }

        if (authState == AuthState.Unauthenticated)
        {
            onUnauthenticated?.Invoke();
        }

        if (authState == AuthState.NotReady)
        {
            onNotReady?.Invoke();
        }
    }
}
