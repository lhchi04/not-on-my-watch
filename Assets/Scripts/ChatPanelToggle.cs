using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;

/// <summary>
/// Attach to the XR Rig (or any persistent GameObject).
/// Press the assigned button (X by default) to show/hide the chat panel.
///
/// Setup in Inspector:
///  - chatPanel    : the ChatPanel root GameObject in the scene
///  - toggleAction : InputActionReference bound to XRI LeftHand/PrimaryButton  (X button)
///  - spawnPoint   : (optional) Transform in front of the camera — if assigned,
///                   the panel repositions there each time it is shown
/// </summary>
public class ChatPanelToggle : MonoBehaviour
{
    [Header("Panel")]
    public GameObject chatPanel;

    [Header("Input")]
    public InputActionReference toggleAction;

    [Header("Spawn Position (optional)")]
    [Tooltip("If set, the panel moves to this transform every time it is shown.")]
    public Transform spawnPoint;

    private void Awake()
    {
        if (chatPanel == null) { enabled = false; return; }
        chatPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (toggleAction == null) return;
        toggleAction.action.Enable();
        toggleAction.action.performed += OnToggle;
    }

    private void OnDisable()
    {
        if (toggleAction == null) return;
        toggleAction.action.performed -= OnToggle;
    }

    private void OnToggle(InputAction.CallbackContext ctx) => Toggle();

    public void Toggle()
    {
        if (chatPanel == null) return;

        bool show = !chatPanel.activeSelf;

        if (show && spawnPoint != null)
        {
            chatPanel.transform.position = spawnPoint.position;
            chatPanel.transform.rotation = spawnPoint.rotation;
        }

        chatPanel.SetActive(show);

        if (!show)
            GlobalNonNativeKeyboard.instance?.HideKeyboard();
    }
}
