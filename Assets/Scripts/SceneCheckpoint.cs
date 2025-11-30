using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class SceneCheckpoint : MonoBehaviour
{
    [Tooltip("Base URL of your API, same as LoginManager")]
    public string apiBaseUrl = "https://unity-backend-wdzk.onrender.com";

    [System.Serializable]
    private class CheckpointPayload
    {
        public string username;
        public string lastScene;
    }

    void Start()
    {
        var lm = LoginManager.Instance;
        if (lm == null || string.IsNullOrEmpty(lm.CurrentUsername))
        {
            Debug.Log("SceneCheckpoint: no logged-in user, skipping checkpoint update.");
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"SceneCheckpoint: saving checkpoint username={lm.CurrentUsername}, scene={sceneName}");
        StartCoroutine(SendCheckpoint(lm.CurrentUsername, sceneName));
    }

    private IEnumerator SendCheckpoint(string username, string sceneName)
    {
        string url = apiBaseUrl + "/api/game/checkpoint";

        var payload = new CheckpointPayload
        {
            username = username,
            lastScene = sceneName
        };

        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            // Optional: forward session cookie, harmless if empty
            if (!string.IsNullOrEmpty(LoginManager.SessionCookie))
            {
                www.SetRequestHeader("Cookie", LoginManager.SessionCookie);
            }

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("SceneCheckpoint error: " + www.error);
                Debug.LogWarning("Body: " + www.downloadHandler.text);
            }
            else
            {
                Debug.Log("SceneCheckpoint saved: " + www.downloadHandler.text);
            }
        }
    }
}
