using UnityEngine;

// ROMBAK dari versi sebelumnya (dulu pakai istilah "star").
// Perubahan:
// 1. Nama field diganti jadi "sticker" biar sesuai konsep collectible kalian
// 2. Setelah menang, status unlock stiker disimpan permanen lewat
//    StickerCollection, biar bisa dibaca lagi sama scene Level Selection
public class GameOverController : MonoBehaviour {
    [Header("Panel")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Stiker (placeholder dulu, nanti diganti sprite asli)")]
    public GameObject bronzeSticker;
    public GameObject silverSticker;
    public GameObject goldSticker;

    void Start() {
        if (LevelSessionManager.Instance == null) {
            Debug.LogError("LevelSessionManager tidak ditemukan di scene ini");
            return;
        }

        if (LevelSessionManager.Instance.levelFailed) {
            ShowLose();
        } else {
            ScoreTier tier = LevelSessionManager.Instance.GetScoreTier();
            ShowWin(tier);
            SaveUnlockedStickers(tier);
        }
    }

    void ShowLose() {
        losePanel.SetActive(true);
    }

    void ShowWin(ScoreTier tier) {
        winPanel.SetActive(true);
        bronzeSticker.SetActive(false);
        silverSticker.SetActive(false);
        goldSticker.SetActive(false);

        // Kumulatif: tier lebih tinggi otomatis nyalain semua yang di bawahnya juga
        if (tier == ScoreTier.OneStar) {
            bronzeSticker.SetActive(true);
        } else if (tier == ScoreTier.TwoStar) {
            bronzeSticker.SetActive(true);
            silverSticker.SetActive(true);
        } else if (tier == ScoreTier.ThreeStar) {
            bronzeSticker.SetActive(true);
            silverSticker.SetActive(true);
            goldSticker.SetActive(true);
        }
    }

    void SaveUnlockedStickers(ScoreTier tier) {
        string levelId = LevelSessionManager.Instance.currentLevel.levelId;

        StickerCollection.Unlock(levelId, ScoreTier.OneStar);
        if (tier >= ScoreTier.TwoStar) StickerCollection.Unlock(levelId, ScoreTier.TwoStar);
        if (tier >= ScoreTier.ThreeStar) StickerCollection.Unlock(levelId, ScoreTier.ThreeStar);
    }
}