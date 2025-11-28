using UnityEngine;

public class DeathBarrier : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the thing that hit the barrier has a Health component
        Health health = collision.GetComponent<Health>();
        if (health != null)
        {
            health.Kill();          // triggers die anim + respawn via Health script
        }

    }
}
