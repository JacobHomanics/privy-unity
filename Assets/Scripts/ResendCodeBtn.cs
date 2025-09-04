using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ResendCodeBtn : MonoBehaviour
{
    public float toggleTime = 5f;
    public SendCodeBtn sendCodeBtn;
    public UnityEvent onToggleEnd;


    public async void Resend()
    {
        StartCoroutine(DelayToggle());
        await sendCodeBtn.sendCode();
    }

    private IEnumerator DelayToggle()
    {
        yield return new WaitForSeconds(toggleTime);
        onToggleEnd?.Invoke();
    }
}
