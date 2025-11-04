using UnityEngine;
using UnityEngine.UI;
using Privy;
using System;

public class PrivyUserIdText : MonoBehaviour
{
    public Text text;

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
                Debug.LogError(e.Message);
            }
        }
    }
}
