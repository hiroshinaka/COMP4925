using UnityEngine;
using UnityEngine.SceneManagement;

public class GateTrigger : MonoBehaviour
{
    [SerializeField] private int levelIdOverride = -1;
    [SerializeField] private string nextSceneName; // set in Inspector
    [SerializeField] private ParticleSystem confettiFX;

    private bool startTriggered = false;
    private bool endTriggered = false;


    private int GetLevelId()
    {
        if (levelIdOverride > 0)
            return levelIdOverride;

        return SceneManager.GetActiveScene().buildIndex;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // START GATE
        if (CompareTag("StartGate"))
        {
            if (startTriggered) return;
            startTriggered = true;

            if (RunTimer.Instance != null)
            {
                RunTimer.Instance.StartTimer();
                Debug.Log("Timer Started");
            }
            else
            {
                Debug.LogError("RunTimer.Instance is NULL");
            }

            return;
        }

        // END GATE
        if (CompareTag("EndGate"))
        {
            if (endTriggered) return;
            endTriggered = true;

            Debug.Log("EndGate triggered");

            // Play confetti if assigned
            if (confettiFX != null)
            {
                confettiFX.transform.position = transform.position; // optional
                confettiFX.Play();
            }
            else
            {
                Debug.LogWarning("ConfettiFX is not assigned on GateTrigger", this);
            }

            // Make sure singletons exist before using them
            if (RunTimer.Instance == null ||
                LeaderboardAPI.Instance == null ||
                LeaderboardUI.Instance == null)
            {
                if (RunTimer.Instance == null)
                    Debug.LogError("RunTimer.Instance is NULL");
                if (LeaderboardAPI.Instance == null)
                    Debug.LogError("LeaderboardAPI.Instance is NULL");
                if (LeaderboardUI.Instance == null)
                    Debug.LogError("LeaderboardUI.Instance is NULL");
                return; // avoid NullReferenceException
            }

            RunTimer.Instance.StopTimer();

            float time = RunTimer.Instance.currentTime;
            int levelId = GetLevelId();

            // This is where your SSL error is coming from if the backend isn’t reachable
            LeaderboardAPI.Instance.SubmitScore(levelId, time);

            LeaderboardUI.Instance.ShowLeaderboard(levelId, time, () =>
            {
                if (!string.IsNullOrEmpty(nextSceneName))
                    SceneManager.LoadScene(nextSceneName);
            });
        }
    }
}