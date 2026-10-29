using UnityEngine;
using UnityEngine.Events;
using Unity.VRTemplate;
using UnityEngine.XR.Interaction.Toolkit;

public class SprayInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StepManager stepManager;
    [SerializeField] private GameObject sprayZoneIndicator;
    [SerializeField] private Renderer bloodRenderer;
    [SerializeField] private AudioClip spraySound;

    public UnityEvent onSprayComplete;

    [Header("Settings")]
    [SerializeField] private int requiredSprays = 3;
    [SerializeField] private float fadeAmount = 0.25f;
    [SerializeField] private int myStepIndex = 2;

    private int sprayCount = 0;
    private bool isCompleted = false;
    private bool isInSprayZone = false;
    private AudioSource audioSource;
    private Color bloodColor;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private GameObject activeZone;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Wire events in code - no Inspector changes needed
        grabInteractable.activated.AddListener(OnActivated);
        grabInteractable.selectEntered.AddListener(OnPickedUp);
        grabInteractable.selectExited.AddListener(OnPutDown);

        if (bloodRenderer != null)
        {
            bloodColor = bloodRenderer.material.color;
            bloodColor.a = 1f;
            bloodRenderer.material.color = bloodColor;
        }

        if (sprayZoneIndicator != null)
            sprayZoneIndicator.SetActive(true);
    }

    void OnPickedUp(SelectEnterEventArgs args)
    {
        Debug.Log("[Spray] Bottle picked up");
    }

    void OnPutDown(SelectExitEventArgs args)
    {
        Debug.Log("[Spray] Bottle put down");
    }

    void OnActivated(ActivateEventArgs args)
    {
        Debug.Log("[Spray] Activated fired");
        if (isCompleted) return;

        if (isInSprayZone)
        {
            Debug.Log("[Spray] Spraying on wound");
            Spray();
        }
        else
        {
            if (spraySound != null)
                audioSource.PlayOneShot(spraySound);
            Debug.Log("[Spray] Sprayed but not on wound");
        }
    }

    public void SetBloodRenderer(Renderer r)
    {
        if (r == bloodRenderer) return;

        bloodRenderer = r;
        sprayCount = 0;
        isCompleted = false;

        if (bloodRenderer != null)
        {
            bloodColor = bloodRenderer.material.color;
            bloodColor.a = 1f;
            bloodRenderer.material.color = bloodColor;
        }
    }

    public void SetActiveZone(GameObject zone)
    {
        activeZone = zone;
    }

    public void EnterSprayZone()
    {
        isInSprayZone = true;
        Debug.Log("[Spray] Entered spray zone");
    }

    public void ExitSprayZone()
    {
        isInSprayZone = false;
        Debug.Log("[Spray] Left spray zone");
    }

    void Spray()
    {
        if (spraySound != null)
            audioSource.PlayOneShot(spraySound);

        if (bloodRenderer != null)
        {
            bloodColor.a = Mathf.Max(0f, bloodColor.a - fadeAmount);
            bloodRenderer.material.color = bloodColor;
            Debug.Log("[Spray] Blood alpha: " + bloodColor.a);
        }

        sprayCount++;

        if (sprayCount >= requiredSprays)
        {
            isCompleted = true;

            GameObject zoneToHide = activeZone != null ? activeZone : sprayZoneIndicator;
            if (zoneToHide != null)
                zoneToHide.SetActive(false);

            if (bloodRenderer != null)
            {
                bloodColor.a = 0f;
                bloodRenderer.material.color = bloodColor;
            }

            onSprayComplete.Invoke();

            if (stepManager != null)
                stepManager.UnlockContinue();
        }

        if (stepManager != null && stepManager.m_CurrentStepIndex == myStepIndex)
            stepManager.UnlockContinue();
    }
}