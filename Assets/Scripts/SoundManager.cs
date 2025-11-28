using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    private AudioSource source;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySound(AudioClip _sound)
    {
        source.PlayOneShot(_sound);
    }

    private void Awake()
    {
        instance = this;
        source = GetComponent<AudioSource>();
    }
}
