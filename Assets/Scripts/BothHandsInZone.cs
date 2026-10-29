using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects when two tagged objects (e.g., both player hands) are inside a trigger zone
/// </summary>
[RequireComponent(typeof(Collider))]
public class BothHandsInZone : MonoBehaviour
{
    [Tooltip("Tag required on entering objects")]
    public string requiredTag = "PlayerHand";

    // When both hands enter the zone
    public UnityEvent OnBothHandsEntered = new UnityEvent();

    // When one or both hands leave the zone
    public UnityEvent OnHandsLeft = new UnityEvent();

    private int handCount = 0;
    private bool bothInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!MatchesTag(other.gameObject))
            return;

        handCount++;

        if (handCount >= 2 && !bothInside)
        {
            bothInside = true;
            OnBothHandsEntered.Invoke();
        }
        Debug.Log("Something entered: " + other.gameObject.name + " tag: " + other.tag);

    }

    private void OnTriggerExit(Collider other)
    {
        if (!MatchesTag(other.gameObject))
            return;

        handCount--;

        if (handCount < 2 && bothInside)
        {
            bothInside = false;
            OnHandsLeft.Invoke();
        }
    }

    private bool MatchesTag(GameObject obj)
    {
        if (requiredTag == string.Empty)
            return true;

        return obj.CompareTag(requiredTag);
    }

    private void OnValidate()
    {
        if (TryGetComponent(out Collider collider))
            collider.isTrigger = true;
    }

}
