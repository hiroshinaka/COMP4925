using UnityEngine;

public class HealthCollectible: MonoBehaviour
{
    [SerializeField] private float healthValue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private AudioClip pickupSound;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            SoundManager.instance.PlaySound(pickupSound);
            collision.GetComponent<Health>().AddHealth(healthValue);
            gameObject.SetActive(false);
        }
    }


}
