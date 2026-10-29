using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BandageRoll : MonoBehaviour
{
    private BandageCircularWrapper[] allWrappers;
    private BandageCircularWrapper activeWrapper;
    private bool isHeld;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void Update()
    {
        if (!isHeld) return;

        // If active wrapper just completed, clear so we can find the next patient
        if (activeWrapper != null && activeWrapper.IsComplete)
            activeWrapper = null;

        if (activeWrapper != null) return;

        foreach (var wrapper in allWrappers)
        {
            if (wrapper != null && !wrapper.IsComplete && wrapper.IsNearLimb(transform.position))
            {
                activeWrapper = wrapper;
                activeWrapper.OnBandageGrabbed(transform);
                break;
            }
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isHeld = true;
        activeWrapper = null;
        allWrappers = FindObjectsByType<BandageCircularWrapper>(FindObjectsSortMode.None);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isHeld = false;
        if (activeWrapper != null)
            activeWrapper.OnBandageReleased();
        activeWrapper = null;
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}
