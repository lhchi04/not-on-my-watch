// CountdownCircleUI.cs - Attach to Canvas
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownCircleUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image circleImage;                   // The circular fill image
    public TextMeshProUGUI countdownText;       // "8" counting down to "0"
    public TextMeshProUGUI instructionText;     // "Hold position"

    [Header("Colors")]
    public Color activeColor = Color.cyan;      // While counting
    public Color completeColor = Color.green;   // When finished

    private float totalDuration = 8f;
    private float currentTime = 0f;

    [Header("Audio")]
    public AudioSource popUpSound;

    void Start()
    {
        // Setup circle to fill radially
        if (circleImage != null)
        {
            circleImage.type = Image.Type.Filled;
            circleImage.fillMethod = Image.FillMethod.Radial360;
            circleImage.fillOrigin = (int)Image.Origin360.Top;
            circleImage.fillAmount = 0f;
            circleImage.color = activeColor;
        }
    }

    public void StartCountdown()
    {
        currentTime = 0f;
        circleImage.fillAmount = 0f;
        circleImage.color = activeColor;

        if (instructionText != null)
            instructionText.text = "Hold position...";

        if (popUpSound != null)
            popUpSound.Play();

        UpdateDisplay();
    }

    public void UpdateProgress(float progress)
    {
        Debug.Log($"UpdateProgress called! Progress: {progress}");

        // Progress is 0 to 1
        progress = Mathf.Clamp01(progress);

        if (circleImage != null)
            circleImage.fillAmount = progress;

        // Calculate remaining time
        currentTime = progress * totalDuration;
        float remaining = totalDuration - currentTime;

        if (countdownText != null)
            countdownText.text = Mathf.Ceil(remaining).ToString();
    }

    public void ShowComplete()
    {
        if (circleImage != null)
        {
            circleImage.fillAmount = 1f;
            circleImage.color = completeColor;
        }

        if (countdownText != null)
            countdownText.text = "";

        if (instructionText != null)
            instructionText.text = "Check complete!";

        // Hide after 2 seconds
        Invoke("HideUI", 2f);
    }

    void HideUI()
    {
        gameObject.SetActive(false);
    }

    void UpdateDisplay()
    {
        if (countdownText != null)
            countdownText.text = totalDuration.ToString();
    }
}
