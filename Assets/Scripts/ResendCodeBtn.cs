using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ResendCodeBtn : MonoBehaviour
{
    public float toggleTime = 5f;
    public PrivyController controller;
    public TMP_InputField emailText;

    public UnityEvent onToggleEnd;


    public async void Resend()
    {
        StartCoroutine(DelayToggle());
        await controller.SendCode(emailText.text);
    }

    private IEnumerator DelayToggle()
    {
        yield return new WaitForSeconds(toggleTime);
        onToggleEnd?.Invoke();
    }
}
