using UnityEngine;
using UnityEngine.UI;
using TMPro; // if you're using TextMeshPro
using Unity.VRTemplate;

public class CPRFullUI : MonoBehaviour
{
    [Header("Tracker")]
    public CPRCompressionTracker tracker;

    [Header("Depth UI")]
    public Slider depthSlider;
    public Image depthFill;

    [Header("Rate UI")]
    public Slider rateSlider;
    public Image rateFill;
    public TextMeshProUGUI rateValueText;

    [Header("Text UI")]
    public TextMeshProUGUI countText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI rateText;

    [Header("Colors")]
    public Color goodColor = Color.green;
    public Color badColor = Color.red;
    public Color neutralColor = Color.yellow;

    [Header("Card Reference")]
    public StepManager stepManager;

    private void Start()
    {
        // Depth + slider + color
        tracker.OnCompressionChanged.AddListener(UpdateDepthUI);

        // Compression results
        tracker.OnGoodCompression.AddListener(OnGoodCompression);
        tracker.OnBadCompression.AddListener(OnBadCompression);

        // Rate feedback
        tracker.OnRateFeedback.AddListener(UpdateRateText);

        // Rate slider
        tracker.OnRateValueChanged.AddListener(UpdateRateSlider);

        // Completion
        tracker.OnComplete.AddListener(OnComplete);
        tracker.OnCycleComplete.AddListener(OnCycleComplete);

        // Initialize UI
        UpdateCount();
        feedbackText.text = "";
        rateText.text = "";
    }

    void UpdateDepthUI(float depth)
    {
        depthSlider.SetValueWithoutNotify(depth);

        if (depth >= tracker.greenZoneMin && depth <= tracker.greenZoneMax)
        {
            depthFill.color = goodColor;
        }
        else
        {
            depthFill.color = badColor;
        }
    }

    void UpdateRateSlider(float rate)
    {
        rateSlider.SetValueWithoutNotify(rate);

        if (rate >= tracker.minRate && rate <= tracker.maxRate)
        {
            rateFill.color = goodColor;
        }
        else
        {
            rateFill.color = badColor;
        }
        if (rateValueText != null)
        {
            rateValueText.text = Mathf.RoundToInt(rate).ToString();
        }
    }

    void OnGoodCompression()
    {
        feedbackText.text = "Good Compression";
        feedbackText.color = goodColor;

        UpdateCount();
    }

    void OnBadCompression()
    {
        feedbackText.text = "Bad Compression";
        feedbackText.color = badColor;
    }

    void UpdateRateText(string message)
    {
        rateText.text = message;

        if (message == "Good rhythm")
            rateText.color = goodColor;
        else
            rateText.color = badColor;
    }

    void OnCycleComplete()
    {
        feedbackText.text = "Keep going!";
        feedbackText.color = goodColor;
        UpdateCount();
    }

    void OnComplete()
    {
        feedbackText.text = "CPR Complete!";
        feedbackText.color = goodColor;
        if (stepManager != null)
            stepManager.UnlockContinue();
    }

    void UpdateCount()
    {
        countText.text = tracker.CompletedCompressions + " / " + tracker.targetCompressions;
    }

    public void ResetUI()
    {
        tracker.ResetAndStart();
        feedbackText.text = "";
        rateText.text = "";
        UpdateCount();
    }
}
