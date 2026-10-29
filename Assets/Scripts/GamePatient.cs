using UnityEngine;
using UnityEngine.Events;

public class GamePatient : MonoBehaviour
{
    public enum PatientType { CPRAdult, CPRKid, WoundCare }
    public enum PatientState { Waiting, Distressed, BeingHelped, Healed, PassedOut }

    [Header("Setup")]
    [SerializeField] public PatientType patientType;
    [SerializeField] private PatientStateController patientStateController;

    [Header("Help Timer")]
    [SerializeField] private float helpTimerDuration = 240f;

    [Header("Distress Effects")]
    [SerializeField] private Light distressLight;
    [SerializeField] private AudioSource distressAudio;
    [SerializeField] private float lightPulseSpeed = 2f;
    [SerializeField] private float lightMaxIntensity = 2f;

    [Header("CPR Zone Gates (CPR patients only)")]
    [SerializeField] private GameObject breathingCheckZone;
    [SerializeField] private GameObject cprZone;

    [Header("Proximity")]
    [SerializeField] private string playerTag = "MainCamera";

    public bool IsPlayerNearby { get; private set; }

    [Header("Events")]
    public UnityEvent onDistressed;
    public UnityEvent onHealed;
    public UnityEvent onPassedOut;

    public PatientState State { get; private set; } = PatientState.Waiting;
    public bool IsResolved => State == PatientState.Healed || State == PatientState.PassedOut;
    public bool AllStepsComplete => nextExpectedStep > 3;
    public float HelpTimerRemaining { get; private set; }
    public string DisplayName => patientType switch
    {
        PatientType.CPRAdult => "Adult (CPR)",
        PatientType.CPRKid => "Child (CPR)",
        _ => "Wound Care"
    };

    private bool isBeingHelped;
    private float distressStartTime;
    private float responseTime = -1f;
    private WoundCareWalker walker;

    private const float ResponseWindow = 60f;
    private const float PenaltyInterval = 10f;
    private float responseElapsed = 0f;
    private float penaltyAccumulator = 0f;
    private bool playerReached = false;

    private const float StepPenaltyInterval = 5f;
    private float stepPenaltyAccumulator = 0f;
    private bool awaitingStepCompletion = false;

    private enum HintType { None, LateToReach, WrongStep }
    private HintType activeHint = HintType.None;

    private int nextExpectedStep = 1;

    void Awake()
    {
        walker = GetComponent<WoundCareWalker>();
        if (patientStateController == null)
            patientStateController = GetComponent<PatientStateController>();
    }

    void Start()
    {
        if (distressLight) distressLight.enabled = false;
        if (breathingCheckZone) breathingCheckZone.SetActive(false);
        if (cprZone) cprZone.SetActive(false);
    }

    void Update()
    {
        if (State == PatientState.Waiting || State == PatientState.Healed || State == PatientState.PassedOut)
            return;

        if (HelpTimerRemaining > 0f)
        {
            HelpTimerRemaining -= Time.deltaTime;
            if (HelpTimerRemaining <= 0f) OnTimerExpired();
        }

        TickResponsePenalty();
        TickStepPenalty();
        PulseLight();
    }

    void TickResponsePenalty()
    {
        if (playerReached)
        {
            if (activeHint == HintType.LateToReach)
            {
                GameHUD.Instance?.HideHint();
                activeHint = HintType.None;
            }
            return;
        }

        responseElapsed += Time.deltaTime;
        if (responseElapsed <= ResponseWindow) return;

        if (activeHint != HintType.WrongStep)
        {
            GameHUD.Instance?.ShowHint($"Late to reach {DisplayName}!\n-1 pt per 10 secs");
            activeHint = HintType.LateToReach;
        }

        penaltyAccumulator += Time.deltaTime;
        while (penaltyAccumulator >= PenaltyInterval)
        {
            GameFlowManager.Instance?.DeductPoints(1);
            penaltyAccumulator -= PenaltyInterval;
        }
    }

    void TickStepPenalty()
    {
        if (!awaitingStepCompletion) return;

        stepPenaltyAccumulator += Time.deltaTime;
        while (stepPenaltyAccumulator >= StepPenaltyInterval)
        {
            GameFlowManager.Instance?.DeductPoints(1);
            stepPenaltyAccumulator -= StepPenaltyInterval;
        }
    }

    public void TriggerDistress()
    {
        if (State != PatientState.Waiting) return;

        State = PatientState.Distressed;
        HelpTimerRemaining = helpTimerDuration;
        distressStartTime = Time.time;
        responseElapsed = 0f;
        penaltyAccumulator = 0f;
        playerReached = false;
        nextExpectedStep = 1;
        awaitingStepCompletion = false;
        stepPenaltyAccumulator = 0f;
        activeHint = HintType.None;

        if (distressLight) distressLight.enabled = true;
        if (distressAudio) distressAudio.Play();
        if (breathingCheckZone) breathingCheckZone.SetActive(true);
        if (cprZone) cprZone.SetActive(true);

        patientStateController?.Fall();
        walker?.StopWalking();

        onDistressed.Invoke();
        GameHUD.Instance?.ShowTimer(this);
    }

    public void NotifyStep1() => ReportStep(1);
    public void NotifyStep2() => ReportStep(2);
    public void NotifyStep3() => ReportStep(3);

    void ReportStep(int step)
    {
        if (IsResolved || State == PatientState.Waiting) return;
        if (!playerReached) return;

        if (step > nextExpectedStep)
        {
            awaitingStepCompletion = true;
            stepPenaltyAccumulator = 0f;
            GameHUD.Instance?.ShowHint(GetMissedStepHint(nextExpectedStep));
            activeHint = HintType.WrongStep;
        }
        else if (step == nextExpectedStep)
        {
            nextExpectedStep = step + 1;
            awaitingStepCompletion = false;
            stepPenaltyAccumulator = 0f;
            if (activeHint == HintType.WrongStep)
            {
                GameHUD.Instance?.HideHint();
                activeHint = HintType.None;
            }
        }
    }

    string GetMissedStepHint(int expectedStep)
    {
        if (patientType == PatientType.WoundCare)
        {
            return expectedStep switch
            {
                1 => "Put on gloves first!\n-1 pt per 5 secs",
                2 => "Clean the wound with spray first!\n-1 pt per 5 secs",
                _ => "Follow the correct steps!\n-1 pt per 5 secs"
            };
        }
        else
        {
            return expectedStep switch
            {
                1 => "Call 911 first!\n-1 pt per 5 secs",
                2 => "Check the patient's breathing first!\n-1 pt per 5 secs",
                _ => "Follow the correct steps!\n-1 pt per 5 secs"
            };
        }
    }

    public void BeginHelping() => SetBeingHelped(true);
    public void StopHelping() => SetBeingHelped(false);

    void SetBeingHelped(bool helping)
    {
        if (IsResolved) return;

        isBeingHelped = helping;

        if (helping)
        {
            State = PatientState.BeingHelped;
            playerReached = true;
            if (responseTime < 0f)
                responseTime = Time.time - distressStartTime;
        }
        else
        {
            if (State == PatientState.BeingHelped)
                State = PatientState.Distressed;
        }
    }

    public void OnAmbulanceArrived()
    {
        if (IsResolved) return;

        if (!AllStepsComplete)
        {
            GameFlowManager.Instance?.DeductPoints(25);
            OnTimerExpired();
            return;
        }

        OnTreatmentComplete();
    }

    public void OnTreatmentComplete()
    {
        if (State == PatientState.PassedOut || State == PatientState.Healed) return;

        State = PatientState.Healed;
        StopDistressEffects();
        patientStateController?.OnHealed();
        onHealed.Invoke();
        GameFlowManager.Instance?.ReportOutcome(this, PatientOutcome.Healed);
        GameHUD.Instance?.HideTimer();
        GameHUD.Instance?.HideHint();
        activeHint = HintType.None;
    }

    public void OnTimerExpired()
    {
        if (IsResolved) return;

        if (!AllStepsComplete)
            GameFlowManager.Instance?.DeductPoints(25);

        State = PatientState.PassedOut;
        StopDistressEffects();
        onPassedOut.Invoke();
        GameFlowManager.Instance?.ReportOutcome(this, PatientOutcome.PassedOut);
        GameHUD.Instance?.HideTimer();
        GameHUD.Instance?.HideHint();
        activeHint = HintType.None;
    }

    void StopDistressEffects()
    {
        if (distressLight) distressLight.enabled = false;
        if (distressAudio) distressAudio.Stop();
    }

    void PulseLight()
    {
        if (!distressLight) return;
        distressLight.intensity = Mathf.Abs(Mathf.Sin(Time.time * lightPulseSpeed)) * lightMaxIntensity;
    }

    public float GetResponseTime() => responseTime;

    public float ResponseTimeRemaining => playerReached ? -1f : Mathf.Max(0f, ResponseWindow - responseElapsed);

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        IsPlayerNearby = true;
        playerReached = true;

        if (patientType == PatientType.WoundCare && State == PatientState.Distressed)
            BeginHelping();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        IsPlayerNearby = false;
    }
}