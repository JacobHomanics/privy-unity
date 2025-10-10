using TMPro;
using UnityEngine;

public class SendCodeBtn : MonoBehaviour
{
    public PrivyController controller;

    public async void SendCode(TMP_InputField email)
    {
        await controller.SendCode(email.text);
    }
}
