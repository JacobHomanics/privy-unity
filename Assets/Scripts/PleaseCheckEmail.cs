using TMPro;
using UnityEngine;

public class PleaseCheckEmail : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_Text emailText;


    void Update()
    {
        emailText.text = "Please check <b>" + emailInputField.text + "</b> for an email from privy.io and enter your code below.";
    }
}
