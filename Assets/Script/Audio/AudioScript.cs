using UnityEngine;

public class AudioScript : MonoBehaviour
{
    public static AudioScript instance;
    public AudioSource sfxplayer;
    public AudioClip correct;

    void Awake()
    {
        instance = this;
        if (instance != this && instance == null)
        {
            Destroy(gameObject);

        }
        else
        {
            instance = this;
        }
    }
    public void correctaudioplay()
    {
        sfxplayer.clip = correct;
        sfxplayer.Play();
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
