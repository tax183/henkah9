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

    public void PlayChoice(int choice)
    {
        if (hasChosen) return;

        animator.SetBool("StartPlay", true);
        animator.SetFloat("Blend", choice); // 0,1,2
        DisableButtons();
    }

    public void ResetButtons()
    {
        hasChosen = false;
        buttonRock.interactable = true;
        buttonPaper.interactable = true;
        buttonScissors.interactable = true;
        animator.SetBool("StartPlay", false); // ‰Êﬁ› «·√‰„Ì‘‰
        hasChosen = false;

    }


}
