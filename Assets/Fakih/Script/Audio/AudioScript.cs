using UnityEngine;

public class AudioScript : MonoBehaviour
{
    public static AudioScript instance;
    public AudioSource sfxaudioplayer;
    public AudioClip correctaudio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (instance == null && instance != this)
        {
            instance = this;

        }
        else
        {
            Destroy(gameObject);
        }
            
        
    }
    void Start()
    {
        
    }
    public void playcorrectaudio()
    {
        sfxaudioplayer.clip = correctaudio;
        sfxaudioplayer.Play();

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
