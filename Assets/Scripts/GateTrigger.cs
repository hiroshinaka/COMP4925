using UnityEngine;
using UnityEngine.SceneManagement;

public class GateTrigger : MonoBehaviour
{
    [SerializeField] private int levelIdOverride = -1;
    [SerializeField] private string nextSceneName; // set in Inspector

    private int GetLevelId()
    {
        if (levelIdOverride > 0)
            return levelIdOverride;

        return SceneManager.GetActiveScene().buildIndex;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (CompareTag("StartGate"))
        {
            RunTimer.Instance.StartTimer();
        }
        else if (CompareTag("EndGate"))
        {
            RunTimer.Instance.StopTimer();

            float time = RunTimer.Instance.currentTime;
            int levelId = GetLevelId();

            // Submit score to backend
            LeaderboardAPI.Instance.SubmitScore(levelId, time);

            // Show leaderboard UI and only change scene after player continues
            LeaderboardUI.Instance.ShowLeaderboard(levelId, time, () =>
            {
                if (!string.IsNullOrEmpty(nextSceneName))
                    SceneManager.LoadScene(nextSceneName);
            });
        }
    }
}
