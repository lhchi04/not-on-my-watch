// BreathingCheckZone.cs - Attach to BreathingCheckZone GameObject
using UnityEngine;
using UnityEngine.Events;
using Unity.VRTemplate;

public class BreathingCheckZone : MonoBehaviour
{
    [Header("Detection")]
    public Transform ghostHeadMarker;           // Where player head should be
    public float detectionRadius = 0.2f;        // How close head needs to be
    public float requiredDuration = 8f;         // Seconds to hold position

    [Header("Ghost Head")]
    [SerializeField] private bool showGhostHead = true;

    [Header("References")]
    public GameObject ghostHeadVisual;          // The sphere/icon to hide
    public CountdownCircleUI countdownUI;       // The countdown display
    
    
    [Header("State")]
    private Transform playerHead;               // VR camera
    private bool isCheckingBreathing = false;
    private bool checkComplete = false;
    private float timeInPosition = 0f;

    [Header("Card Reference")]
    public StepManager stepManager;

    [Header("Game Events")]
    public UnityEvent onBreathingComplete;  // wire to GamePatient.NotifyStep2
    public UnityEvent onPlayerApproached;   // fires when head enters zone — wire to GamePatient.BeginHelping
    public UnityEvent onPlayerPulledAway;   // fires when head leaves before check completes — wire to GamePatient.StopHelping
    
    void Start()
    {
        // Find the VR camera (player's head)
        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
            playerHead = mainCamera.transform;
        else
            Debug.LogError("Can't find Main Camera!");
        
        if (!showGhostHead && ghostHeadVisual != null)
            ghostHeadVisual.SetActive(false);

        // Make sure countdown is hidden
        countdownUI.gameObject.SetActive(false);
        
       
    }
    
    void Update()
    {
        if (playerHead == null || checkComplete) return;
        
        // Calculate distance between player head and ghost head position
        float distance = Vector3.Distance(playerHead.position, ghostHeadMarker.position);
        
        if (distance < detectionRadius)
        {
            // Player head is in correct position
            if (!isCheckingBreathing)
            {
                // Just entered position - start check
                StartBreathingCheck();
            }
            
            // Increment timer
            timeInPosition += Time.deltaTime;
            
            // Update countdown UI
            float progress = timeInPosition / requiredDuration;
            countdownUI.UpdateProgress(progress);
            
            // Check if complete
            if (timeInPosition >= requiredDuration)
            {
                CompleteBreathingCheck();
            }
        }
        else
        {
            // Player head left position
            if (isCheckingBreathing)
            {
                // Reset if they move away
                CancelBreathingCheck();
            }
        }
    }
    
    void StartBreathingCheck()
    {
        Debug.Log("Player head in position - starting breathing check");
        isCheckingBreathing = true;
        timeInPosition = 0f;
        onPlayerApproached.Invoke();
        
        // Hide ghost head
        ghostHeadVisual.SetActive(false);
        
        // Show countdown UI
        countdownUI.gameObject.SetActive(true);
        countdownUI.StartCountdown();
      
    }
    
    void CancelBreathingCheck()
    {
        Debug.Log("Player moved head - breathing check cancelled");
        isCheckingBreathing = false;
        timeInPosition = 0f;
        onPlayerPulledAway.Invoke();
        
        // Show ghost head again
        if (showGhostHead)
            ghostHeadVisual.SetActive(true);
        
        // Hide countdown UI
        countdownUI.gameObject.SetActive(false);
      
    }
    
    void CompleteBreathingCheck()
    {
        if (checkComplete) return; // Prevent multiple calls
        
        Debug.Log("Breathing check complete!");
        checkComplete = true;
        isCheckingBreathing = false;

        // Keep countdown UI visible showing completion
        countdownUI.ShowComplete();
        
        
        
        // Trigger next scenario step
        // ScenarioManager.Instance.OnBreathingCheckComplete();
        ShowFeedback("Patient is not breathing");
        onBreathingComplete.Invoke();

        if (stepManager != null)
            stepManager.UnlockContinue();
    }
    
   
    void ShowFeedback(string message)
    {
        Debug.Log($"Feedback: {message}");
        // Show UI text or trigger next instruction
    }
    
    // Visualize detection zone in editor
    void OnDrawGizmos()
    {
        if (ghostHeadMarker != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(ghostHeadMarker.position, detectionRadius);
        }
    }
    
    // Public method to activate this check (called from scenario manager)
    public void ActivateBreathingCheck()
    {
        if (showGhostHead)
            ghostHeadVisual.SetActive(true);
        checkComplete = false;
        timeInPosition = 0f;
    }

    public void ResetCheck()
    {
        checkComplete = false;
        isCheckingBreathing = false;
        timeInPosition = 0f;
        if (countdownUI) countdownUI.gameObject.SetActive(false);
        if (showGhostHead && ghostHeadVisual != null)
            ghostHeadVisual.SetActive(true);
    }
}