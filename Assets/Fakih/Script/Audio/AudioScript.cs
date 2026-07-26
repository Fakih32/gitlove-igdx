using UnityEngine;

public class AudioScript : MonoBehaviour
{
    public static AudioScript instance;
    [Header("Audio Source")]
    public AudioSource sfxaudioplayer;
    public AudioSource Bgmaudioplayer;
    [Header("Audio Clip")]
    [Header("Sfx")]
    public AudioClip correctaudio;
    public AudioClip Dragingup;
    public AudioClip Dropingdown;
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
    public void Playsfx(AudioClip Soundeffect)
    {
        sfxaudioplayer.clip = Soundeffect;
        sfxaudioplayer.Play();

    }
    public void PlayBGM(AudioClip Musik){
        Bgmaudioplayer.clip = Musik;
        Bgmaudioplayer.Play();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
