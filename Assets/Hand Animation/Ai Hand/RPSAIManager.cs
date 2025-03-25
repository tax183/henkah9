using UnityEngine;

public class RPSAIManager : MonoBehaviour
{
    public GameObject playerHandObject;
    public GameObject aiHandObject;

    public HumanHandController playerHandController;
    public HumanHandController aiHandController;

    public GameObject panelPlayer;
    public GameObject panelResult;
    public RPSAIResultHandler resultHandler; // ✅ ربط مع سكربت النتيجة

    private int playerChoice = -1;
    private int aiChoice = -1;

    public void OnPlayerChoice(int choice)
    {
        playerChoice = choice;
        panelPlayer.SetActive(false); // ❌ نخفي خيارات اللاعب

        // ✅ ننتظر 1.5 ثانية ثم نشغل AI
        Invoke(nameof(AIChoose), 1.5f);
    }

    private void AIChoose()
    {
        aiChoice = RPSAIController.Instance.GetAIChoice();
        PlayAnimationsAndShowResult();
    }

    private void PlayAnimationsAndShowResult()
    {
        panelResult.SetActive(true);

        // ✅ تشغيل اليدين
        playerHandObject.SetActive(true);
        aiHandObject.SetActive(true);

        playerHandController.PlayChoice(playerChoice);
        aiHandController.PlayChoice(aiChoice);

        // ✅ إرسال النتيجة لسكربت المسؤول عن عرضها
        resultHandler.HandleResult(playerChoice, aiChoice);
    }
}

