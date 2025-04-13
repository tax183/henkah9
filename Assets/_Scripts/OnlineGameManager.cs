using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Networking.Transport.Relay;
using Unity.Networking.Transport;
using System.Threading.Tasks;
using System;
using Unity.Services.Authentication;
using System.Collections.Generic;

public class OnlineGameManager : MonoBehaviour
{
    public static OnlineGameManager Instance;

    public bool isHost = false;
    public string lobbyCode = "";
    public string joinCode = "";
    public string myChoice = "";
    public string opponentChoice = "";

    public bool isMyTurn = false;
    public bool rpsDone = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if (isHost || !string.IsNullOrEmpty(joinCode))
        {
            InvokeRepeating(nameof(CheckLobbyForMoves), 3f, 3f);

        }
    }

    public void SetMyChoice(string choice)
    {
        myChoice = choice;
        Debug.Log("🌟 اخترت: " + choice);

        // ترسل اختيارك للطرف الثاني (بعدين نفعّل الإرسال)
    }

    public void SetOpponentChoice(string choice)
    {
        opponentChoice = choice;
        Debug.Log("👤 خصمك اختار: " + choice);

        // نتحقق من الفوز لما يكون عندنا خيار الطرفين
        CheckResult();
    }

    void CheckResult()
    {
        if (string.IsNullOrEmpty(myChoice) || string.IsNullOrEmpty(opponentChoice))
            return;

        string winner = "";

        if (myChoice == opponentChoice)
        {
            winner = "draw";
        }
        else if (
            (myChoice == "rock" && opponentChoice == "scissors") ||
            (myChoice == "scissors" && opponentChoice == "paper") ||
            (myChoice == "paper" && opponentChoice == "rock")
        )
        {
            winner = "me";
        }
        else
        {
            winner = "opponent";
        }

        Debug.Log("🏁 الفائز: " + winner);

        if (winner == "me")
        {
            isMyTurn = true;
        }
        else if (winner == "opponent")
        {
            isMyTurn = false;
        }

        rpsDone = true;
        // نقدر نكمل بعدين على البورد
    }

    // ✅ أضفنا هذا:
    public int receivedFieldIndex = -1;

    private Lobby currentLobby;
    private float checkInterval = 1f;
    private float checkTimer = 0f;

    public async void SendMoveToOpponent(int fieldIndex)
    {
        if (string.IsNullOrEmpty(lobbyCode)) return;

        try
        {
            var data = new Dictionary<string, PlayerDataObject>
        {
            { "move", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, fieldIndex.ToString()) }
        };

            await LobbyService.Instance.UpdatePlayerAsync(
                lobbyCode,
                AuthenticationService.Instance.PlayerId,
                new UpdatePlayerOptions { Data = data }
            );

            Debug.Log("📤 تم إرسال الحركة إلى الخصم: " + fieldIndex);
        }
        catch (Exception e)
        {
            Debug.LogError("خطأ في إرسال الحركة: " + e.Message);
        }
    }


    private async void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            await CheckLobbyForMoves();
        }
    }

    private async Task CheckLobbyForMoves()
    {
        if (string.IsNullOrEmpty(lobbyCode)) return;

        try
        {
            currentLobby = await LobbyService.Instance.GetLobbyAsync(lobbyCode);

            foreach (var player in currentLobby.Players)
            {
                if (player.Id != AuthenticationService.Instance.PlayerId && player.Data != null && player.Data.ContainsKey("move"))
                {
                    string moveValue = player.Data["move"].Value;
                    if (!string.IsNullOrEmpty(moveValue))
                    {
                        int fieldIndex = int.Parse(moveValue);
                        Debug.Log("📥 استلام حركة من الخصم: " + fieldIndex);

                        ReceiveMoveFromOpponent(fieldIndex);

                        // 🧹 نمسح الحركة بعد تنفيذها
                        var resetOptions = new UpdatePlayerOptions
                        {
                            Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "move", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "") }
                        }
                        };

                        await LobbyService.Instance.UpdatePlayerAsync(currentLobby.Id, player.Id, resetOptions);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("🔁 خطأ في التحقق من اللوبي: " + e.Message);
        }
    }

    public void ReceiveMoveFromOpponent(int fieldIndex)
    {
        Debug.Log("📥 استلام حركة الخصم: " + fieldIndex);
        receivedFieldIndex = fieldIndex;
    }

}

