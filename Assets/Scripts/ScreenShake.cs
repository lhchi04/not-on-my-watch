using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenShake : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 0.4f;
    public float magnitude = 0.08f;
    public Transform cameraTransform;

    [Header("Red Flash")]
    public Image redOverlay;  // A full screen red UI image

    public void TriggerShake()
    {
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        Vector3 originalPos = cameraTransform.localPosition;
        float elapsed = 0f;

        // Fade red in
        if (redOverlay != null)
            redOverlay.color = new Color(1f, 0f, 0f, 0.4f);

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cameraTransform.localPosition = new Vector3(
                originalPos.x + x,
                originalPos.y + y,
                originalPos.z
            );

            // Fade red out over time
            if (redOverlay != null)
            {
                float alpha = Mathf.Lerp(0.4f, 0f, elapsed / duration);
                redOverlay.color = new Color(1f, 0f, 0f, alpha);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localPosition = originalPos;

        // Make sure it's fully gone
        if (redOverlay != null)
            redOverlay.color = new Color(1f, 0f, 0f, 0f);
    }
}