using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] AudioMixer audiomixxer;
    [SerializeField] Slider audioslide;
    [SerializeField] Slider sfxslide;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlayerPrefs.HasKey("Musicvolume"))
        {
            loadvolume();
        }
        else
        {
            audioslide.value = 1;
        }

        if (PlayerPrefs.HasKey("sfxvolume"))
        {
            loadsfx();
        }
        else
        {
            sfxslide.value = 1;
        }
    }

    public void setvolume()
    {
        float volume = audioslide.value;
        // Clamp volume to prevent Mathf.Log10(0) returning negative infinity
        audiomixxer.SetFloat("BGM", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("Musicvolume", volume);
    }

    public void setsfx()
    {
        float volume = sfxslide.value;
        // Clamp volume to prevent Mathf.Log10(0) returning negative infinity
        audiomixxer.SetFloat("SFX", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("sfxvolume", volume);
    }

    private void loadvolume()
    {
        audioslide.value = PlayerPrefs.GetFloat("Musicvolume");
        setvolume();
    }

    private void loadsfx()
    {
        sfxslide.value = PlayerPrefs.GetFloat("sfxvolume");
        setsfx();
    }

    // Update is called once per frame
    void Update()
    {
        setsfx();
        setvolume();
    }
}
