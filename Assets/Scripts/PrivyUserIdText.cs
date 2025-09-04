
using UnityEngine;
using Privy;
using TMPro;

public class PrivyUserIdText : MonoBehaviour
{
    public TMP_Text text;

    async void Update()
    {
        var user = await PrivyManager.Instance.GetUser();
        text.text = user.Id;
    }
}
