using UnityEngine;
using Privy;
using System;
using UnityEngine.UI;

public class PrivyUserWalletText : MonoBehaviour
{
    public Text text;
    public bool throwErrorOnInstanceInitialize;

    async void Update()
    {
        try
        {
            var user = await PrivyManager.Instance.GetUser();
            text.text = user.EmbeddedWallets[0].Address;
        }
        catch (Exception e)
        {
            if (e.Message == "Call PrivyManager.Initialize before attempting to get the Privy instance.")
            {
                if (throwErrorOnInstanceInitialize)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
    }
}
