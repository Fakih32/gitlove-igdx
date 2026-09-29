using UnityEngine;

// REVISI:
// 1. Restart() tidak lagi menerima nama scene dan tidak memanggil
//    SceneManager.LoadScene sendiri. Sebelumnya scene dimuat dua kali
//    (sekali di LevelSessionManager.restartlevel(), sekali di sini), dan
//    nama scene yang di-pass manual bisa tidak sinkron dengan
//    currentMechanicIndex. Sekarang LevelSessionManager satu-satunya yang
//    menentukan scene restart, lewat LevelData.restartMechanicIndex.
// 2. Restart() mengembalikan timeScale dan menutup panel pause lebih dulu,
//    jadi kalau restart gagal pemain tidak terjebak di game yang membeku.
// 3. Guard LevelSessionManager.Instance == null.
public class PauseLogic : MonoBehaviour {
    public GameObject pauseMenu;

    public void TogglePause() {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume() {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Restart() {
        Time.timeScale = 1f;

        if (pauseMenu != null) {
            pauseMenu.SetActive(false);
        }

        if (LevelSessionManager.Instance == null) {
            Debug.LogError("PauseLogic: LevelSessionManager tidak ditemukan, restart dibatalkan.");
            return;
        }

        LevelSessionManager.Instance.restartlevel();
    }

    public void BacktoLevelSelection(string Scene) {
        AudioManager.Instance?.PlayBgm(AudioScript.instance.BgmMenu);
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(Scene);
    }
}