using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to the XR Rig (or any persistent GameObject in GameScene).
/// Press Button A (right controller) to open a "Return to Start?" confirmation panel.
///
/// Setup in Inspector:
///  - confirmPanel   : root GameObject of the confirmation UI (starts hidden)
///  - returnAction   : InputActionReference → GameInputActions / Navigation / ReturnToStart
///  - spawnPoint     : (optional) Transform in front of the camera — panel moves here when shown
///  - startSceneName : name of your start scene (default "StartScene")
/// </summary>
public class ReturnToStartConfirm : MonoBehaviour
{
    [Header("Confirmation Panel")]
    public GameObject confirmPanel;

    [Header("Input")]
    public InputActionReference returnAction;

    [Header("Spawn Position (optional)")]
    [Tooltip("If set, the panel repositions here each time it opens.")]
    public Transform spawnPoint;

    [Header("Scene")]
    public string startSceneName = "StartScene";

    private void Awake()
    {
        if (confirmPanel == null) { enabled = false; return; }
        confirmPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (returnAction == null) return;
        returnAction.action.Enable();
        returnAction.action.performed += OnButtonA;
    }

    private void OnDisable()
    {
        if (returnAction == null) return;
        returnAction.action.performed -= OnButtonA;
    }

    private void OnButtonA(InputAction.CallbackContext ctx)
    {
        // If already open, pressing A again cancels (optional — remove if you only want UI buttons)
        if (confirmPanel.activeSelf)
        {
            Cancel();
            return;
        }

        ShowConfirm();
    }

    private void ShowConfirm()
    {
        if (spawnPoint != null)
        {
            confirmPanel.transform.position = spawnPoint.position;
            confirmPanel.transform.rotation = spawnPoint.rotation;
        }

        confirmPanel.SetActive(true);
    }

    // Wire the "Yes" button's OnClick to this
    public void Confirm()
    {
        SceneManager.LoadScene(startSceneName);
    }

    // Wire the "No" button's OnClick to this
    public void Cancel()
    {
        confirmPanel.SetActive(false);
    }
}
