using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    public static LeaderboardUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform rowsContainer;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Button nextButton;

    private Action _onDone;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnNextClicked()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        _onDone?.Invoke();
    }

    /// <summary>
    /// Show leaderboard for this level, then call onDone when player clicks Next.
    /// </summary>
    public void ShowLeaderboard(int levelId, float finalTime, Action onDone)
    {
        _onDone = onDone;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (titleText != null)
            titleText.text = $"Level {levelId} – Leaderboard\nYour time: {finalTime:F3}s";

        // Clear old rows
        foreach (Transform child in rowsContainer)
            Destroy(child.gameObject);

        // Fetch data from backend
        LeaderboardAPI.Instance.GetLeaderboard(levelId, OnLeaderboardLoaded);
    }

    private void OnLeaderboardLoaded(LeaderboardResponse resp)
    {
        if (resp == null || resp.scores == null)
        {
            Debug.LogWarning("No leaderboard data received");
            return;
        }

        for (int i = 0; i < resp.scores.Length; i++)
        {
            ScoreEntry entry = resp.scores[i];

            GameObject rowGO = Instantiate(rowPrefab, rowsContainer);
            TextMeshProUGUI[] texts = rowGO.GetComponentsInChildren<TextMeshProUGUI>();

            // Expecting 3 texts: Rank, Name, Time
            if (texts.Length >= 3)
            {
                texts[0].text = (i + 1).ToString() + ".";
                texts[1].text = entry.playerName;
                texts[2].text = $"{entry.timeSec:F3}s";
            }
        }
    }
}
