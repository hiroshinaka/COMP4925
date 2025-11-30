using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    [Header("Input Fields")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    [Header("Error UI")]
    public TMP_Text errorText;

    [Header("Buttons")]
    public GameObject startButton;   // Start New Game
    public GameObject resumeButton;  // Resume Game

    [Header("Backend")]
    [Tooltip("Base URL of your API, no trailing slash")]
    public string apiBaseUrl = "https://unity-backend-wdzk.onrender.com";

    private const string SaveKeyPrefix = "SaveData_";

    // Current logged-in user
    public string CurrentUsername { get; private set; }

    // Optional: cookie if we ever use sessions from non-WebGL
    public static string SessionCookie { get; private set; }

    [System.Serializable]
    private class AuthPayload
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    private class AuthResponse
    {
        public string message;
        public string username;
        public string error;
    }

    [System.Serializable]
    private class GameStateDto
    {
        public string username;
        public int level;
        public int coins;
        public string lastScene;
    }

    private GameStateDto currentGameState;

    private void Awake()
{
    if (Instance == null)
    {
        // First time: keep this instance across scenes
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    else if (Instance != this)
    {
        // We're loading landingScene again and a new LoginManager was created.
        // Kill the old one and let this new one be the active singleton,
        // so all UI button references stay valid.
        Destroy(Instance.gameObject);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
    void Start()
    {
        if (startButton != null) startButton.SetActive(false);
        if (resumeButton != null) resumeButton.SetActive(false);
        if (errorText != null) errorText.text = "";
    }

    // ========================
    // UI HOOKS
    // ========================

    public void OnLoginClicked()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            if (errorText != null)
                errorText.text = "Please enter username and password.";
            Debug.LogWarning("Username or password is empty.");
            return;
        }

        StartCoroutine(LoginCoroutine(username, password));
    }

    public void OnSignupClicked()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            if (errorText != null)
                errorText.text = "Please enter username and password.";
            Debug.LogWarning("Username or password is empty.");
            return;
        }

        StartCoroutine(SignupCoroutine(username, password));
    }

    public void OnStartNewGameClicked()
    {
        if (string.IsNullOrEmpty(CurrentUsername))
        {
            Debug.LogWarning("Tried to start game without login.");
            return;
        }

        // Old local behaviour: clear any local save
        string saveKey = SaveKeyPrefix + CurrentUsername;
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();

        // NEW: also tell backend that checkpoint is scene "1"
        StartCoroutine(SetCheckpointAndLoadCoroutine("1"));
    }

    public void OnResumeGameClicked()
    {
    if (string.IsNullOrEmpty(CurrentUsername))
    {
        Debug.LogWarning("Tried to resume game without login.");
        return;
    }

    // Always fetch the latest state from backend before resuming
    StartCoroutine(ResumeFromServerCoroutine());
    }

    // ========================
    // COROUTINES: AUTH
    // ========================

    private IEnumerator SignupCoroutine(string username, string password)
    {
        string url = apiBaseUrl + "/api/auth/signup";

        AuthPayload payload = new AuthPayload { username = username, password = password };
        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Signup network error: {www.error}");
                Debug.LogError("Response: " + www.downloadHandler.text);
                if (errorText != null)
                    errorText.text = "Network error. Please try again.";
            }
            else
            {
                var responseText = www.downloadHandler.text;
                Debug.Log("Signup response: " + responseText);

                if (www.responseCode >= 200 && www.responseCode < 300)
                {
                    AuthResponse res = null;
                    try
                    {
                        res = JsonUtility.FromJson<AuthResponse>(responseText);
                    }
                    catch { }

                    CaptureSessionCookie(www); // harmless if no Set-Cookie

                    Debug.Log("Signup successful for user: " + username);
                    OnAuthSuccess(username);
                }
                else
                {
                    Debug.LogWarning($"Signup failed with status {www.responseCode}: {www.downloadHandler.text}");
                    if (errorText != null)
                        errorText.text = "Signup failed. Check username/password.";
                }
            }
        }
    }

    private IEnumerator LoginCoroutine(string username, string password)
    {
        string url = apiBaseUrl + "/api/auth/login";

        AuthPayload payload = new AuthPayload { username = username, password = password };
        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Login network error: {www.error}");
                Debug.LogError("Response: " + www.downloadHandler.text);
                if (errorText != null)
                    errorText.text = "Network error. Please try again.";
            }
            else
            {
                var responseText = www.downloadHandler.text;
                Debug.Log("Login response: " + responseText);

                if (www.responseCode >= 200 && www.responseCode < 300)
                {
                    AuthResponse res = null;
                    try
                    {
                        res = JsonUtility.FromJson<AuthResponse>(responseText);
                    }
                    catch { }

                    CaptureSessionCookie(www);

                    Debug.Log("Login successful for user: " + username);
                    OnAuthSuccess(username);
                }
                else
                {
                    Debug.LogWarning($"Login failed with status {www.responseCode}: {www.downloadHandler.text}");
                    if (errorText != null)
                        errorText.text = "Login failed. Check username/password.";
                }
            }
        }
    }

    // ========================
    // POST-AUTH SETUP
    // ========================

    private void OnAuthSuccess(string username)
    {
        CurrentUsername = username;

        string saveKey = SaveKeyPrefix + username;
        bool hasLocalSave = PlayerPrefs.HasKey(saveKey);

        PlayerPrefs.SetString("LoggedInUser", username);
        PlayerPrefs.Save();

        if (errorText != null)
            errorText.text = "";

        if (startButton != null) startButton.SetActive(false);
        if (resumeButton != null) resumeButton.SetActive(false);

        StartCoroutine(FetchGameStateCoroutine(hasLocalSave));
    }

    private IEnumerator FetchGameStateCoroutine(bool hasLocalSave)
    {
        if (string.IsNullOrEmpty(CurrentUsername))
        {
            if (startButton != null) startButton.SetActive(true);
            if (resumeButton != null) resumeButton.SetActive(false);
            yield break;
        }

        // NEW: send username as query, so we don't depend on cookies
        string url = apiBaseUrl + "/api/game/state?username=" + UnityWebRequest.EscapeURL(CurrentUsername);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            ApplySessionCookie(www); // harmless now but fine

            yield return www.SendWebRequest();

            bool hasRemoteSave = false;

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("FetchGameState error: " + www.error);
                Debug.LogWarning("Body: " + www.downloadHandler.text);
                hasRemoteSave = false;
            }
            else
            {
                string json = www.downloadHandler.text;
                Debug.Log("Game state response: " + json);

                GameStateDto dto = null;
                try
                {
                    dto = JsonUtility.FromJson<GameStateDto>(json);
                }
                catch
                {
                    Debug.LogWarning("Could not parse GameStateDto.");
                }

                currentGameState = dto;
                hasRemoteSave = dto != null && !string.IsNullOrEmpty(dto.lastScene);
            }

            if (startButton != null) startButton.SetActive(true);
            if (resumeButton != null) resumeButton.SetActive(hasLocalSave || hasRemoteSave);
        }
    }

    private IEnumerator SetCheckpointAndLoadCoroutine(string sceneName)
    {
        if (string.IsNullOrEmpty(CurrentUsername))
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        string url = apiBaseUrl + "/api/game/checkpoint";

        var payloadObj = new GameStateDto
        {
            username = CurrentUsername,
            lastScene = sceneName
        };

        string json = JsonUtility.ToJson(payloadObj);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            ApplySessionCookie(www);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("SetCheckpoint error: " + www.error);
                Debug.LogWarning("Body: " + www.downloadHandler.text);
                SceneManager.LoadScene(sceneName); // fallback
            }
            else
            {
                Debug.Log("Checkpoint saved: " + www.downloadHandler.text);

                try
                {
                    currentGameState = JsonUtility.FromJson<GameStateDto>(www.downloadHandler.text);
                }
                catch { }

                SceneManager.LoadScene(sceneName);
            }
        }
    }

    private IEnumerator ResumeFromServerCoroutine()
    {
    string url = apiBaseUrl + "/api/game/state?username=" + UnityWebRequest.EscapeURL(CurrentUsername);

    using (UnityWebRequest www = UnityWebRequest.Get(url))
    {
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        ApplySessionCookie(www); // harmless, even if no cookie

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("ResumeFromServer error: " + www.error);
            Debug.LogWarning("Body: " + www.downloadHandler.text);

            // Fallback: if request fails, at least load level 1 so game is playable
            SceneManager.LoadScene("1");
        }
        else
        {
            string json = www.downloadHandler.text;
            Debug.Log("ResumeFromServer response: " + json);

            GameStateDto dto = null;
            try
            {
                dto = JsonUtility.FromJson<GameStateDto>(json);
            }
            catch
            {
                Debug.LogWarning("Could not parse GameStateDto in ResumeFromServer.");
            }

            currentGameState = dto;

            string sceneToLoad = "1"; // default
            if (dto != null && !string.IsNullOrEmpty(dto.lastScene))
            {
                sceneToLoad = dto.lastScene;
            }

            Debug.Log("Resuming game at scene from server: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
    }

    // ========================
    // COOKIE HELPERS
    // ========================

    private void ApplySessionCookie(UnityWebRequest www)
    {
        if (!string.IsNullOrEmpty(SessionCookie))
        {
            www.SetRequestHeader("Cookie", SessionCookie);
        }
    }

    private void CaptureSessionCookie(UnityWebRequest www)
    {
        var headers = www.GetResponseHeaders();
        if (headers == null)
        {
            Debug.LogWarning("No response headers; cannot capture session cookie.");
            return;
        }

        foreach (var kv in headers)
        {
            Debug.Log($"Header: {kv.Key} = {kv.Value}");
        }

        foreach (var kv in headers)
        {
            if (kv.Key != null && kv.Key.ToLower() == "set-cookie")
            {
                var raw = kv.Value;
                if (string.IsNullOrEmpty(raw)) continue;

                var parts = raw.Split(';');
                if (parts.Length > 0)
                {
                    SessionCookie = parts[0].Trim();
                    Debug.Log("Stored session cookie: " + SessionCookie);
                }
            }
        }

        if (string.IsNullOrEmpty(SessionCookie))
        {
            Debug.LogWarning("Login/Signup succeeded but no Set-Cookie header was found.");
        }
    }
}
