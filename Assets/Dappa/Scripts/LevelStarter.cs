using UnityEngine;

// SEMENTARA, sampai Main Menu scene beneran ada. Tempel di GameObject
// "Managers" yang sama, satu tingkat sama LevelSessionManager & AudioManager.
// Nanti kalau Main Menu sudah jadi, method StartTestLevel() ini bisa
// langsung dicolok ke OnClick tombol "Play" di menu, tanpa refactor lagi.
public class LevelStarter : MonoBehaviour {
    public LevelData levelToStart;

    // Pakai flag sendiri, bukan ngecek LevelSessionManager.currentLevel --
    // soalnya currentLevel itu public dan bisa ke-isi manual nggak sengaja
    // lewat Inspector, yang bikin pengecekan null jadi salah kayak yang
    // baru kejadian.
    private static bool hasStarted = false;

    void Start() {
        if (hasStarted || LevelSessionManager.Instance == null) return;

        hasStarted = true;
        LevelSessionManager.Instance.StartLevel(levelToStart);
    }
}