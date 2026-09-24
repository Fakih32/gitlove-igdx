using UnityEngine;
//Jadinya Cuma Refrensi Audio
public class AudioScript : MonoBehaviour
{
    public static AudioScript instance;
   
    [Header("BGM")]
    public AudioClip BgmMenu;
    public AudioClip Bgmgame;
    [Header("Sfx")]
    public AudioClip correctaudio;
    public AudioClip Dragingup;
    public AudioClip Dropingdown;
    public AudioClip wronganswer;
    public AudioClip Clicking;
    public AudioClip Winsound;
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
        DontDestroyOnLoad(instance);
           
        
    }
    void Start()
    {
        
    }
 
    // Update is called once per frame
    void Update()
    {
        
    }
}
