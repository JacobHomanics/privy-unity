
using UnityEngine;
using Privy;

public class PrivyLogoutButton : MonoBehaviour
{
    public void Logout()
    {
        PrivyManager.Instance.Logout();
    }
}
