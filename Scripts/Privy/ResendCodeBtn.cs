using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ResendCodeBtn : MonoBehaviour
{
    public float toggleTime = 5f;
    public PrivyController controller;
    public InputField emailText;

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
