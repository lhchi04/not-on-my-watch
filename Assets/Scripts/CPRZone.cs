// CPRZone.cs - Attach to invisible trigger zone on patient chest
using UnityEngine;

public class CPRZone : MonoBehaviour
{
    [Header("Detection")]
    public Transform leftHandMarker;     // Where left hand should be
    public Transform rightHandMarker;    // Where right hand should be
    public float detectionRadius = 0.1f; // How close hands need to be

    [Header("References")]
    // public HandMarker handMarkerVisual;
    public CPRCompressionUI compressionUI;

    public Transform playerLeftHand;
    public Transform playerRightHand;
    private bool handsInPosition = false;

    void Start()
    {
        // Get player hand transforms from XR Rig
        // playerLeftHand = GameObject.Find("LeftHand Controller").transform;
        // playerRightHand = GameObject.Find("RightHand Controller").transform;
    }

    void Update()
    {
        CheckHandPosition();
    }

    void CheckHandPosition()
    {
        // Check if both hands are near markers
        float leftDistance = Vector3.Distance(playerLeftHand.position, leftHandMarker.position);
        float rightDistance = Vector3.Distance(playerRightHand.position, rightHandMarker.position);

        if (leftDistance < detectionRadius && rightDistance < detectionRadius)
        {
            if (!handsInPosition)
            {
                OnHandsPositionedCorrectly();
            }
            handsInPosition = true;
        }
        else
        {
            if (handsInPosition)
            {
                OnHandsLeftPosition();
            }
            handsInPosition = false;
        }
    }

    void OnHandsPositionedCorrectly()
    {
        Debug.Log("Hands positioned correctly!");

        // handMarkerVisual.HandsPlaced();
        compressionUI.ActivateCompressionMode();

        // Haptic feedback
        // (Meta Quest controller vibration)
        // OVRInput.SetControllerVibration(0.5f, 0.5f, OVRInput.Controller.LTouch);
        // OVRInput.SetControllerVibration(0.5f, 0.5f, OVRInput.Controller.RTouch);
    }

    void OnHandsLeftPosition()
    {
        Debug.Log("Hands left position");
        compressionUI.DeactivateCompressionMode();
    }
}
