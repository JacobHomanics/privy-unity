using UnityEngine.UI;
using UnityEngine;

public class SendCodeBtn : MonoBehaviour
{
    public PrivyController controller;

    public async void SendCode(InputField email)
    {
        await controller.SendCode(email.text);
    }
}
