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
    public void DebugClick()
    {
        Debug.Log("Next button clicked!");
    }

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
        if (resp == null || resp.scores == null) return;

        int maxToShow = 5;
        int count = Mathf.Min(resp.scores.Length, maxToShow);

        for (int i = 0; i < count; i++)
        {
            ScoreEntry entry = resp.scores[i];

            GameObject row = Instantiate(rowPrefab, rowsContainer);
            row.transform.localScale = Vector3.one;


            var rankTf = row.transform.Find("RankText");
            var nameTf = row.transform.Find("NameText");
            var timeTf = row.transform.Find("TimeText");


            var rankText = rankTf != null ? rankTf.GetComponent<TextMeshProUGUI>() : null;
            var nameText = nameTf != null ? nameTf.GetComponent<TextMeshProUGUI>() : null;
            var timeText = timeTf != null ? timeTf.GetComponent<TextMeshProUGUI>() : null;

            if (rankText != null) rankText.text = (i + 1) + ".";
            if (nameText != null) nameText.text = entry.playerName;
            if (timeText != null) timeText.text = entry.timeSec.ToString("F3") + "s";
        }
    }


}
