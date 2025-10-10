using System.Collections;
using Privy;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EmailCodeHandler : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new(1f);
    public TMP_InputField emailInputField;
    public TMP_InputField codeInputField;
    public TMP_Text userWalletText;

    public PrivyController controller;

    public TMP_Text messageText;

    public UnityEvent onCodeEntered;

    public UnityEvent onDelaySuccess;

    void Update()
    {
        if (codeInputField.text.Length >= codeInputField.characterLimit)
        {
            this.enabled = false;
            onCodeEntered?.Invoke();
        }
    }

    public async void LoginWithCode()
    {
        await controller.LoginWithCode(emailInputField.text, codeInputField.text);
    }

    public void OnSuccess(PrivyUser user)
    {
        userWalletText.text = user.EmbeddedWallets[0].Address;
        StartCoroutine(ShowMessage("Succesfully logged in!", Color.green));
        StartCoroutine(DelaySuccess());
    }

    public void OnCaughtError(string message)
    {
        StartCoroutine(ShowMessage(message, Color.red));
        StartCoroutine(DelayTextReset());
        StartCoroutine(DelayScriptEnable());
    }

    private IEnumerator DelayScriptEnable()
    {
        yield return _waitForSeconds1;
        this.enabled = true;
    }

    private IEnumerator DelayTextReset()
    {
        yield return _waitForSeconds1;
        codeInputField.text = "";
    }

    private IEnumerator DelaySuccess()
    {
        yield return _waitForSeconds1;
        onDelaySuccess?.Invoke();
    }

    private IEnumerator ShowMessage(string message, Color color)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = message;
        messageText.color = color;
        yield return _waitForSeconds1;
        messageText.gameObject.SetActive(false);
    }
}
