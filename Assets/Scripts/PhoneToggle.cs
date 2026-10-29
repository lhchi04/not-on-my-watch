using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attach to the XR Rig (or any persistent GameObject).
/// When the assigned controller button is pressed, the phone
/// appears attached to the player's non-dominant hand.
/// Pressing again hides it.
///
/// Setup in Inspector:
///  - phoneObject        : the phone GameObject (with XRGrabInteractable)
///  - handAttachPoint    : a Transform child of the left or right hand controller
///                         (e.g. LeftHandController/PhoneAttach — position/rotate it
///                          so the phone sits naturally in the palm)
///  - toggleAction       : an InputActionReference bound to the controller button
///                         (e.g. XRI LeftHand/SecondaryButton  or  Y Button)
/// </summary>
public class PhoneToggle : MonoBehaviour
{
    [Header("Phone")]
    [Tooltip("The phone GameObject that should be toggled.")]
    public GameObject phoneObject;

    [Tooltip("A Transform parented to the hand controller where the phone will snap when shown.")]
    public Transform handAttachPoint;

    [Header("Input")]
    [Tooltip("InputActionReference for the controller button that toggles the phone (e.g. Y / B button).")]
    public InputActionReference toggleAction;

    private bool phoneVisible = false;
    private XRGrabInteractable grabInteractable;
    private Rigidbody phoneRb;

    void Awake()
    {
        if (phoneObject == null) { enabled = false; return; }
        grabInteractable = phoneObject.GetComponent<XRGrabInteractable>();
        phoneRb = phoneObject.GetComponent<Rigidbody>();
        phoneObject.SetActive(false);

        if (handAttachPoint == null)
            Debug.LogWarning("[PhoneToggle] handAttachPoint is not assigned — phone will appear at its original world position.", this);

        if (toggleAction == null)
            Debug.LogWarning("[PhoneToggle] toggleAction is not assigned — only Y key shortcut will work in Editor.", this);
    }

    void OnEnable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.Enable();
            toggleAction.action.performed += OnTogglePressed;
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnPhoneGrabbed);
            grabInteractable.selectExited.AddListener(OnPhoneReleased);
        }
    }

    void OnDisable()
    {
        if (toggleAction != null)
            toggleAction.action.performed -= OnTogglePressed;

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnPhoneGrabbed);
            grabInteractable.selectExited.RemoveListener(OnPhoneReleased);
        }
    }

    // When the player grabs the phone, unparent it so XRI can move it freely
    void OnPhoneGrabbed(SelectEnterEventArgs args)
    {
        phoneObject.transform.SetParent(null);
        if (phoneRb != null)
        {
            phoneRb.isKinematic = false;
            phoneRb.useGravity = true;
        }
    }

    // When the player releases the phone, re-attach it to the hand
    void OnPhoneReleased(SelectExitEventArgs args)
    {
        AttachToHand();
    }

    void OnTogglePressed(InputAction.CallbackContext ctx)
    {
        Debug.Log("[PhoneToggle] Button pressed via controller input.");
        Toggle();
    }

    void Toggle()
    {
        // Don't toggle away while the player is actively holding the phone
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            Debug.Log("[PhoneToggle] Toggle ignored — phone is currently held.");
            return;
        }

        phoneVisible = !phoneVisible;
        Debug.Log($"[PhoneToggle] phoneVisible = {phoneVisible}");

        if (phoneVisible)
            ShowPhone();
        else
            HidePhone();
    }

    void ShowPhone()
    {
        if (phoneObject == null) return;

        AttachToHand();
        phoneObject.SetActive(true);
        Debug.Log("[PhoneToggle] Phone shown.");
    }

    void AttachToHand()
    {
        // Freeze physics so it doesn't fall
        if (phoneRb != null)
        {
            phoneRb.isKinematic = true;
            phoneRb.useGravity = false;
            phoneRb.linearVelocity = Vector3.zero;
            phoneRb.angularVelocity = Vector3.zero;
        }

        if (handAttachPoint != null)
        {
            phoneObject.transform.SetParent(handAttachPoint, false);
            phoneObject.transform.localPosition = Vector3.zero;
            phoneObject.transform.localRotation = Quaternion.identity;
        }
    }

    public void HidePhone()
    {
        if (phoneObject == null) return;

        phoneObject.SetActive(false);
        phoneObject.transform.SetParent(null);

        if (phoneRb != null)
        {
            phoneRb.isKinematic = false;
            phoneRb.useGravity = true;
        }

        phoneVisible = false;
    }
}
