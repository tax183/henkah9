using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfileButtonManager : MonoBehaviour
{
    public string profileSceneName = "ProfileScene"; // ���� �� ��� ������ ������

    public void OpenProfileScene()
    {
        SceneManager.LoadScene(profileSceneName);
    }
}
