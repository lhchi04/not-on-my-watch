using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Tracks controller Y-position to measure CPR compression depth and count valid compressions
/// </summary>
public class CPRCompressionTracker : MonoBehaviour
{
    [Header("Hand References")]
    [Tooltip("The left hand controller transform")]
    public Transform leftHand = null;

    [Tooltip("The right hand controller transform")]
    public Transform rightHand = null;

    [Header("Compression Settings")]
    [Tooltip("Maximum compression distance in world units (0.06 = 6 cm)")]
    public float maxCompressionDistance = 0.06f;

    [Tooltip("Normalized depth range for a correct compression (green zone)")]
    [Range(0, 1)] public float greenZoneMin = 0.7f;
    [Range(0, 1)] public float greenZoneMax = 1.0f;

    [Tooltip("Normalized depth the hands must return above to count as a release")]
    [Range(0, 1)] public float releaseThreshold = 0.3f;

    [Tooltip("Number of good compressions needed to complete CPR")]
    public int targetCompressions = 30;

    [Tooltip("If true, resets count each cycle instead of firing OnComplete — used in game mode where ambulance determines end")]
    public bool continuousMode = false;

    [Header("Rate Feedback")]
    [Tooltip("Minimum compressions per minute (real CPR guideline: 100)")]
    public float minRate = 100.0f;

    [Tooltip("Maximum compressions per minute (real CPR guideline: 120)")]
    public float maxRate = 120.0f;

    [Serializable] public class FloatEvent : UnityEvent<float> { }
    [Serializable] public class StringEvent : UnityEvent<string> { }

    // Fires each frame with the normalized compression depth (0 to 1)
    public FloatEvent OnCompressionChanged = new FloatEvent();

    // NEW: event for BPM value
    public FloatEvent OnRateValueChanged = new FloatEvent();

    // Fires when a single good compression cycle completes
    public UnityEvent OnGoodCompression = new UnityEvent();

    // Fires when a compression is too shallow or too deep
    public UnityEvent OnBadCompression = new UnityEvent();

    // Fires with rate feedback text ("Good rhythm", "Too fast", "Too slow")
    public StringEvent OnRateFeedback = new StringEvent();

    // Fires when the target number of compressions is reached (non-continuous mode)
    public UnityEvent OnComplete = new UnityEvent();

    // Fires each time a full cycle of compressions completes in continuous mode
    public UnityEvent OnCycleComplete = new UnityEvent();

    // Fires the first time StartTracking is called — use this to notify step 3 (CPR started)
    public UnityEvent onTrackingStarted = new UnityEvent();

    public int CompletedCompressions { get; private set; } = 0;
    public float CurrentDepth { get; private set; } = 0.0f;

    private float restingY = 0.0f;
    private bool isTracking = false;
    private bool inCompression = false;
    private float lastCompressionTime = 0.0f;

    public void StartTracking()
    {
        if (leftHand == null || rightHand == null)
            return;

        restingY = AverageHandY();
        CompletedCompressions = 0;
        inCompression = false;
        lastCompressionTime = 0.0f;
        isTracking = true;
        onTrackingStarted.Invoke();
    }

    public void StopTracking()
    {
        isTracking = false;
        CurrentDepth = 0.0f;
        OnCompressionChanged.Invoke(0.0f);
    }

    public void ResetCount()
    {
        CompletedCompressions = 0;
    }

    private void Update()
    {
        if (!isTracking)
            return;

        // Calculate how far down the hands have moved from the resting position
        float currentY = AverageHandY();
        float rawDepth = restingY - currentY;
        CurrentDepth = Mathf.Clamp01(rawDepth / maxCompressionDistance);

        OnCompressionChanged.Invoke(CurrentDepth);

        // Detect compression cycles
        bool inGreenZone = CurrentDepth >= greenZoneMin && CurrentDepth <= greenZoneMax;
        bool tooDeep = CurrentDepth > greenZoneMax;
        bool released = CurrentDepth < releaseThreshold;

        // Hand pushed into or past the green zone — start a compression
        if ((inGreenZone || tooDeep) && !inCompression)
        {
            inCompression = true;
        }

        // Hand released — end of compression cycle
        if (released && inCompression)
        {
            inCompression = false;
            RegisterCompression(inGreenZone: !tooDeep);
        }
    }

    private void RegisterCompression(bool inGreenZone)
    {
        if (inGreenZone)
        {
            CompletedCompressions++;
            OnGoodCompression.Invoke();
            EvaluateRate();

            if (CompletedCompressions >= targetCompressions)
            {
                if (continuousMode)
                {
                    OnCycleComplete.Invoke();
                    CompletedCompressions = 0;
                }
                else
                {
                    OnComplete.Invoke();
                }
            }
        }
        else
        {
            OnBadCompression.Invoke();
        }
    }

    private void EvaluateRate()
    {
        if (lastCompressionTime <= 0.0f)
        {
            lastCompressionTime = Time.time;
            return;
        }

        float interval = Time.time - lastCompressionTime;
        lastCompressionTime = Time.time;

        // Convert interval to compressions per minute
        float rate = 60.0f / interval;
        OnRateValueChanged.Invoke(rate);

        if (rate < minRate)
            OnRateFeedback.Invoke("Too slow");
        else if (rate > maxRate)
            OnRateFeedback.Invoke("Too fast");
        else
            OnRateFeedback.Invoke("Good rhythm");
    }

    private float AverageHandY()
    {
        return (leftHand.position.y + rightHand.position.y) * 0.5f;
    }

    public void ResetAndStart()
    {
        CompletedCompressions = 0;
        inCompression = false;
        lastCompressionTime = 0f;
        StartTracking();
    }
}
