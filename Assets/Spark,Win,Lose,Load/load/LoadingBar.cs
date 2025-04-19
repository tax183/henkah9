using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // �� ������ ����� ����

public class LoadingBar : MonoBehaviour
{
    public Slider slider;
    public float loadingSpeed = 0.3f;
    public string nextSceneName = "YourNextScene"; // ����� �� ������

    void Start()
    {
        slider.value = 0f;
    }

    void Update()
    {
        if (slider.value < 1f)
        {
            slider.value += loadingSpeed * Time.deltaTime;
        }
        else
        {
            // ��� ���� ����� ��� �� ���� �������
            // SceneManager.LoadScene(nextSceneName);
            Debug.Log("����� �����!");
        }
    }
}
