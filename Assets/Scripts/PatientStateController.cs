using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PatientStateController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject[] bloodObjects;

    [Header("Fall Timing")]
    [SerializeField] private bool autoFall = false;
    [SerializeField] private float minFallDelay = 5f;
    [SerializeField] private float maxFallDelay = 20f;
    [SerializeField] private float bloodShowDelay = 1.5f;

    [Header("Lie Down (CPR patients)")]
    [SerializeField] private bool lieDownWhenFalling = false;
    [SerializeField] private float lieDownDelay = 0.4f;
    [SerializeField] private float lieDownDuration = 0.5f;

    [Header("Animation Parameters")]
    [SerializeField] private float normalWalkSpeed = 1.5f;
    [SerializeField] private string fallTriggerName = "fallDown";
    [SerializeField] private string healedTriggerName = "isHealed";
    [SerializeField] private string speedParamName = "Speed";
    [SerializeField] private string idleTypeParamName = "IdleType";
    [SerializeField] private int idleVariations = 3;

    [Header("Events")]
    public UnityEvent onPatientFallen;
    public UnityEvent onPatientHealed;

    private enum State { Normal, Falling, Fallen, Healed }
    private State state = State.Normal;

    public bool IsFallen => state == State.Fallen;

    public void SetAlreadyFallen()
    {
        state = State.Fallen;
        SetBloodVisible(true);
    }

    void Start()
    {
        SetBloodVisible(false);
        if (animator != null)
        {
            animator.SetFloat(idleTypeParamName, Random.Range(0, idleVariations));
            animator.SetFloat(speedParamName, normalWalkSpeed);
        }
        if (autoFall) StartCoroutine(FallAfterDelay());
    }

    IEnumerator FallAfterDelay()
    {
        yield return new WaitForSeconds(Random.Range(minFallDelay, maxFallDelay));
        Fall();
    }

    public void Fall()
    {
        if (state != State.Normal) return;
        state = State.Falling;
        if (animator != null)
        {
            animator.SetFloat(speedParamName, 0f);
            animator.SetTrigger(fallTriggerName);
        }
        StartCoroutine(ShowBloodAfterDelay());
        if (lieDownWhenFalling) StartCoroutine(LieDownRoutine());
        onPatientFallen?.Invoke();
    }

    IEnumerator LieDownRoutine()
    {
        yield return new WaitForSeconds(lieDownDelay);
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(-90f, transform.eulerAngles.y, 0f);
        float elapsed = 0f;
        while (elapsed < lieDownDuration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / lieDownDuration);
            yield return null;
        }
        transform.rotation = endRot;
    }

    IEnumerator ShowBloodAfterDelay()
    {
        yield return new WaitForSeconds(bloodShowDelay);
        SetBloodVisible(true);
        state = State.Fallen;
    }

    public void OnHealed()
    {
        if (state != State.Fallen) return;
        state = State.Healed;
        SetBloodVisible(false);
        if (animator != null)
        {
            animator.SetFloat(speedParamName, normalWalkSpeed);
            animator.SetTrigger(healedTriggerName);
        }
        onPatientHealed?.Invoke();
    }

    void SetBloodVisible(bool visible)
    {
        if (bloodObjects == null) return;
        foreach (var obj in bloodObjects)
            if (obj != null) obj.SetActive(visible);
    }
}
