using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private void Reset()
    {
        // Auto-fill if this script is on the same GameObject as the text
        if (timerText == null)
            timerText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (timerText == null || RunTimer.Instance == null)
            return;

        float t = RunTimer.Instance.currentTime;

        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        int milliseconds = Mathf.FloorToInt((t * 1000f) % 1000f);

        timerText.text = $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }
}
