using UnityEngine;
using System.Collections;
using TMPro;

public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    [Header("Camera Reference")]
    [SerializeField] private Transform xrCamera;
    [SerializeField] private Vector3 hudLocalOffset = new Vector3(0f, 0.12f, 1.5f);

    [Header("Panels (drag the panel GameObjects)")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private GameObject ambulancePanel;

    [Header("Text inside Score Panel")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Text inside Timer Panel")]
    [SerializeField] private TextMeshProUGUI patientLabel;
    [SerializeField] private TextMeshProUGUI timerText;
    [Header("Text inside Countdown Panel")]
    [SerializeField] private TextMeshProUGUI gameTimerText;
    [Header("Text inside Ambulance Panel")]
    [SerializeField] private TextMeshProUGUI etaText;

    [Header("Hint Panel")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Urgency")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color urgentColor = Color.red;
    [SerializeField] private float urgentThreshold = 60f;

    private GamePatient trackedPatient;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (timerPanel) timerPanel.SetActive(false);
        if (ambulancePanel) ambulancePanel.SetActive(false);
        if (hintPanel) hintPanel.SetActive(false);
        UpdateScore();
    }

    void Update()
    {
        FollowCamera();
        UpdateScore();
        UpdateTimer();
        UpdateGameTimer();
    }

    void UpdateGameTimer()
    {
        if (!gameTimerText || GameFlowManager.Instance == null) return;

        float remaining = Mathf.Max(0f,
            GameFlowManager.Instance.GameDuration - GameFlowManager.Instance.GameTimer);

        int m = Mathf.FloorToInt(remaining / 60f);
        int s = Mathf.FloorToInt(remaining % 60f);

        gameTimerText.text = $"Time Left: {m}:{s:00}";
        gameTimerText.color = remaining < urgentThreshold ? urgentColor : normalColor;
    }

    void FollowCamera()
    {
        if (!xrCamera) return;
        transform.position = xrCamera.TransformPoint(hudLocalOffset);
        transform.rotation = xrCamera.rotation;
    }

    void UpdateScore()
    {
        if (!scoreText || GameFlowManager.Instance == null) return;
        scoreText.text = $"Score: {GameFlowManager.Instance.Score}";
    }

    void UpdateTimer()
    {
        if (trackedPatient == null) return;

        if (trackedPatient.IsResolved)
        {
            HideTimer();
            return;
        }

        float responseRemaining = trackedPatient.ResponseTimeRemaining;
        float remaining;

        string instruction;
        if (responseRemaining >= 0f)
        {
            remaining = responseRemaining;
            instruction = "Reach them!";
        }
        else
        {
            remaining = Mathf.Max(0f, trackedPatient.HelpTimerRemaining);
            instruction = "Treat them!";
        }

        if (patientLabel) patientLabel.text = $"! {trackedPatient.DisplayName} needs help!";

        int m = Mathf.FloorToInt(remaining / 60f);
        int s = Mathf.FloorToInt(remaining % 60f);

        if (timerText)
        {
            timerText.text = $"{instruction}\n{m}:{s:00}";
            timerText.color = remaining < urgentThreshold ? urgentColor : normalColor;
        }
    }

    public void ShowTimer(GamePatient patient)
    {
        trackedPatient = patient;
        if (patientLabel) patientLabel.text = $"! {patient.DisplayName} needs help!";
        if (timerPanel) timerPanel.SetActive(true);
    }

    public void HideTimer()
    {
        trackedPatient = null;
        if (timerPanel) timerPanel.SetActive(false);
    }

    // Called by AmbulanceSystem to show/hide the ambulance ETA panel
    public void ShowAmbulanceETA(string text)
    {
        if (ambulancePanel) ambulancePanel.SetActive(true);
        if (etaText) etaText.text = text;
    }

    public void HideAmbulanceETA()
    {
        if (ambulancePanel) ambulancePanel.SetActive(false);
    }

    public void ShowHint(string message, float autoDismissDuration = 0f)
    {
        if (!hintPanel || !hintText) return;
        StopCoroutine("AutoHideHint");
        hintText.text = message;
        hintPanel.SetActive(true);
        if (autoDismissDuration > 0f)
            StartCoroutine("AutoHideHint", autoDismissDuration);
    }

    public void HideHint()
    {
        StopCoroutine("AutoHideHint");
        if (hintPanel) hintPanel.SetActive(false);
    }

    IEnumerator AutoHideHint(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (hintPanel) hintPanel.SetActive(false);
    }
}
