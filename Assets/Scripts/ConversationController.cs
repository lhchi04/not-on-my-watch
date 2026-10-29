using UnityEngine;
using DialogueEditor;

public class ConversationController : MonoBehaviour
{

    [SerializeField] private NPCConversation myConversation;
    [SerializeField] private Animator npcAnimator;
    private bool isConversing = false;

    private bool isFalling = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera") && !isConversing)
        {
            isConversing = true;
            npcAnimator.SetBool("isTalking", true);
            ConversationManager.Instance.StartConversation(myConversation);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            isConversing = false;
            npcAnimator.SetBool("isTalking", false);
        }
    }

    public void StartFalling()
    {
        npcAnimator.SetTrigger("fallDown");
        isFalling = true;
    }

    public void StopFalling()
    {
        Debug.Log("StopFalling called!");
        isFalling = false;
        npcAnimator.SetBool("isTalking", true);
    }
}
