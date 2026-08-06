using UnityEngine;

// ROMBAK dari AudioScript.cs.
// Perubahan utama:
// 1. DontDestroyOnLoad ditambahkan -> BGM tidak keputus tiap ganti scene
// 2. Bug di Awake() diperbaiki (kondisi lama "instance == null && instance != this"
//    tidak pernah membedakan kasus dengan benar)
// 3. PlaySfx pakai PlayOneShot -> SFX yang tumpang tindih (misal klik cepat)
//    tidak saling motong seperti sebelumnya
public class AudioManager : MonoBehaviour {
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource sfxPlayer;
    public AudioSource bgmPlayer;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void PlaySfx(AudioClip clip) {
        if (clip == null || sfxPlayer == null) return;
        sfxPlayer.PlayOneShot(clip);
    }

    public void PlayBgm(AudioClip clip) {
        if (clip == null || bgmPlayer == null) return;
        if (bgmPlayer.clip == clip && bgmPlayer.isPlaying) return; // hindari restart BGM yang sama
        bgmPlayer.clip = clip;
        bgmPlayer.Play();
    }
}
