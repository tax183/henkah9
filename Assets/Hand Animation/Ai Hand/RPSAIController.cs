using UnityEngine;

public class RPSAIController : MonoBehaviour
{
    public static RPSAIController Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ✅ اختيار حركة AI عشوائيًا (0 = حجر, 1 = ورقة, 2 = مقص)
    public int GetAIChoice()
    {
        int choice = Random.Range(0, 3);
        Debug.Log($"🤖 AI اختار: {(choice == 0 ? "حجر" : choice == 1 ? "ورقة" : "مقص")}");
        return choice;
    }
}
