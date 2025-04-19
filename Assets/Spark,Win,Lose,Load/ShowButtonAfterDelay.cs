using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShowButtonAfterDelay : MonoBehaviour
{
    public GameObject targetButton;  // ���� ���� ��� ���� ��� ���
    public float delaySeconds = 2f;

    void Start()
    {
        targetButton.SetActive(false);  // ���� ���� ��� �� ����� ������
        StartCoroutine(ShowAfterDelay());
    }

    IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);
        targetButton.SetActive(true);
    }

    // ������ ��� ������ �� ����
    public void GoToGamesPage()
    {
        SceneManager.LoadScene("Games page");
    }
}
