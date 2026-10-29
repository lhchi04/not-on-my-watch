using UnityEngine;
using UnityEngine.Events;
using Unity.VRTemplate;

public class WearableItem : MonoBehaviour
{
    public StepManager stepManager;
    [SerializeField] private int myStepIndex = 1;
    public UnityEvent onWorn;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            gameObject.SetActive(false);
            onWorn.Invoke();

            if (stepManager.m_CurrentStepIndex == myStepIndex)
                stepManager.UnlockContinue();
        }
    }
}