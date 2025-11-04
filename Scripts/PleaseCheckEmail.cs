using UnityEngine;
using UnityEngine.UI;

public class PleaseCheckEmail : MonoBehaviour
{
    public InputField emailInputField;
    public Text emailText;


    void Update()
    {
        emailText.text = "Please check <b>" + emailInputField.text + "</b> for an email from privy.io and enter your code below.";
    }
}
