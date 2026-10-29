using UnityEngine;

public class WorldSpaceUIFollow : MonoBehaviour
{
    [Tooltip("Leave empty to auto-find Camera.main.")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("How far in front of the camera the panel appears.")]
    [SerializeField] private float distance = 1.5f;

    [Tooltip("Vertical offset from the camera's forward position (negative = lower).")]
    [SerializeField] private float verticalOffset = -0.2f;

    [Tooltip("Flip the panel to face the camera rather than away from it.")]
    [SerializeField] private bool flipFacing = false;

    void OnEnable()
    {
        SnapToCamera();
    }

    void SnapToCamera()
    {
        if (cameraTransform == null)
        {
            Camera main = Camera.main;
            if (main == null) return;
            cameraTransform = main.transform;
        }

        // Use only horizontal yaw so the panel stays upright
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        forward.Normalize();

        transform.position = cameraTransform.position
                           + forward * distance
                           + Vector3.up * verticalOffset;

        transform.rotation = Quaternion.LookRotation(flipFacing ? -forward : forward);
    }
}
