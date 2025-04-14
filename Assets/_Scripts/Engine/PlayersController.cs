
using System.Diagnostics;
public class PlayersController
{
    private AiPlayer firstAiPlayer;
    private AiPlayer secondAiPlayer;
    private bool gameEngineReady;
    private PlayerNumber currentPlayerTurn;
    private Stopwatch stopWatch;

    public PlayersController(AiPlayer firstAiPlayer = null, AiPlayer secondAiPlayer = null)
    {
        this.firstAiPlayer = firstAiPlayer;
        this.secondAiPlayer = secondAiPlayer;
        this.currentPlayerTurn = PlayerNumber.FirstPlayer;
        this.gameEngineReady = true;
    }

    public long CheckStep()
    {
        if (!gameEngineReady) return 0;
        if (GameEngine.Instance == null || GameEngine.Instance.GameState == null) return 0;

        PlayerNumber actualTurn = GameEngine.Instance.GameState.CurrentMovingPlayer;

        if (actualTurn != currentPlayerTurn)
        {
            UnityEngine.Debug.Log("⛔ AI halted: mismatch between expected turn (" + currentPlayerTurn + ") and actual turn (" + actualTurn + ")");
            return 0;
        }

        if (GameEngine.Instance.GameState.PawnsToRemove > 0)
        {
            UnityEngine.Debug.Log("⛔ AI halted: player must remove pawn.");
            return 0;
        }

        stopWatch = Stopwatch.StartNew();

        if (currentPlayerTurn == PlayerNumber.FirstPlayer)
        {
            HandleAiMove(firstAiPlayer);
        }
        else
        {
            HandleAiMove(secondAiPlayer);
        }

        stopWatch.Stop();
        return stopWatch.ElapsedMilliseconds;
    }

    public PlayerNumber GetCurrentAiTurn()
    {
        return currentPlayerTurn;
    }

    public void OnPlayerTurnChanged(PlayerNumber playerNumber)
    {
        UnityEngine.Debug.Log("👂 AI Controller noticed turn switch to: " + playerNumber);
        this.currentPlayerTurn = playerNumber;
        this.gameEngineReady = true;
    }
    private void HandleAiMove(AiPlayer player)
    {
        gameEngineReady = false;
        if (player != null)
        {
            player.MakeMove();

            // ✅ تحديث الواجهة بعد أول حركة للذكاء الصناعي
            UnityEngine.Object.FindObjectOfType<GameUIController>().UpdateStonesUI(2);
        }
    }
}
