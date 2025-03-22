using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.IO;
using System.Threading.Tasks;
using TMPro; // استيراد مكتبة TextMeshPro

public class ProfileManager : MonoBehaviour
{
    [SerializeField] private Image profileImage;
    [SerializeField] private TMP_InputField nameInputField; // تغيير إلى TMP_InputField
    [SerializeField] private TMP_Text playerIDText; // تغيير إلى TMP_Text
    [SerializeField] private TMP_Text scoreText; // تغيير إلى TMP_Text
    [SerializeField] private Button changeProfilePicButton;
    [SerializeField] private Button saveProfileButton;
    [SerializeField] private Button logoutButton;

    private string profileImageKey = "ProfileImage";
    private static string playerScoreKey = "PlayerScore";

    async void Start()
    {
        await InitializeUnityServices();

        nameInputField.text = PlayerPrefs.GetString("PlayerName", "Type your name:");
        playerIDText.text = "ID: " + PlayerPrefs.GetString("PlayerID", "UnknownPlayer");
        scoreText.text = "⭐ " + PlayerPrefs.GetInt(playerScoreKey, 0);

        LoadProfileImage();

        changeProfilePicButton.onClick.AddListener(ChangeProfilePicture);
        saveProfileButton.onClick.AddListener(SaveProfile);
        logoutButton.onClick.AddListener(Logout);
    }

    private async Task InitializeUnityServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }
    }

    private void LoadProfileImage()
    {
        string imagePath = PlayerPrefs.GetString(profileImageKey, "");

        if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageBytes);
            profileImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Sprite defaultSprite = Resources.Load<Sprite>("DefaultProfile");
            if (defaultSprite != null)
            {
                profileImage.sprite = defaultSprite;
            }
        }
    }

    public void ChangeProfilePicture()
    {
        Debug.Log("🚀 ميزة تغيير الصورة لم تُفعّل بعد.");
    }

    public void SaveProfile()
    {
        PlayerPrefs.SetString("PlayerName", nameInputField.text);
        PlayerPrefs.Save();
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("PlayerID");
        PlayerPrefs.DeleteKey("PlayerScore");
        PlayerPrefs.DeleteKey(profileImageKey);
        PlayerPrefs.Save();

        SceneManager.LoadScene("LoginScene");
    }
}

