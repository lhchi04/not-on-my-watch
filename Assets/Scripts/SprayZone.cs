using UnityEngine;

public class SprayZone : MonoBehaviour
{
    private Renderer localBloodRenderer;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
            Debug.LogWarning("[SprayZone] No collider found on: " + gameObject.name);
        else if (!col.isTrigger)
            Debug.LogWarning("[SprayZone] Collider is not set to Is Trigger on: " + gameObject.name);
        else
            Debug.Log("[SprayZone] SprayZone ready on: " + gameObject.name);

        // Find the blood renderer on the same patient (sibling named "Blood")
        if (transform.parent != null)
        {
            Transform bloodTransform = transform.parent.Find("Blood");
            if (bloodTransform != null)
            {
                localBloodRenderer = bloodTransform.GetComponent<Renderer>();
                Debug.Log("[SprayZone] Found local blood renderer on: " + bloodTransform.name);
            }
            else
            {
                Debug.LogWarning("[SprayZone] No sibling named 'Blood' found under: " + transform.parent.name);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[SprayZone] Something entered: " + other.gameObject.name);

        SprayInteraction spray = other.GetComponent<SprayInteraction>();
        if (spray != null)
        {
            Debug.Log("[SprayZone] Spray bottle entered zone");
            if (localBloodRenderer != null)
                spray.SetBloodRenderer(localBloodRenderer);
            spray.SetActiveZone(gameObject);
            spray.EnterSprayZone();
        }
        else
        {
            Debug.Log("[SprayZone] Entered object has no SprayInteraction: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("[SprayZone] Something exited: " + other.gameObject.name);

        SprayInteraction spray = other.GetComponent<SprayInteraction>();
        if (spray != null)
        {
            Debug.Log("[SprayZone] Spray bottle left zone");
            spray.ExitSprayZone();
        }
    }
}