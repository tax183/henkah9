using UnityEngine;
using UnityEngine.SceneManagement;

public class Leaderboard : MonoBehaviour
{
    // ���� ������ ���� ��� Leaderboard
    public void LoadLeaderboardScene()
    {
        SceneManager.LoadScene("LeaderBoard");  // ���� �� �� ��� ������ �� "Leaderboard"
    }
}
