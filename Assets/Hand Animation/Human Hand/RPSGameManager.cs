﻿using UnityEngine;
using UnityEngine.UI;

public class RPSGameManager : MonoBehaviour
{
    public static int whoStarts = 1;

    public GameObject rpsPanel; // بانل حجرة ورقة مقص
    public GameObject boardPanel; // بانل البورد التراثية

    public HumanHandController player1HandController;
    public HumanHandController player2HandController;

    public GameObject player1HandObject;
    public GameObject player2HandObject;

    public GameObject panelPlayer1;
    public GameObject panelPlayer2;
    public GameObject panelResult;
    public Text resultText;
    public GameObject restartButton;


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

            if (result == "تعادل!")
            {
                // ❌ نخفي زر الإعادة والنص أولًا قبل تشغيل الأنميشن
                restartButton.SetActive(false);
                resultText.text = ""; // نخفي النص

                // ✅ تشغيل الأنميشن
                player1HandObject.SetActive(true);
                player2HandObject.SetActive(true);

                player1HandController.PlayChoice(player1Choice);
                player2HandController.PlayChoice(player2Choice);

                // ✅ بعد 4 ثانية (وقت كافي للأنميشن) نظهر زر الإعادة والنص
                Invoke(nameof(ShowResultTextAndRestartButton), 4f);
            }


            else
            {
                restartButton.SetActive(false);
                resultText.text = ""; // ❌ نخفي النص مؤقتًا

                // ✅ نشغل الأنميشن أولًا
                player1HandObject.SetActive(true);
                player2HandObject.SetActive(true);

                player1HandController.PlayChoice(player1Choice);
                player2HandController.PlayChoice(player2Choice);

                // ✅ بعد 4 ثواني، نظهر تكست الفائز
                Invoke(nameof(ShowWinnerText), 4f);

                // ✅ بعد 4.5 ثانية (وقت كافي للأنميشن) يتم الانتقال للبورد
                Invoke(nameof(TransitionToBoard), 4.5f);

                currentTurn = PlayerTurn.ShowResult;
            }

            currentTurn = PlayerTurn.ShowResult;
        }
    }

    private void ShowRestartButton()
    {
        restartButton.SetActive(true);
    }

    private void ShowResultTextAndRestartButton()
    {
        resultText.text = "تعادل!"; // إظهار النص
        restartButton.SetActive(true); // إظهار زر الإعادة
    }

    private void ShowWinnerText()
    {
        resultText.text = DetermineWinner(player1Choice, player2Choice);
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

        player1HandObject.SetActive(true);
        player2HandObject.SetActive(true);

        player1HandController.ResetButtons();
        player2HandController.ResetButtons();

        restartButton.SetActive(false);

    }

    // ✅ دالة الانتقال بعد انتهاء الأنميشن
    private void TransitionToBoard()
    {
        // ✅ نخفي RPS ونظهر البورد
        rpsPanel.SetActive(false);
        boardPanel.SetActive(true);

        // ✅ تحديد من الفائز ليبدأ أولًا
        RPSGameManager.whoStarts = resultText.text == "اللاعب الأول فاز!" ? 1 : 2;

        // ✅ نبدأ اللعبة
        GameObject.Find("GameUIController").GetComponent<GameUIController>().StartGame();
    }
}

