using UnityEngine;
using UnityEngine.UI;

public class HumanHandController : MonoBehaviour
{
    public Animator animator;
    public Button buttonRock;
    public Button buttonPaper;
    public Button buttonScissors;

    private bool hasChosen = false;

    public void PlayRock()
    {
        if (hasChosen) return;
        animator.SetBool("StartPlay", true);
        animator.SetFloat("Blend", 0f);
        DisableButtons();
    }

    public void PlayPaper()
    {
        if (hasChosen) return;
        animator.SetBool("StartPlay", true);
        animator.SetFloat("Blend", 1f);
        DisableButtons();
    }

    public void PlayScissors()
    {
        if (hasChosen) return;
        animator.SetBool("StartPlay", true);
        animator.SetFloat("Blend", 2f);
        DisableButtons();
    }

    private void DisableButtons()
    {
        hasChosen = true;
        buttonRock.interactable = false;
        buttonPaper.interactable = false;
        buttonScissors.interactable = false;
    }
}
