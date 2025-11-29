using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;


public class LeaderboardAPI : MonoBehaviour
{
    public static LeaderboardAPI Instance;

    [SerializeField] private string baseUrl = "https://unity-backend-wdzk.onrender.com";

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

    public void SubmitScore(int levelId, float timeSec)
    {
        StartCoroutine(SubmitRoutine(levelId, timeSec));
    }

    // Submit score coroutine
    private IEnumerator SubmitRoutine(int levelId, float timeSec)
    {
        ScoreSubmitData data = new ScoreSubmitData
        {
            timeSec = timeSec,
            username = LoginManager.Instance.CurrentUsername  // or however you store it
        };

        string json = JsonUtility.ToJson(data);
        string url = baseUrl + "/api/leaderboard/" + levelId;

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            // You can STILL send the cookie if you want, but it’s no longer required.
            if (!string.IsNullOrEmpty(LoginManager.SessionCookie))
            {
                req.SetRequestHeader("Cookie", LoginManager.SessionCookie);
            }

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"SubmitScore error: HTTP {req.responseCode} - {req.error} - {req.downloadHandler.text}"
                );
            }
            else
            {
                Debug.Log("Run stored: " + req.downloadHandler.text);
            }
        }
    }




    public void GetLeaderboard(int levelId, System.Action<LeaderboardResponse> onResult)
    {
        StartCoroutine(GetLeaderboardRoutine(levelId, onResult));
    }


    private IEnumerator GetLeaderboardRoutine(int levelId, System.Action<LeaderboardResponse> onResult)
    {
        string url = baseUrl + "/api/leaderboard/" + levelId;

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.downloadHandler = new DownloadHandlerBuffer();

            if (!string.IsNullOrEmpty(LoginManager.SessionCookie))
            {
                req.SetRequestHeader("Cookie", LoginManager.SessionCookie);
            }
            else
            {
                Debug.LogWarning("GetLeaderboardRoutine: no SessionCookie set.");
            }

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"Leaderboard error: HTTP {req.responseCode} - {req.error} - {req.downloadHandler.text}"
                );
                onResult?.Invoke(null);
            }
            else
            {
                var json = req.downloadHandler.text;
                var resp = JsonUtility.FromJson<LeaderboardResponse>(json);
                onResult?.Invoke(resp);
            }
        }

    }
}
