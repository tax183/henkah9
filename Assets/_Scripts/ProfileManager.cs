using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using SFB; // Standalone File Browser

public class ProfileManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField idInputField;
    [SerializeField] private Button editNameButton;
    [SerializeField] private Button editIDButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button changeProfilePicButton;
    [SerializeField] private Image profileImage;

    private bool isNameEditing = false;
    private bool isIDEditing = false;
    private string selectedImagePath = null; // لحفظ مسار الصورة المختارة
    [SerializeField] private GameObject goToGameButton;

    async void Start()
    {
        await InitializeUnityServices();
        await LoadDataFromCloud();
        LoadDefaultProfileImage();
        nameInputField.interactable = false;
        idInputField.interactable = false;

        editNameButton.onClick.AddListener(ToggleNameEdit);
        editIDButton.onClick.AddListener(ToggleIDEdit);
        saveButton.onClick.AddListener(OnClick_Save);
        changeProfilePicButton.onClick.AddListener(ChangeProfilePicture);
        Invoke("LoadDefaultProfileImage", 0.1f); // تأخير تحميل الصورة الافتراضية قليلاً
        // تحميل الصورة من السحابة أو استخدام الافتراضية
        await LoadProfileImageFromCloud();
    }

    private void LoadDefaultProfileImage()
    {
        if (profileImage == null)
        {
            Debug.LogError("❌ profileImage غير مضبوط في Inspector!");
            return;
        }

        Sprite defaultSprite = Resources.Load<Sprite>("blankprofileimg");
        if (defaultSprite != null)
        {
            profileImage.sprite = defaultSprite;
            Debug.Log("✅ تم تحميل الصورة الافتراضية بنجاح.");
        }
        else
        {
            Debug.LogWarning("⚠️ لم يتم العثور على الصورة الافتراضية! تأكد من أنها موجودة في Resources.");
        }
    }
    private async Task InitializeUnityServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task LoadDataFromCloud()
    {
        string fallbackID = AuthenticationService.Instance.PlayerId;
        var keys = new HashSet<string> { "PlayerName", "PlayerID" };
        var savedData = await CloudSaveService.Instance.Data.LoadAsync(keys);

        nameInputField.text = savedData.TryGetValue("PlayerName", out var nameValue) ? nameValue : "Player";
        idInputField.text = savedData.TryGetValue("PlayerID", out var idValue) ? idValue : fallbackID;
    }

    private async Task LoadProfileImageFromCloud()
    {
        var keys = new HashSet<string> { "ProfileImage" };
        var savedData = await CloudSaveService.Instance.Data.LoadAsync(keys);

        if (savedData.TryGetValue("ProfileImage", out var imageData))
        {
            byte[] imageBytes = System.Convert.FromBase64String(imageData);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageBytes);
            profileImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            LoadDefaultProfileImage(); // تحميل الصورة الافتراضية
        }
    }

   

    private void ToggleNameEdit()
    {
        isNameEditing = !isNameEditing;
        nameInputField.interactable = isNameEditing;
    }

    private void ToggleIDEdit()
    {
        isIDEditing = !isIDEditing;
        idInputField.interactable = isIDEditing;
    }

    public void OnClick_Save()
    {
        _ = SaveProfileToCloud().ContinueWith(_ =>
        {
            Debug.Log("✅ تم حفظ جميع البيانات في الكلاود، ويتم تحديث الصورة الآن...");
            LoadProfileImageFromCloud(); // تحديث الصورة مباشرة بعد الحفظ
        });

    }
   
    public void GoToGamesPage()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Games page");
    }

    private async Task SaveProfileToCloud()
    {
        string fixedName = Regex.Replace(nameInputField.text.Trim(), "[^a-zA-Z0-9 _-]", "");
        string fixedID = idInputField.text.Trim();

        var dataToSave = new Dictionary<string, object>
        {
            { "PlayerName", fixedName },
            { "PlayerID", fixedID }
        };

        // حفظ الصورة الجديدة إن وجدت
        if (selectedImagePath != null && File.Exists(selectedImagePath))
        {
            byte[] imageBytes = File.ReadAllBytes(selectedImagePath);
            string base64Image = System.Convert.ToBase64String(imageBytes);
            dataToSave["ProfileImage"] = base64Image;
        }
        else if (profileImage.sprite != null) // إذا لم يغير الصورة، نحفظ الموجودة
        {
            Texture2D tex = profileImage.sprite.texture;
            byte[] imageBytes = tex.EncodeToPNG();
            string base64Image = System.Convert.ToBase64String(imageBytes);
            dataToSave["ProfileImage"] = base64Image;
        }

        nameInputField.interactable = false;
        idInputField.interactable = false;
        isNameEditing = false;
        isIDEditing = false;

        await CloudSaveService.Instance.Data.ForceSaveAsync(dataToSave);
        Debug.Log("✅ تم حفظ جميع البيانات في الكلاود!");
    }

    public void ChangeProfilePicture()
    {
        var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Profile Picture", "", extensions, false);

        if (paths.Length > 0 && File.Exists(paths[0]))
        {
            selectedImagePath = paths[0];
            byte[] imageBytes = File.ReadAllBytes(selectedImagePath);

            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageBytes);

            tex = MakeTextureCircular(tex); // تحويل الصورة إلى شكل دائري

            profileImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            Debug.Log("✅ تم تغيير صورة الملف الشخصي!");
        }
        else
        {
            Debug.LogWarning("⚠️ لم يتم اختيار صورة صحيحة!");
        }
    }
    private Texture2D MakeTextureCircular(Texture2D sourceTex)
    {
        int width = sourceTex.width;
        int height = sourceTex.height;
        Texture2D circularTex = new Texture2D(width, height);

        Color clear = new Color(0, 0, 0, 0); // لون شفاف
        Vector2 center = new Vector2(width / 2, height / 2);
        float radius = width / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist > radius)
                {
                    circularTex.SetPixel(x, y, clear);
                }
                else
                {
                    circularTex.SetPixel(x, y, sourceTex.GetPixel(x, y));
                }
            }
        }

        circularTex.Apply();
        return circularTex;
    }


}
