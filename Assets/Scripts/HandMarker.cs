// HandMarker.cs - Attach to hand marker object on patient chest
using System.Collections;
using UnityEngine;

public class HandMarker : MonoBehaviour
{
    public Material glowMaterial;           // Glowing outline material
    public GameObject handIconPrefab;       // 3D hand icon or sprite
    
    private Renderer markerRenderer;
    private bool handsInPosition = false;
    
    void Start()
    {
        // markerRenderer = GetComponent<Renderer>();
        // Start with pulsing glow animation
        // StartCoroutine(PulseGlow());
    }
    
    // Make marker pulse to draw attention
    IEnumerator PulseGlow()
    {
        while (!handsInPosition)
        {
            float emission = Mathf.PingPong(Time.time * 2f, 1f);
            glowMaterial.SetColor("_EmissionColor", Color.cyan * emission);
            yield return null;
        }
    }
    
    public void HandsPlaced()
    {
        handsInPosition = true;
        // Fade out or hide marker
        // StartCoroutine(FadeOut());
        
    }
    
    IEnumerator FadeOut()
    {
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime;
            Color c = markerRenderer.material.color;
            markerRenderer.material.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}