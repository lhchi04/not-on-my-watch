using UnityEngine;
using TMPro;

public class PerformanceSummaryManager : MonoBehaviour
{
    public static PerformanceSummaryManager Instance { get; private set; }

    [Header("Patients")]
    [SerializeField] private GamePatient cprAdult;
    [SerializeField] private GamePatient woundCare1;
    [SerializeField] private GamePatient cprKid;
    [SerializeField] private GamePatient woundCare2;

    [Header("UI")]
    [SerializeField] private GameObject summaryPanel;
    [SerializeField] private TextMeshProUGUI scoreLine;
    [SerializeField] private TextMeshProUGUI patientResultsText;
    [SerializeField] private TextMeshProUGUI feedbackLine;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        if (summaryPanel) summaryPanel.SetActive(false);
    }

    public void ShowSummary()
    {
        if (!summaryPanel) return;
        summaryPanel.SetActive(true);

        int score = GameFlowManager.Instance != null ? GameFlowManager.Instance.Score : 0;
        if (scoreLine) scoreLine.text = $"{score}";

        if (patientResultsText)
            patientResultsText.text = BuildPatientList();

        if (feedbackLine) feedbackLine.text = BuildFeedback();
    }

    string BuildPatientList()
    {
        string result = "";
        result += FormatLine(cprAdult);
        result += FormatLine(woundCare1);
        result += FormatLine(cprKid);
        result += FormatLine(woundCare2);
        return result.TrimEnd('\n');
    }

    string FormatLine(GamePatient p)
    {
        if (p == null) return "";
        bool healed = p.State == GamePatient.PatientState.Healed;
        string color   = healed ? "#2ecc71" : "#e74c3c";
        string icon    = healed ? "✓" : "✗";
        string outcome = healed ? "Helped" : "Passed out";
        float rt = p.GetResponseTime();
        string response = rt >= 0f ? $"Found in {Mathf.FloorToInt(rt)}s" : "Not reached";
        return $"<color={color}>{icon} {p.DisplayName}: {outcome}</color>\n<color=#888888>   {response}</color>\n";
    }

    string BuildFeedback()
    {
        foreach (var p in new[] { cprAdult, woundCare1, cprKid, woundCare2 })
        {
            if (p == null) continue;
            if (p.State == GamePatient.PatientState.PassedOut)
                return $"{p.DisplayName} was not reached in time. Try locating patients faster.";
        }

        float slowest = -1f;
        string slowestName = "";
        foreach (var p in new[] { cprAdult, woundCare1, cprKid, woundCare2 })
        {
            if (p == null) continue;
            float rt = p.GetResponseTime();
            if (rt > slowest) { slowest = rt; slowestName = p.DisplayName; }
        }

        if (slowest > 0f)
            return $"Good job! {slowestName} took {Mathf.FloorToInt(slowest)}s to reach — try to be faster next time.";

        return "Excellent — all patients helped in time!";
    }
}
