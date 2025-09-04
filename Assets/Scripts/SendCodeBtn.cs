using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SendCodeBtn : MonoBehaviour
{
    public PrivyController controller;
    public TMP_InputField email;

    public UnityEvent onSuccess;
    public UnityEvent onFailure;
    public UnityEvent onError;

    string appIdentifierException = "has not been set as an allowed app identifier in the Privy dashboard";
    string invalidClientIdException = "Invalid app client ID";
    string invalidEmailAddressException = "Invalid email address";

    public async Task sendCode()
    {
        try
        {
            var result = await controller.SendCodeAsync(email.text);

            if (result) onSuccess?.Invoke();
            else onFailure?.Invoke();
        }
        catch (Exception e)
        {
            if (e.Message.Contains(invalidClientIdException))
            {
                Debug.LogWarning("Invalid App Client ID.");
            }

            else if (e.Message.Contains(appIdentifierException))
            {
                Debug.Log(Application.identifier);
                Debug.LogWarning("Please add the app identifier to the Privy dashboard.");
            }
            else if (e.Message.Contains(invalidEmailAddressException))
            {
                Debug.LogWarning("Invalid Email Address.");
            }
            else
            {
                Debug.Log(e.Message);
            }

            onError?.Invoke();
        }
    }

    public async void SendCode()
    {
        await sendCode();

        // try
        // {
        //     var result = await controller.SendCodeAsync(email.text);

        //     if (result) onSuccess?.Invoke();
        //     else onFailure?.Invoke();
        // }
        // catch (Exception e)
        // {
        //     if (e.Message.Contains(invalidClientIdException))
        //     {
        //         Debug.LogWarning("Invalid App Client ID.");
        //     }

        //     else if (e.Message.Contains(appIdentifierException))
        //     {
        //         Debug.LogWarning("Please add the app identifier to the Privy dashboard.");
        //     }
        //     else if (e.Message.Contains(invalidEmailAddressException))
        //     {
        //         Debug.LogWarning("Invalid Email Address.");
        //     }
        //     else
        //     {
        //         Debug.Log(e.Message);
        //     }

        //     onError?.Invoke();
        // }
    }
}
