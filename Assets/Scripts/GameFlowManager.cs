using UnityEngine;
using UnityEngine.Events;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Patients (in distress order)")]
    [SerializeField] private GamePatient cprAdult;
    [SerializeField] private GamePatient woundCare1;
    [SerializeField] private GamePatient cprKid;
    [SerializeField] private GamePatient woundCare2;

    [Header("Timing")]
    [SerializeField] private float distressWindowSize = 30f;
    [SerializeField] private float breakAfterDistress = 240f;
    [SerializeField] private float gameDuration = 1200f;

    [Header("Events")]
    public UnityEvent onGameEnd;

    public int Score { get; private set; } = 100;
    public float GameTimer { get; private set; }
    public bool GameRunning { get; private set; }
    public float GameDuration => gameDuration;

    private GamePatient[] patients;
    private float[] hurtTimes = new float[4];
    private bool[] scheduled = new bool[4];
    private bool[] triggered = new bool[4];

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        patients = new[] { cprAdult, woundCare1, cprKid, woundCare2 };

        hurtTimes[0] = Random.Range(0f, distressWindowSize);
        scheduled[0] = true;
    }

    void Start()
    {
        GameRunning = true;
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null) audio.Play();
    }

    void Update()
    {
        if (!GameRunning) return;

        GameTimer += Time.deltaTime;

        for (int i = 0; i < 4; i++)
        {
            if (scheduled[i] && !triggered[i] && GameTimer >= hurtTimes[i])
                TriggerPatient(i);
        }

        if (GameTimer >= gameDuration) EndGame();
    }

    void TriggerPatient(int index)
    {
        triggered[index] = true;

        int prev = index - 1;
        if (prev >= 0 && patients[prev] != null && !patients[prev].IsResolved)
            patients[prev].OnTimerExpired();

        if (patients[index] != null)
            patients[index].TriggerDistress();

        int next = index + 1;
        if (next < 4 && !scheduled[next])
        {
            float windowStart = GameTimer + breakAfterDistress;
            hurtTimes[next] = Random.Range(windowStart, windowStart + distressWindowSize);
            scheduled[next] = true;
        }
    }

    public void DeductPoints(int amount)
    {
        Score = Mathf.Max(0, Score - amount);
        if (Score <= 0) EndGame();
    }

    public void ReportOutcome(GamePatient patient, PatientOutcome outcome)
    {
        CheckAllResolved();
    }

    void CheckAllResolved()
    {
        foreach (var p in patients)
            if (p != null && !p.IsResolved) return;

        EndGame();
    }

    void EndGame()
    {
        if (!GameRunning) return;
        GameRunning = false;
        onGameEnd.Invoke();
        PerformanceSummaryManager.Instance?.ShowSummary();
    }
}

public enum PatientOutcome { Healed, PassedOut }