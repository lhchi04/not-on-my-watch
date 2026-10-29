using System.Collections;
using UnityEngine;

public class WoundCareWalker : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float waypointReachedDistance = 0.5f;
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 6f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private float getUpDuration = 2f;

    private bool isActive = false;
    private Coroutine walkCoroutine;

    void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
            StartWalking();
    }

    public void StopWalking()
    {
        isActive = false;
        if (walkCoroutine != null)
        {
            StopCoroutine(walkCoroutine);
            walkCoroutine = null;
        }
        SetSpeed(0f);
    }

    public void StartWalking()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        StopWalking();
        isActive = true;
        walkCoroutine = StartCoroutine(WalkRoutine());
    }

    // Wire this to GamePatient.onHealed — waits for the get-up animation then resumes walking
    public void StartWalkingAfterHealed()
    {
        StartCoroutine(ResumeAfterGetUp());
    }

    IEnumerator ResumeAfterGetUp()
    {
        yield return new WaitForSeconds(getUpDuration);
        if (animator != null)
            animator.CrossFadeInFixedTime("Walking", 0.25f);
        StartWalking();
    }

    IEnumerator WalkRoutine()
    {
        while (isActive)
        {
            Transform target = waypoints[Random.Range(0, waypoints.Length)];
            if (target == null) { yield return null; continue; }

            SetSpeed(moveSpeed);

            while (isActive)
            {
                float dist = Vector3.Distance(
                    new Vector3(transform.position.x, 0f, transform.position.z),
                    new Vector3(target.position.x,    0f, target.position.z));

                if (dist <= waypointReachedDistance) break;

                Vector3 dir = target.position - transform.position;
                dir.y = 0f;
                dir.Normalize();

                transform.position += dir * moveSpeed * Time.deltaTime;

                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(dir),
                        Time.deltaTime * 5f);

                yield return null;
            }

            if (!isActive) yield break;

            SetSpeed(0f);
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        }
    }

    void SetSpeed(float speed)
    {
        if (animator != null)
            animator.SetFloat(speedParam, speed);
    }
}
