using UnityEngine;
using TMPro;

public class PhoneKeypad : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI displayText;
    public PhoneCallManager callManager;

    private string enteredNumber = "";

    public void PressKey(string digit)
    {
        if (enteredNumber.Length >= 11) return;
        enteredNumber += digit;
        displayText.text = enteredNumber;
    }

    public void PressDelete()
    {
        if (enteredNumber.Length == 0) return;
        enteredNumber = enteredNumber[..^1];
        displayText.text = enteredNumber;
    }

    public void PressCall()
    {
        callManager.TryCall(enteredNumber);
    }

    public void ResetKeypad()
    {
        enteredNumber = "";
        displayText.text = "";
    }
}