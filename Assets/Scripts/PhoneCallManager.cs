using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using DialogueEditor;

public class PhoneCallManager : MonoBehaviour
{
    [Header("Game Events")]
    public UnityEvent onValidCallMade;

    [Header("References")]
    public GameObject callUI;
    public GameObject dialUI;
    public PhoneKeypad keypad;
    public ScreenShake screenShake;

    [Header("Dialogue")]
    public GameObject SpatialPanelManipulatorModel;

    [Tooltip("Enable in scenes with no dialogue (e.g. GameScene) so the call resets automatically.")]
    [SerializeField] private bool autoEndEnabled = false;

    [Tooltip("How long the call UI stays visible before resetting when autoEndEnabled is true.")]
    [SerializeField] private float callDisplayDuration = 2f;

    private const string VALID_NUMBER = "911";
    private bool callActive = false;
    private Coroutine autoEndCoroutine;

    void OnEnable()
    {
        ConversationManager.OnConversationEnded += OnConversationEnded;
    }

    void OnDisable()
    {
        ConversationManager.OnConversationEnded -= OnConversationEnded;
    }

    public void TryCall(string number)
    {
        if (callActive) return;

        if (number == VALID_NUMBER)
            StartCall();
        else
            StartCoroutine(HandleError());
    }

    void StartCall()
    {
        callActive = true;
        onValidCallMade.Invoke();
        callUI.SetActive(true);
        dialUI.SetActive(false);
        keypad.ResetKeypad();

        if (SpatialPanelManipulatorModel != null) SpatialPanelManipulatorModel.SetActive(true);

        if (autoEndEnabled)
            autoEndCoroutine = StartCoroutine(AutoEnd());
    }

    IEnumerator AutoEnd()
    {
        yield return new WaitForSeconds(callDisplayDuration);
        EndCall();
    }

    public void OnConversationEnded()
    {
        if (autoEndCoroutine != null)
        {
            StopCoroutine(autoEndCoroutine);
            autoEndCoroutine = null;
        }
        EndCall();
    }

    void EndCall()
    {
        callActive = false;
        callUI.SetActive(false);
        dialUI.SetActive(true);
        if (SpatialPanelManipulatorModel != null) SpatialPanelManipulatorModel.SetActive(false);
        keypad.ResetKeypad();
    }

    IEnumerator HandleError()
    {
        screenShake.TriggerShake();
        yield return new WaitForSeconds(0.5f);
        keypad.ResetKeypad();
    }
}
