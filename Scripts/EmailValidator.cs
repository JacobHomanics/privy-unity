using UnityEngine;
using UnityEngine.UI;

public class EmailValidator : MonoBehaviour
{
    public Text submitText;
    public Color validColor;
    public Color invalidColor;

    public Text text;

    void Update()
    {
        bool result = IsValidEmail(text.text);
        var color = result ? validColor : invalidColor;
        submitText.color = color;
    }

    bool IsValidEmail(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith("."))
        {
            return false; // suggested by @TK-421
        }
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }
    }
}
