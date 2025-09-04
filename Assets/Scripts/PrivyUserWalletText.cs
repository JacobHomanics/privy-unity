
using UnityEngine;
using Privy;
using TMPro;

public class PrivyUserWalletText : MonoBehaviour
{
    public TMP_Text text;

    async void Update()
    {
        var user = await PrivyManager.Instance.GetUser();
        text.text = user.EmbeddedWallets[0].Address;
    }
}
