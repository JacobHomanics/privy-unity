using System;
using System.Collections;
using Privy;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EmailCodeHandler : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField codeInputField;
    public TMP_Text userWalletText;

    public UnityEvent onCodeEntered;

    void Update()
    {
        if (codeInputField.text.Length >= codeInputField.characterLimit)
        {
            this.enabled = false;
            onCodeEntered?.Invoke();
        }
    }

    public PrivyController controller;

    public UnityEvent onSuccess;
    public UnityEvent onFailure;
    public UnityEvent onError;

    string invalidEmailAndCodeCombinationException = "Invalid email and code combination";
    public async void LoginWithCode()
    {
        try
        {
            var result = await controller.LoginWithCodeAsync(emailInputField.text, codeInputField.text);

            if (result == AuthState.Authenticated)
            {
                var privyUser = await PrivyManager.Instance.GetUser();

                if (privyUser.EmbeddedWallets.Length == 0)
                {
                    await privyUser.CreateWallet();
                }

                userWalletText.text = privyUser.EmbeddedWallets[0].Address;
                StartCoroutine(ShowMessage("Succesfully logged in!", Color.green));
                StartCoroutine(DelaySuccess());
            }
            else onFailure?.Invoke();
        }
        catch (Exception e)
        {

            string userErrorMessage = "Invalid or expired verification code.";

            if (e.Message.Contains(invalidEmailAndCodeCombinationException))
            {
                Debug.LogWarning("Invalid Email and code combination.");
                StartCoroutine(ShowMessage(userErrorMessage, Color.red));
                StartCoroutine(DelayTextReset());
                StartCoroutine(DelayScriptEnable());
            }
            else
            {
                Debug.Log(e.Message);
            }

            // Debug.Log(e);
            onError?.Invoke();
        }
    }

    public TMP_Text messageText;

    private IEnumerator DelayScriptEnable()
    {
        yield return new WaitForSeconds(1f);
        this.enabled = true;
    }

    private IEnumerator DelayTextReset()
    {
        yield return new WaitForSeconds(1f);
        codeInputField.text = "";
    }

    private IEnumerator DelaySuccess()
    {
        yield return new WaitForSeconds(1f);
        onSuccess?.Invoke();
    }

    private IEnumerator ShowMessage(string message, Color color)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = message;
        messageText.color = color;
        yield return new WaitForSeconds(1f);
        messageText.gameObject.SetActive(false);
    }
}
