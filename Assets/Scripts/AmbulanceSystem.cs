using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class AmbulanceSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PhoneCallManager phoneCallManager;
    [SerializeField] private GamePatient cprAdult;
    [SerializeField] private GamePatient cprKid;

    [Header("Arrival Timing (seconds)")]
    [SerializeField] private float minArrivalTime = 90f;
    [SerializeField] private float maxArrivalTime = 180f;

    [Header("Audio")]
    [SerializeField] private AudioSource ambulanceSiren;

    public UnityEvent onAmbulanceArrived;

    // True only while a countdown is actively running — resets automatically when ambulance arrives
    private bool countdownActive;
    private GamePatient activeCPRPatient;

    void OnEnable()  => phoneCallManager.onValidCallMade.AddListener(OnCallMade);
    void OnDisable() => phoneCallManager.onValidCallMade.RemoveListener(OnCallMade);

    void OnCallMade()
    {
        if (countdownActive) return;

        // Find whichever CPR patient is currently active (distressed or being helped)
        GamePatient target = null;
        if (cprAdult != null && !cprAdult.IsResolved
            && cprAdult.State != GamePatient.PatientState.Waiting)
            target = cprAdult;
        else if (cprKid != null && !cprKid.IsResolved
            && cprKid.State != GamePatient.PatientState.Waiting)
            target = cprKid;

        if (target == null) return;
        if (!target.IsPlayerNearby) return;

        activeCPRPatient = target;
        countdownActive = true;
        StartCoroutine(AmbulanceCountdown());
    }

    IEnumerator AmbulanceCountdown()
    {
        float countdown = Random.Range(minArrivalTime, maxArrivalTime);

        while (countdown > 0f)
        {
            countdown -= Time.deltaTime;
            int m = Mathf.FloorToInt(countdown / 60f);
            int s = Mathf.FloorToInt(countdown % 60f);
            GameHUD.Instance?.ShowAmbulanceETA($"Ambulance arriving in {m}:{s:00}");
            yield return null;
        }

        AmbulanceArrived();
    }

    void AmbulanceArrived()
    {
        countdownActive = false;
        GameHUD.Instance?.HideAmbulanceETA();
        if (ambulanceSiren) ambulanceSiren.Play();

        // Capture BEFORE OnTreatmentComplete — that method calls HideHint() internally
        bool incomplete = activeCPRPatient != null && !activeCPRPatient.AllStepsComplete;
        if (incomplete)
            GameFlowManager.Instance?.DeductPoints(25);

        activeCPRPatient?.OnTreatmentComplete();
        onAmbulanceArrived.Invoke();
        activeCPRPatient = null;

        // Show AFTER OnTreatmentComplete so HideHint() can't cancel it
        if (incomplete)
            GameHUD.Instance?.ShowHint("Did not finish all steps\n(-25 pts)", 10f);
    }
}
