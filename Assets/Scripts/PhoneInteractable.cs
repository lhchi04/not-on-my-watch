using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PhoneInteractable : MonoBehaviour
{
    public GameObject keypadUI;

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnPickedUp);
        grabInteractable.selectExited.AddListener(OnPutDown);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnPickedUp);
        grabInteractable.selectExited.RemoveListener(OnPutDown);
    }

    void OnPickedUp(SelectEnterEventArgs args)
    {
        keypadUI.SetActive(true);
    }

    void OnPutDown(SelectExitEventArgs args)
    {
        keypadUI.SetActive(false);
    }
}
