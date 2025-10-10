using UnityEngine;
using Privy;
using TMPro;
using System;

public class PrivyUserIdText : MonoBehaviour
{
    public TMP_Text text;

    public bool throwErrorOnInstanceInitialize;

    async void Update()
    {
        try
        {
            var user = await PrivyManager.Instance.GetUser();
            text.text = user.Id;
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
