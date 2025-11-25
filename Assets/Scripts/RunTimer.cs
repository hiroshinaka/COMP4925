using UnityEngine;

public class RunTimer : MonoBehaviour
{
    public static RunTimer Instance;

    public float currentTime = 0f;
    public bool timerRunning = false;

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

    private void Update()
    {
        if (timerRunning)
            currentTime += Time.deltaTime;
    }

    public void StartTimer()
    {
        currentTime = 0f;
        timerRunning = true;
        Debug.Log("Timer Started");
    }

    public void StopTimer()
    {
        timerRunning = false;
        Debug.Log("Timer Ended at: " + currentTime);
    }
}
