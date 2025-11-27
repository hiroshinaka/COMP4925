using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class GameStateAPI : MonoBehaviour
{
    public static GameStateAPI Instance;

    [SerializeField] private string baseUrl = "http://localhost:4000";

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

    // Save current state (e.g. last completed level)
    public void SaveGameState(int level)
    {
        StartCoroutine(SaveGameStateRoutine(level));
    }

    private IEnumerator SaveGameStateRoutine(int level)
    {
        var dto = new GameStateDto { level = level };
        string json = JsonUtility.ToJson(dto);

        string url = baseUrl + "/api/game/state";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                Debug.LogError("SaveGameState error: " + req.error);
            else
                Debug.Log("GameState saved: " + req.downloadHandler.text);
        }
    }

    // Load saved state (e.g. when user logs in)
    public void LoadGameState(Action<GameStateDto> onResult)
    {
        StartCoroutine(LoadGameStateRoutine(onResult));
    }

    private IEnumerator LoadGameStateRoutine(Action<GameStateDto> onResult)
    {
        string url = baseUrl + "/api/game/state";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.downloadHandler = new DownloadHandlerBuffer();

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("LoadGameState error: " + req.error);
                onResult?.Invoke(null);
            }
            else
            {
                string json = req.downloadHandler.text;
                GameStateDto state = JsonUtility.FromJson<GameStateDto>(json);
                onResult?.Invoke(state);
            }
        }
    }
}