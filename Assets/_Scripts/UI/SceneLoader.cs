using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // ✅ دالة تشغيل `um9 menu page` عند الضغط على `Basic Game`
    public void LoadUm9MenuPage()
    {
        SceneManager.LoadScene("um9 menu page");
    }

    // ✅ دالة تشغيل `Main` عند الضغط على `Start Game`
    public void LoadMainScene()
    {
        SceneManager.LoadScene("Main");
    }
}
