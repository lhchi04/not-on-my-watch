// BandageCircularWrapper.cs - FIXED VERSION (Trail stays on arm)
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class BandageCircularWrapper : MonoBehaviour
{
    [Header("Limb References")]
    public Transform limbCenter;
    public float limbRadius = 0.08f;
    public float wrapDetectionRadius = 0.15f;
    
    [Header("Bandage")]
    public Transform bandageRoll;
    public LineRenderer bandageTrail;
    
    [Header("Wrap Settings")]
    public int requiredWraps = 4;
    public float minWrapHeight = 0.05f;
    public float angleThreshold = 350f;
    
    [Header("Trail Settings")]
    public float minPointDistance = 0.01f;  // NEW: Minimum 1cm between points
    
    [Header("Visual Feedback")]
    public Material inRangeMaterial;        // NEW: Green when in range
    public Material outOfRangeMaterial;     // NEW: Red when out of range
    public Renderer bandageRenderer;        // NEW: Bandage roll renderer
    
    [Header("State")]
    private bool isWrapping = false;
    private bool isComplete = false;
    private int completedWraps = 0;
    private float currentWrapHeight = 0f;

    public bool IsComplete => isComplete;
    
    [Header("Angle Tracking")]
    private Vector3 lastDirection;
    private float accumulatedAngle = 0f;
    private bool hasStartedWrap = false;
    private Vector3 lastTrailPoint = Vector3.zero;  // NEW: Track last point
    
    [Header("References (Optional)")]
    public BandageUI bandageUI;

    [Header("Audio")]
    public AudioSource wrapSound;

    [Header("Events")]
    public UnityEvent onBandagingComplete;
    
    void Start()
    {
        if (bandageTrail != null && limbCenter != null)
        {
            bandageTrail.transform.SetParent(limbCenter);
            bandageTrail.transform.localPosition = Vector3.zero;
            bandageTrail.transform.localRotation = Quaternion.identity;
            bandageTrail.useWorldSpace = false;
            bandageTrail.positionCount = 0;
            bandageTrail.enabled = false;
        }
    }
    
    void Update()
    {
        if (!isWrapping) return;
        
        // Check if bandage is close enough to limb
        bool isNear = IsBandageNearLimb();
        
        // NEW: Visual feedback on bandage color
        UpdateBandageVisual(isNear);
        
        if (isNear)
        {
            TrackWrapProgress();
        }
    }
    
    void UpdateBandageVisual(bool isInRange)
    {
        if (bandageRenderer == null) return;
        
        if (isInRange && inRangeMaterial != null)
        {
            bandageRenderer.material = inRangeMaterial;
        }
        else if (!isInRange && outOfRangeMaterial != null)
        {
            bandageRenderer.material = outOfRangeMaterial;
        }
    }
    
    bool IsBandageNearLimb()
    {
        if (bandageRoll == null || limbCenter == null) return false;
        return IsNearLimb(bandageRoll.position);
    }

    public bool IsNearLimb(Vector3 position)
    {
        if (limbCenter == null) return false;
        Vector3 closest = GetClosestPointOnLimbAxis(position);
        return Vector3.Distance(position, closest) < wrapDetectionRadius;
    }
    
    Vector3 GetClosestPointOnLimbAxis(Vector3 point)
    {
        Vector3 limbDirection = limbCenter.up;
        Vector3 pointToLimbCenter = point - limbCenter.position;
        float projection = Vector3.Dot(pointToLimbCenter, limbDirection);
        Vector3 closestPoint = limbCenter.position + limbDirection * projection;
        
        return closestPoint;
    }
    
    void TrackWrapProgress()
    {
        Vector3 bandagePos = bandageRoll.position;
        
        // NEW: Snap to surface for accurate tracking
        Vector3 surfacePos = SnapToCylinderSurface(bandagePos);
        
        // Add point to trail (only on surface)
        AddPointToTrail(surfacePos);
        
        // Calculate angle using surface position
        float currentAngle = CalculateAngleAroundLimb(surfacePos);
        
        if (!hasStartedWrap)
        {
            StartNewWrap(surfacePos, currentAngle);
        }
        else
        {
            UpdateWrapAngle(currentAngle);
        }
    }
    
    void StartNewWrap(Vector3 position, float startAngle)
    {
        Debug.Log($"Starting wrap #{completedWraps + 1}");
        hasStartedWrap = true;
        currentWrapHeight = position.y;
        accumulatedAngle = 0f;
        lastDirection = GetDirectionAroundLimb(position);
        
        if (bandageUI != null)
            bandageUI.ShowInstruction($"Wrap {completedWraps + 1}/{requiredWraps} - Keep wrapping in circle");
    }
    
    void UpdateWrapAngle(float currentAngle)
    {
        Vector3 currentDirection = GetDirectionAroundLimb(bandageRoll.position);
        float angleDelta = Vector3.SignedAngle(lastDirection, currentDirection, limbCenter.up);
        
        // Count both directions
        if (Mathf.Abs(angleDelta) < 180f)
        {
            accumulatedAngle += Mathf.Abs(angleDelta);
        }
        
        lastDirection = currentDirection;
        
        if (accumulatedAngle >= angleThreshold)
        {
            OnWrapCompleted();
        }
    }
    
    float CalculateAngleAroundLimb(Vector3 position)
    {
        Vector3 limbToBandage = position - limbCenter.position;
        limbToBandage.y = 0;
        limbToBandage.Normalize();
        
        float angle = Mathf.Atan2(limbToBandage.z, limbToBandage.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        
        return angle;
    }
    
    Vector3 GetDirectionAroundLimb(Vector3 position)
    {
        Vector3 radialDirection = position - limbCenter.position;
        radialDirection.y = 0;
        radialDirection.Normalize();
        
        Vector3 tangentDirection = Vector3.Cross(limbCenter.up, radialDirection);
        return tangentDirection.normalized;
    }
    
    void OnWrapCompleted()
    {
        completedWraps++;
        hasStartedWrap = false;
        accumulatedAngle = 0f;
        
        Debug.Log($"✓ Wrap {completedWraps}/{requiredWraps} completed!");
        
        if (bandageUI != null)
        {
            bandageUI.UpdateProgress(completedWraps, requiredWraps);
            bandageUI.ShowFeedback($"✓ Wrap {completedWraps} complete!");
        }
        
        if (wrapSound != null)
            wrapSound.Play();
        
        if (completedWraps >= requiredWraps)
        {
            CompleteBandaging();
        }
        else
        {
            currentWrapHeight += minWrapHeight;
        }
    }
    
    void AddPointToTrail(Vector3 point)
    {
        if (bandageTrail == null) return;
        
        // FIXED: Only add when near arm
        if (!IsBandageNearLimb()) return;
        
        // Snap to arm surface
        Vector3 snappedPoint = SnapToCylinderSurface(point);
        
        // FIXED: Check minimum distance from last point
        if (bandageTrail.positionCount > 0)
        {
            float distanceFromLast = Vector3.Distance(snappedPoint, lastTrailPoint);
            if (distanceFromLast < minPointDistance)
            {
                return; // Too close, skip this point
            }
        }
        
        // Add point in local space so trail moves with the limb
        Vector3 localPoint = bandageTrail.transform.InverseTransformPoint(snappedPoint);
        int currentPoints = bandageTrail.positionCount;
        bandageTrail.positionCount = currentPoints + 1;
        bandageTrail.SetPosition(currentPoints, localPoint);

        lastTrailPoint = localPoint;
        
        // Memory optimization
        if (bandageTrail.positionCount > 500)
        {
            List<Vector3> positions = new List<Vector3>();
            for (int i = 100; i < bandageTrail.positionCount; i++)
            {
                positions.Add(bandageTrail.GetPosition(i));
            }
            
            bandageTrail.positionCount = positions.Count;
            bandageTrail.SetPositions(positions.ToArray());
            
            if (bandageTrail.positionCount > 0)
                lastTrailPoint = bandageTrail.GetPosition(bandageTrail.positionCount - 1);
        }
    }
    
    Vector3 SnapToCylinderSurface(Vector3 point)
    {
        if (limbCenter == null) return point;
        
        // Get height along limb axis
        Vector3 limbDirection = limbCenter.up;
        Vector3 pointToLimbCenter = point - limbCenter.position;
        float heightAlongLimb = Vector3.Dot(pointToLimbCenter, limbDirection);
        
        // Base point on axis at this height
        Vector3 basePoint = limbCenter.position + limbDirection * heightAlongLimb;
        
        // Radial direction from axis to point
        Vector3 radialDirection = point - basePoint;
        radialDirection = Vector3.ProjectOnPlane(radialDirection, limbDirection);
        
        // Place on surface at limb radius
        if (radialDirection.magnitude > 0.001f)
        {
            radialDirection.Normalize();
            return basePoint + radialDirection * limbRadius;
        }
        
        // Fallback if on axis
        Vector3 fallbackDirection = Vector3.ProjectOnPlane(Vector3.right, limbDirection).normalized;
        return basePoint + fallbackDirection * limbRadius;
    }
    
    void CompleteBandaging()
    {
        Debug.Log("🎉 BANDAGING COMPLETE!");
        isWrapping = false;
        isComplete = true;

        if (bandageUI != null)
            bandageUI.ShowComplete();

        onBandagingComplete?.Invoke();
    }
    
    // PUBLIC METHODS
    
    public void ActivateBandaging()
    {
        Debug.Log("Bandaging mode activated");
        isWrapping = true;
        completedWraps = 0;
        hasStartedWrap = false;
        accumulatedAngle = 0f;
        lastTrailPoint = Vector3.zero;
        
        if (bandageTrail != null)
        {
            bandageTrail.enabled = true;
            bandageTrail.positionCount = 0;
        }
        
        if (bandageUI != null)
        {
            bandageUI.StartBandaging();
            bandageUI.ShowInstruction("Wrap bandage around the limb in circular motion");
        }
    }
    
    public void DeactivateBandaging()
    {
        isWrapping = false;
    }
    
    public void OnBandageGrabbed(Transform bandage)
    {
        bandageRoll = bandage;
        Debug.Log("Bandage grabbed - ready to wrap");
        
        // Get bandage renderer for visual feedback
        if (bandageRenderer == null && bandage != null)
        {
            bandageRenderer = bandage.GetComponent<Renderer>();
        }
        
        ActivateBandaging();
    }
    
    public void OnBandageReleased()
    {
        Debug.Log("Bandage released");
    }
    
    // GIZMOS
    
    void OnDrawGizmos()
    {
        if (limbCenter == null) return;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(limbCenter.position - limbCenter.up * 0.3f, 
                       limbCenter.position + limbCenter.up * 0.3f);
        
        Gizmos.color = Color.yellow;
        DrawCircleGizmo(limbCenter.position, limbCenter.up, wrapDetectionRadius, 32);
        
        Gizmos.color = Color.green;
        DrawCircleGizmo(limbCenter.position, limbCenter.up, limbRadius, 32);
    }
    
    void DrawCircleGizmo(Vector3 center, Vector3 normal, float radius, int segments)
    {
        Vector3 forward = Vector3.Slerp(Vector3.forward, -normal, 0.5f);
        Vector3 right = Vector3.Cross(normal, forward).normalized;
        forward = Vector3.Cross(right, normal).normalized;
        
        Vector3 prevPoint = center + (forward * radius);
        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * 2f * Mathf.PI;
            Vector3 nextPoint = center + (forward * Mathf.Cos(angle) + right * Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
}