using UnityEngine;
using UnityEngine.UI;

public class RPSAIResultHandler : MonoBehaviour
{
    public Text resultText;
    public GameObject boardPanel;
    public GameObject rpsPanel;

    public static int whoStarts = 1; // ✅ تحديد من سيبدأ

    public void HandleResult(int playerChoice, int aiChoice)
    {
        resultText.text = ""; // ❌ نخفي النتيجة مؤقتًا

        // ✅ بعد 4 ثواني، نظهر تكست الفائز
        Invoke(nameof(ShowWinnerText), 4f);

        // ✅ بعد 4.5 ثانية، ننتقل للبورد
        Invoke(nameof(TransitionToBoard), 4.5f);
    }

    private void ShowWinnerText()
    {
        resultText.text = DetermineWinner();
    }

    private void TransitionToBoard()
    {
        rpsPanel.SetActive(false);
        boardPanel.SetActive(true);

        // ✅ تحديد من يبدأ في اللعبة التراثية
        whoStarts = resultText.text == "اللاعب فاز!" ? 1 : 2;
    }

    private string DetermineWinner()
    {
        if (playerChoice == aiChoice) return "تعادل!";
        if ((playerChoice == 0 && aiChoice == 2) || (playerChoice == 1 && aiChoice == 0) || (playerChoice == 2 && aiChoice == 1))
            return "اللاعب فاز!";
        return "AI فاز!";
    }
}

