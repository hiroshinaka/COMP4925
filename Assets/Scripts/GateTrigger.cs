using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (CompareTag("StartGate"))
        {
            RunTimer.Instance.StartTimer();
        }
        else if (CompareTag("EndGate"))
        {
            RunTimer.Instance.StopTimer();
        }
    }
}
