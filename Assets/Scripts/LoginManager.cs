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

    // Optional: cookie if we ever get sessions working
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

    private void Awake()
    {
        // Simple singleton so other scripts can do LoginManager.Instance.CurrentUsername
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

    void Start()
    {
        // Disable game buttons until logged in
        startButton.SetActive(false);
        resumeButton.SetActive(false);
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

        string saveKey = SaveKeyPrefix + CurrentUsername;
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();

        SceneManager.LoadScene("1"); // your game scene name
    }

    public void OnResumeGameClicked()
    {
        if (string.IsNullOrEmpty(CurrentUsername))
        {
            Debug.LogWarning("Tried to resume game without login.");
            return;
        }

        SceneManager.LoadScene("1");
    }

    // ========================
    // COROUTINES: BACKEND
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

                    CaptureSessionCookie(www); // optional now

                    Debug.Log("Login successful for user: " + username);
                    OnAuthSuccess(username);
                }
                else
                {
                    Debug.LogWarning($"Login failed with status {www.responseCode}: {www.downloadHandler.text}");
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
        bool hasSave = PlayerPrefs.HasKey(saveKey);

        startButton.SetActive(true);
        resumeButton.SetActive(hasSave);
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
            Debug.LogWarning("Login succeeded but no Set-Cookie header was found.");
        }
    }
}
