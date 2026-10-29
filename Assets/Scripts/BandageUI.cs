// BandageUI.cs - Attach to BandageUI_Canvas
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using Unity.VRTemplate;

public class BandageUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject bandagePanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI feedbackText;

    [Header("Step Manager")]
    [SerializeField] private StepManager stepManager;
    public UnityEvent onBandageComplete;
    [SerializeField] private int myStepIndex = 3; // set if this UI is for a specific step that needs to unlock continue button

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color successColor = Color.green;
    public Color warningColor = Color.yellow;
    
    void Start()
    {
        if (bandagePanel != null)
            bandagePanel.SetActive(false);
        
        if (feedbackText != null)
            feedbackText.text = "";
    }
    
    public void StartBandaging()
    {
        Debug.Log("BandageUI: Starting bandaging");
        
        if (bandagePanel != null)
            bandagePanel.SetActive(true);
        
        if (titleText != null)
            titleText.text = "BANDAGING";
        
        if (instructionText != null)
        {
            instructionText.text = "Wrap bandage around arm in circular motion";
            instructionText.color = normalColor;
        }
        
        if (progressText != null)
        {
            progressText.text = "Wraps: 0/4";
            progressText.color = warningColor;
        }
        
        if (feedbackText != null)
            feedbackText.text = "";
    }
    
    public void UpdateProgress(int current, int total)
    {
        Debug.Log($"BandageUI: Progress {current}/{total}");
        
        if (progressText != null)
        {
            progressText.text = $"Wraps: {current}/{total}";
            
            if (current >= total)
                progressText.color = successColor;
            else if (current > 0)
                progressText.color = warningColor;
        }
    }
    
    public void ShowInstruction(string message)
    {
        Debug.Log($"BandageUI: Instruction - {message}");
        
        if (instructionText != null)
        {
            instructionText.text = message;
            instructionText.color = normalColor;
        }
    }
    
    public void ShowFeedback(string message, bool isSuccess = true)
    {
        Debug.Log($"BandageUI: Feedback - {message}");
        
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? successColor : warningColor;
            
            StopAllCoroutines();
            StartCoroutine(ClearFeedbackAfterDelay(2f));
        }
    }
    
    IEnumerator ClearFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackText != null)
            feedbackText.text = "";
    }
    
    public void ShowComplete()
    {
        Debug.Log("BandageUI: Bandaging complete!");
        
        if (titleText != null)
            titleText.text = "COMPLETE!";
        
        if (instructionText != null)
        {
            instructionText.text = "Bandaging finished successfully";
            instructionText.color = successColor;
        }
        
        if (progressText != null)
        {
            progressText.text = "4/4 Wraps Complete!";
            progressText.color = successColor;
        }
        
        if (feedbackText != null)
        {
            feedbackText.text = "Excellent work!";
            feedbackText.color = successColor;
        }

        onBandageComplete.Invoke();

        // Unlock continue button on correct step
        if (stepManager != null && stepManager.m_CurrentStepIndex == myStepIndex)            
            stepManager.UnlockContinue();

        StartCoroutine(HideAfterDelay(3f));
    }
    
    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bandagePanel != null)
            bandagePanel.SetActive(false);
    }
    
    public void HideUI()
    {
        if (bandagePanel != null)
            bandagePanel.SetActive(false);
    }
    
    public void ShowUI()
    {
        if (bandagePanel != null)
            bandagePanel.SetActive(true);
    }
}