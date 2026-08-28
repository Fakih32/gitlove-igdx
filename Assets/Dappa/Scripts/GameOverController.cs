using UnityEngine;
using UnityEngine.UI;

// ROMBAK total dari versi sebelumnya.
// Perubahan utama:
// 1. WinPanel + LosePanel (2 panel, 2 set sticker) DIHAPUS -> jadi 1
//    GameOverPanel + 1 set sticker (3 GameObject) yang dipakai bareng
//    buat win maupun lose. Yang membedakan cuma berapa sticker yang
//    di-SetActive(true) -- lose = 0, win = 1/2/3 sesuai tier.
//    Ini nutup bug lama: sticker yang di-assign dari WinPanel jadi
//    tidak muncul kalau yang ke-trigger LosePanel.
// 2. Tambah continueButton -> tombol eksplisit balik ke Level Selection,
//    supaya game flow Main Menu -> Level Selection -> Gameplay ->
//    Level Selection benar-benar tertutup di titik ini, bukan cuma
//    nampilin panel lalu diem.
// 3. Logic murni (hitung jumlah sticker, teks hasil, next level index)
//    dipisah ke static method biar testable tanpa scene/singleton aktif.
public class GameOverController : MonoBehaviour {
    [Header("Panel Tunggal (dipakai buat Win & Lose)")]
    public GameObject gameOverPanel;
    public Text resultText;

    [Header("Stiker (placeholder dulu, nanti diganti sprite asli)")]
    public GameObject bronzeSticker;
    public GameObject silverSticker;
    public GameObject goldSticker;

    [Header("Navigasi")]
    public Button continueButton;
    public string levelSelectionSceneName = "LevelSelectionScene";

    void Start() {
        if (continueButton != null) {
            continueButton.onClick.AddListener(GoToLevelSelection);
        }

        if (LevelSessionManager.Instance == null) {
            Debug.LogError("LevelSessionManager tidak ditemukan di scene ini");
            return;
        }

        bool levelFailed = LevelSessionManager.Instance.levelFailed;
        ScoreTier tier = levelFailed ? default : LevelSessionManager.Instance.GetScoreTier();

        ShowResult(levelFailed, tier);

        if (!levelFailed) {
            SaveUnlockedStickers(tier);
            UnlockNextLevel();
        }
    }

    void ShowResult(bool levelFailed, ScoreTier tier) {
        gameOverPanel.SetActive(true);

        if (resultText != null) {
            resultText.text = GetResultText(levelFailed);
        }

        int stickerCount = GetStickerCount(levelFailed, tier);
        var (bronze, silver, gold) = GetStickerVisibility(stickerCount);

        bronzeSticker.SetActive(bronze);
        silverSticker.SetActive(silver);
        goldSticker.SetActive(gold);
    }

    void SaveUnlockedStickers(ScoreTier tier) {
        string levelId = LevelSessionManager.Instance.currentLevel.levelId;

        StickerCollection.Unlock(levelId, ScoreTier.OneStar);
        if (tier >= ScoreTier.TwoStar) StickerCollection.Unlock(levelId, ScoreTier.TwoStar);
        if (tier >= ScoreTier.ThreeStar) StickerCollection.Unlock(levelId, ScoreTier.ThreeStar);
    }

    void UnlockNextLevel() {
        LevelData currentLevel = LevelSessionManager.Instance.currentLevel;
        int nextIndex = GetNextLevelIndex(currentLevel);
        LevelProgress.UnlockUpTo(nextIndex);
    }

    void GoToLevelSelection() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelSelectionSceneName);
    }

    public static int GetStickerCount(bool levelFailed, ScoreTier tier) {
        if (levelFailed) return 0;

        return tier switch {
            ScoreTier.OneStar => 1,
            ScoreTier.TwoStar => 2,
            ScoreTier.ThreeStar => 3,
            _ => 0
        };
    }

    // Kumulatif: makin banyak count, makin banyak sticker yang nyala dari kiri.
    public static (bool bronze, bool silver, bool gold) GetStickerVisibility(int stickerCount) {
        return (stickerCount >= 1, stickerCount >= 2, stickerCount >= 3);
    }

    public static string GetResultText(bool levelFailed) {
        return levelFailed ? "Waktu Habis!" : "Level Selesai!";
    }

    public static int GetNextLevelIndex(LevelData currentLevel) {
        return currentLevel.levelIndex + 1;
    }
}