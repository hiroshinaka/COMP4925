using UnityEngine;
using UnityEngine.SceneManagement;

public class GateTrigger : MonoBehaviour
{
    [SerializeField] private int levelIdOverride = -1;
    [SerializeField] private string nextSceneName; // set in Inspector

    private bool isTimerRunning = false;


    private int GetLevelId()
    {
        if (levelIdOverride > 0)
            return levelIdOverride;

        return SceneManager.GetActiveScene().buildIndex;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (!isTimerRunning)
        {
            if (CompareTag("StartGate"))
            {
                RunTimer.Instance.StartTimer();
                isTimerRunning = true;
            }
            else if (CompareTag("EndGate"))
            {
                Debug.Log("EndGate triggered");

                if (RunTimer.Instance == null)
                    Debug.LogError("RunTimer.Instance is NULL");
                if (LeaderboardAPI.Instance == null)
                    Debug.LogError("LeaderboardAPI.Instance is NULL");
                if (LeaderboardUI.Instance == null)
                    Debug.LogError("LeaderboardUI.Instance is NULL");

                RunTimer.Instance.StopTimer();

                float time = RunTimer.Instance.currentTime;
                int levelId = GetLevelId();

                LeaderboardAPI.Instance.SubmitScore(levelId, time);

                LeaderboardUI.Instance.ShowLeaderboard(levelId, time, () =>
                {
                    if (!string.IsNullOrEmpty(nextSceneName))
                        SceneManager.LoadScene(nextSceneName);
                });
            }



        }

    }
}
