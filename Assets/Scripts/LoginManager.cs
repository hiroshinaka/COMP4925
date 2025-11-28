using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;

public class LoginManager : MonoBehaviour
{
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

    private string currentUser = "";

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

    void Start()
    {
        // Disable game buttons until logged in
        startButton.SetActive(false);
        resumeButton.SetActive(false);
    }

    // ========================
    // UI HOOKS
    // ========================

    // Hook this to Login Button OnClick
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

    // Hook this to Signup Button OnClick
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

    // Hook this to Start New Game Button OnClick
    public void OnStartNewGameClicked()
    {
        if (string.IsNullOrEmpty(currentUser))
        {
            Debug.LogWarning("Tried to start game without login.");
            return;
        }

        // Clear any old save
        string saveKey = SaveKeyPrefix + currentUser;
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();

        SceneManager.LoadScene("1"); // rename to your actual game scene name
    }

    // Hook this to Resume Game Button OnClick
    public void OnResumeGameClicked()
    {
        if (string.IsNullOrEmpty(currentUser))
        {
            Debug.LogWarning("Tried to resume game without login.");
            return;
        }

        // Later you can load actual save data here
        SceneManager.LoadScene("1");
    }

    // ========================
    // COROUTINES: TALK TO BACKEND
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

                // Treat 2xx as success
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
        currentUser = username;

        string saveKey = SaveKeyPrefix + username;
        bool hasSave = PlayerPrefs.HasKey(saveKey);

        startButton.SetActive(true);
        resumeButton.SetActive(hasSave);
    }
}
