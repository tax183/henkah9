using UnityEngine;
using UnityEngine.UI;

public class RPSGameManager : MonoBehaviour
{
    public HumanHandController player1HandController;
    public HumanHandController player2HandController;

    public GameObject player1HandObject;
    public GameObject player2HandObject;

    public GameObject panelPlayer1;
    public GameObject panelPlayer2;
    public GameObject panelResult;
    public Text resultText;

    private int player1Choice = -1;
    private int player2Choice = -1;

    private enum PlayerTurn { Player1, Player2, ShowResult }
    private PlayerTurn currentTurn = PlayerTurn.Player1;

    public void OnChoice(int choice)
    {
        if (currentTurn == PlayerTurn.Player1)
        {
            player1Choice = choice;
            panelPlayer1.SetActive(false);
            panelPlayer2.SetActive(true);
            // ❌ ما نشغل الأنميشن هنا
            currentTurn = PlayerTurn.Player2;
        }
        else if (currentTurn == PlayerTurn.Player2)
        {
            player2Choice = choice;
            panelPlayer2.SetActive(false);
            panelResult.SetActive(true);

            // ✅ نشغل اليدين الآن معًا
            player1HandObject.SetActive(true);
            player2HandObject.SetActive(true);

            player1HandController.PlayChoice(player1Choice);
            player2HandController.PlayChoice(player2Choice);

            string result = DetermineWinner(player1Choice, player2Choice);
            resultText.text = result;

            currentTurn = PlayerTurn.ShowResult;
        }
    }


    string DetermineWinner(int p1, int p2)
    {
        if (p1 == p2) return "تعادل!";
        if ((p1 == 0 && p2 == 2) || (p1 == 1 && p2 == 0) || (p1 == 2 && p2 == 1))
            return "اللاعب الأول فاز!";
        return "اللاعب الثاني فاز!";
    }

    public void RestartGame()
    {
        player1Choice = -1;
        player2Choice = -1;
        currentTurn = PlayerTurn.Player1;

        panelResult.SetActive(false);
        panelPlayer1.SetActive(true);

        player1HandObject.SetActive(false);
        player2HandObject.SetActive(false);

        player1HandController.ResetButtons();
        player2HandController.ResetButtons();
    }
}

