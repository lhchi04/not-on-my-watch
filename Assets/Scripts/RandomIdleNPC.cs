// RandomIdleNPC.cs - Enhanced NPC with random poses
using UnityEngine;
using System.Collections;

public class RandomIdleNPC : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public Transform[] waypoints;
    public bool randomWaypoints = false;
    
    [Header("Animation")]
    public Animator animator;
    public bool useRandomIdles = true;
    
    [Header("Random Idle Settings")]
    public int numberOfIdleVariations = 3;  // How many idle animations you have
    public float minIdleChangeTime = 5f;    // Minimum time before changing idle
    public float maxIdleChangeTime = 15f;   // Maximum time before changing idle
    
    [Header("Starting Pose")]
    public bool randomStartingPose = true;
    public int specificStartPose = -1;      // -1 = random, 0-7 = specific pose
    
    [Header("Current State")]
    private int currentWaypointIndex = 0;
    private bool isWalking = false;
    private int currentIdleType = 0;
    private float nextIdleChangeTime;
    
    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        
        // Set random or specific starting pose
        if (randomStartingPose || specificStartPose == -1)
        {
            currentIdleType = Random.Range(0, numberOfIdleVariations);
        }
        else
        {
            currentIdleType = Mathf.Clamp(specificStartPose, 0, numberOfIdleVariations - 1);
        }
        
        SetIdleType(currentIdleType);
        
        // Schedule first idle change
        if (useRandomIdles)
        {
            nextIdleChangeTime = Time.time + Random.Range(minIdleChangeTime, maxIdleChangeTime);
        }
        
        // Start walking if waypoints exist
        if (waypoints != null && waypoints.Length > 0)
        {
            isWalking = true;
        }
    }
    
    void Update()
    {
        // Random idle changing (only when not walking)
        if (useRandomIdles && !isWalking && Time.time >= nextIdleChangeTime)
        {
            ChangeToRandomIdle();
        }
        
        // Movement logic
        if (!isWalking || waypoints == null || waypoints.Length == 0)
        {
            SetAnimationSpeed(0);
            return;
        }
        
        WalkToWaypoint();
    }
    
    void WalkToWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        if (targetWaypoint == null) return;
        
        Vector3 targetPosition = targetWaypoint.position;
        targetPosition.y = transform.position.y;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        float distanceToWaypoint = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(targetPosition.x, 0, targetPosition.z)
        );
        
        if (distanceToWaypoint < 0.5f)
        {
            SelectNextWaypoint();
            return;
        }
        
        // Rotate to face direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        
        // Move
        transform.position += direction * walkSpeed * Time.deltaTime;
        SetAnimationSpeed(walkSpeed);
    }
    
    void SelectNextWaypoint()
    {
        if (randomWaypoints)
        {
            currentWaypointIndex = Random.Range(0, waypoints.Length);
        }
        else
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        isWalking = false;
        StartCoroutine(WaitAtWaypoint());
    }

    IEnumerator WaitAtWaypoint()
    {
        SetAnimationSpeed(0);
        nextIdleChangeTime = Time.time + Random.Range(minIdleChangeTime, maxIdleChangeTime);
        float waitTime = Random.Range(minIdleChangeTime, maxIdleChangeTime);
        yield return new WaitForSeconds(waitTime);
        isWalking = true;
    }
    
    void ChangeToRandomIdle()
    {
        // Pick a different idle than current one
        int newIdleType = currentIdleType;
        while (newIdleType == currentIdleType && numberOfIdleVariations > 1)
        {
            newIdleType = Random.Range(0, numberOfIdleVariations);
        }
        
        currentIdleType = newIdleType;
        SetIdleType(currentIdleType);
        
        // Schedule next change
        nextIdleChangeTime = Time.time + Random.Range(minIdleChangeTime, maxIdleChangeTime);
        
        Debug.Log($"{gameObject.name} changed to idle type: {currentIdleType}");
    }
    
    void SetIdleType(int idleType)
    {
        if (animator != null)
        {
            animator.SetFloat("IdleType", (float)idleType);
        }
    }
    
    void SetAnimationSpeed(float speed)
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", speed);
        }
    }
    
    // PUBLIC METHOD - Call this to set specific pose
    public void SetSpecificPose(int poseNumber)
    {
        currentIdleType = Mathf.Clamp(poseNumber, 0, numberOfIdleVariations - 1);
        SetIdleType(currentIdleType);
    }
    
    // PUBLIC METHOD - Pause/resume random idle changing
    public void SetRandomIdleEnabled(bool enabled)
    {
        useRandomIdles = enabled;
    }
    
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        
        Gizmos.color = Color.yellow;
        
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            
            Gizmos.DrawSphere(waypoints[i].position, 0.3f);
            
            int nextIndex = (i + 1) % waypoints.Length;
            if (waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            }
        }
    }
}