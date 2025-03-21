using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System;
using System.Threading.Tasks;

public class AccountManager : MonoBehaviour
{
    private static string playerIDKey = "PlayerID";
    private static string guestKey = "IsGuest";
    private static string playerNameKey = "PlayerName";
    private static string playerScoreKey = "PlayerScore";

    void Start()
    {
        DontDestroyOnLoad(gameObject); // تأكد من بقاء هذا الكائن عبر المشاهد
        InitializeUnityServices(); // بدء خدمات يونتي
    }

    private async Task InitializeUnityServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            PlayerPrefs.SetString(playerIDKey, AuthenticationService.Instance.PlayerId);
            PlayerPrefs.Save();
        }

        // التأكد من نقل المستخدم إلى `ProfileScene` إذا كان مسجلًا
        if (PlayerPrefs.HasKey(playerIDKey) && !IsGuest())
        {
            SceneManager.LoadScene("ProfileScene");
        }
    }
    public void OpenProfileScene()
    {
        SceneManager.LoadScene("ProfileScene");
    }

    // ✅ التحقق مما إذا كان المستخدم ضيفًا
    public static bool IsGuest()
    {
        return PlayerPrefs.GetInt(guestKey, 1) == 1;
    }

    // ✅ تسجيل مستخدم جديد
    public async void Register(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.Log("❌ الرجاء إدخال اسم صحيح!");
            return;
        }

        await InitializeUnityServices();
        string playerId = AuthenticationService.Instance.PlayerId;

        PlayerPrefs.SetString(playerNameKey, playerName);
        PlayerPrefs.SetString(playerIDKey, playerId);
        PlayerPrefs.SetInt(guestKey, 0); // ليس ضيفًا
        PlayerPrefs.SetInt(playerScoreKey, 0); // سكور يبدأ من 0
        PlayerPrefs.Save();

        Debug.Log($"✅ تم إنشاء الحساب: {playerName} - ID: {playerId}");
        SceneManager.LoadScene("ProfileScene");
    }

    // ✅ تسجيل الدخول كضيف
    public void PlayAsGuest()
    {
        if (!PlayerPrefs.HasKey(playerIDKey))
        {
            string guestID = "Guest_" + Guid.NewGuid().ToString();
            PlayerPrefs.SetString(playerIDKey, guestID);
        }

        PlayerPrefs.SetInt(guestKey, 1); // المستخدم ضيف
        PlayerPrefs.Save();

        Debug.Log("🚀 تم الدخول كضيف، سيتم توجيهه إلى اللعب المحلي فقط.");
        SceneManager.LoadScene("Um9 menu page"); // تغيير المشهد إلى اللعب المحلي
    }

    // ✅ استرجاع معلومات اللاعب
    public static string GetPlayerName()
    {
        return PlayerPrefs.GetString(playerNameKey, "لاعب مجهول");
    }

    public static string GetPlayerID()
    {
        return PlayerPrefs.GetString(playerIDKey, "UnknownPlayer");
    }

    public static int GetPlayerScore()
    {
        return PlayerPrefs.GetInt(playerScoreKey, 0);
    }

    // ✅ تحديث سكور اللاعب
    public static void UpdateScore(int score)
    {
        int currentScore = PlayerPrefs.GetInt(playerScoreKey, 0);
        currentScore += score;
        PlayerPrefs.SetInt(playerScoreKey, currentScore);
        PlayerPrefs.Save();
    }

}

