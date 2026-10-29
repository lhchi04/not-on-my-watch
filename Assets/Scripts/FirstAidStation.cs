using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attach to each first-aid station object in the scene.
///
/// Each station has one or more ToolSlots. A ToolSlot defines:
///   - the tool prefab to spawn
///   - the Transform where it should sit on the station
///
/// When the player grabs a tool the slot tracks it.
/// An optional respawn time returns the tool to the station
/// (set respawnDelay to 0 to never respawn).
///
/// An optional proximity indicator (e.g. a glowing ring mesh)
/// is shown while the player is within range.
///
/// Setup in Inspector:
///  1. Add this script to a station parent GameObject.
///  2. Fill toolSlots with prefabs + spawn point Transforms
///     (create empty child GameObjects as slot positions).
///  3. Optionally assign a proximityIndicator child mesh.
///  4. Set detectionRadius to taste (default 2m).
///  5. Set respawnDelay (0 = never respawn).
/// </summary>
public class FirstAidStation : MonoBehaviour
{
    [System.Serializable]
    public class ToolSlot
    {
        [Tooltip("The first-aid tool prefab to place at this slot.")]
        public GameObject toolPrefab;

        [Tooltip("Where on the station the tool sits (an empty child Transform).")]
        public Transform spawnPoint;

        [HideInInspector] public GameObject spawnedInstance;
        [HideInInspector] public bool isAvailable = true;
    }

    [Header("Tool Slots")]
    public List<ToolSlot> toolSlots = new List<ToolSlot>();

    [Header("Respawn")]
    [Tooltip("Seconds before a taken tool reappears at the station. Set to 0 to never respawn.")]
    public float respawnDelay = 0f;

    [Header("Proximity Indicator")]
    [Tooltip("Child GameObject (e.g. glowing ring) shown when player is nearby. Leave empty to skip.")]
    public GameObject proximityIndicator;

    [Tooltip("Distance at which the indicator becomes visible.")]
    public float detectionRadius = 2f;

    private Transform playerHead;

    void Start()
    {
        SpawnAllTools();
        SetIndicator(false);

        // Find the main camera (player head) — works with both OVR and XRI rigs
        if (Camera.main != null)
            playerHead = Camera.main.transform;
    }

    void Update()
    {
        if (proximityIndicator == null || playerHead == null) return;

        float dist = Vector3.Distance(transform.position, playerHead.position);
        SetIndicator(dist <= detectionRadius);
    }

    // ── Tool Spawning ──────────────────────────────────────────────────────────

    void SpawnAllTools()
    {
        foreach (var slot in toolSlots)
            SpawnToolInSlot(slot);
    }

    void SpawnToolInSlot(ToolSlot slot)
    {
        if (slot.toolPrefab == null || slot.spawnPoint == null) return;

        slot.spawnedInstance = Instantiate(
            slot.toolPrefab,
            slot.spawnPoint.position,
            slot.spawnPoint.rotation,
            slot.spawnPoint          // parent to slot so it moves with the station
        );
        slot.isAvailable = true;

        // Listen for grab so we know when the tool is taken
        var grab = slot.spawnedInstance.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            // Capture slot reference for the closure
            ToolSlot capturedSlot = slot;
            grab.selectEntered.AddListener(_ => OnToolTaken(capturedSlot));
        }
    }

    void OnToolTaken(ToolSlot slot)
    {
        slot.isAvailable = false;

        // Unparent from station so physics works properly after grabbing
        if (slot.spawnedInstance != null)
            slot.spawnedInstance.transform.SetParent(null);

        if (respawnDelay > 0f)
            StartCoroutine(RespawnAfterDelay(slot));
    }

    IEnumerator RespawnAfterDelay(ToolSlot slot)
    {
        yield return new WaitForSeconds(respawnDelay);

        // Destroy old instance if it still exists (e.g. player dropped it)
        if (slot.spawnedInstance != null)
            Destroy(slot.spawnedInstance);

        SpawnToolInSlot(slot);
    }

    // ── Indicator ─────────────────────────────────────────────────────────────

    void SetIndicator(bool visible)
    {
        if (proximityIndicator != null && proximityIndicator.activeSelf != visible)
            proximityIndicator.SetActive(visible);
    }

    // Draw radius in editor for easy visualisation
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Gizmos.DrawSphere(transform.position, detectionRadius);
        Gizmos.color = new Color(0f, 1f, 0.5f, 1f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
