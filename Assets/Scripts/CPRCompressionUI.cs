// CPRCompressionUI.cs - Attach to Canvas in World Space
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CPRCompressionUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject compressionPanel;      // The whole UI panel
    public Slider depthSlider;              // Shows current compression depth
    public Image depthFillImage;            // Color changes based on depth
    public TextMeshProUGUI compressionCountText;
    public TextMeshProUGUI rateText;        // "120 CPM" (compressions per minute)
    public Image targetZone;                // Green zone showing ideal depth
    
    [Header("CPR Settings")]
    public float minDepth = 0f;             // No compression
    public float maxDepth = 0.1f;           // ~2.4 inches in meters
    public float targetMinDepth = 0.05f;    // ~2 inches
    public float targetMaxDepth = 0.06f;    // ~2.4 inches
    public int targetRate = 110;            // 100-120 CPM is ideal
    
    [Header("Tracking")]
    public Transform playerRightHand;
    private Vector3 initialHandPosition;
    private float currentDepth = 0f;
    private int compressionCount = 0;
    private float[] compressionTimes = new float[10]; // Track last 10 compressions
    private int compressionIndex = 0;
    
    private bool isActive = false;
    private bool isCompressing = false;
    
    void Start()
    {
        compressionPanel.SetActive(false);
        // playerRightHand = GameObject.Find("RightHand Controller").transform;
        
        // Setup slider
        depthSlider.minValue = minDepth;
        depthSlider.maxValue = maxDepth;
        
        // Position target zone on slider
        // PositionTargetZone();
    }
    
    void PositionTargetZone()
    {
        // Visual indicator of where compression should be
        RectTransform rt = targetZone.GetComponent<RectTransform>();
        float sliderHeight = depthSlider.GetComponent<RectTransform>().rect.height;
        
        float targetStart = (targetMinDepth / maxDepth) * sliderHeight;
        float targetEnd = (targetMaxDepth / maxDepth) * sliderHeight;
        
        rt.anchoredPosition = new Vector2(0, targetStart);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, targetEnd - targetStart);
    }
    
    public void ActivateCompressionMode()
    {
        isActive = true;
        compressionPanel.SetActive(true);
        initialHandPosition = playerRightHand.position;
        compressionCount = 0;
        UpdateUI();
    }
    
    public void DeactivateCompressionMode()
    {
        isActive = false;
        compressionPanel.SetActive(false);
    }
    
    void Update()
    {
        if (!isActive) return;
        
        // Calculate compression depth
        float verticalDisplacement = initialHandPosition.y - playerRightHand.position.y;
        currentDepth = Mathf.Clamp(verticalDisplacement, minDepth, maxDepth);
        
        // Detect compression (going down) and release (going up)
        DetectCompression();
        
        UpdateUI();
    }
    
    void DetectCompression()
    {
        // If depth crosses threshold, count as compression
        if (currentDepth > targetMinDepth && !isCompressing)
        {
            isCompressing = true;
            OnCompressionPerformed();
        }
        else if (currentDepth < targetMinDepth * 0.5f && isCompressing)
        {
            // Released - ready for next compression
            isCompressing = false;
        }
    }
    
    void OnCompressionPerformed()
    {
        compressionCount++;
        
        // Record time for rate calculation
        compressionTimes[compressionIndex] = Time.time;
        compressionIndex = (compressionIndex + 1) % compressionTimes.Length;
        
        // Haptic feedback
        // OVRInput.SetControllerVibration(0.3f, 0.3f, OVRInput.Controller.RTouch);
        
        // Check if compression was good depth
        if (currentDepth >= targetMinDepth && currentDepth <= targetMaxDepth)
        {
            // Good compression!
            ShowFeedback("Good!", Color.green);
        }
        else if (currentDepth < targetMinDepth)
        {
            ShowFeedback("Deeper!", Color.yellow);
        }
        else
        {
            ShowFeedback("Too deep!", Color.red);
        }
    }
    
    void UpdateUI()
    {
        // Update depth slider
        depthSlider.value = currentDepth;
        
        // Color based on depth quality
        if (currentDepth >= targetMinDepth && currentDepth <= targetMaxDepth)
        {
            depthFillImage.color = Color.green;
        }
        else if (currentDepth < targetMinDepth)
        {
            depthFillImage.color = Color.yellow;
        }
        else
        {
            depthFillImage.color = Color.red;
        }
        
        // Update compression count
        compressionCountText.text = $"Compressions: {compressionCount}";
        
        // Calculate and display rate
        float currentRate = CalculateCompressionRate();
        rateText.text = $"{currentRate:F0} CPM";
        
        // Color rate text
        if (currentRate >= 100 && currentRate <= 120)
        {
            rateText.color = Color.green;
        }
        else
        {
            rateText.color = Color.yellow;
        }
    }
    
    float CalculateCompressionRate()
    {
        if (compressionCount < 2) return 0f;
        
        // Calculate rate from last compressions
        float oldestTime = compressionTimes[compressionIndex];
        float newestTime = compressionTimes[(compressionIndex - 1 + compressionTimes.Length) % compressionTimes.Length];
        
        float timeDiff = newestTime - oldestTime;
        if (timeDiff <= 0) return 0f;
        
        int compressionsInWindow = Mathf.Min(compressionCount, compressionTimes.Length);
        float rate = (compressionsInWindow / timeDiff) * 60f; // Convert to per minute
        
        return rate;
    }
    
    void ShowFeedback(string message, Color color)
    {
        // Could add floating text feedback
        Debug.Log($"CPR Feedback: {message}");
    }
}
