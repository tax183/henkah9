using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private TMP_InputField lobbyCodeInput;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private TMP_Text lobbyCodeText;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject lobbyPanel;

    void Start()
    {
        createLobbyButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.CreateLobby();
            Invoke(nameof(UpdateLobbyCode), 2f); // ننتظر شوي حتى يتم إنشاء اللوبي
        });

        joinLobbyButton.onClick.AddListener(() =>
        {
            string code = lobbyCodeInput.text;
            LobbyManager.Instance.JoinLobby(code);
        });

        cancelButton.onClick.AddListener(() =>
        {
            lobbyPanel.SetActive(false); // إخفاء النافذة
        });
    }

    void UpdateLobbyCode()
    {
        if (!string.IsNullOrEmpty(LobbyManager.Instance.lobbyCode))
        {
            lobbyCodeText.text = "كود الغرفة: " + LobbyManager.Instance.lobbyCode;
        }
    }
}
