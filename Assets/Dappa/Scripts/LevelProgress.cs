using UnityEngine;

// BARU. Fondasi progression sistem level, sepola StickerCollection
// (static helper di atas PlayerPrefs, bukan MonoBehaviour, jadi bisa
// dipanggil dari mana aja: GameOverController, LevelSelectionHandler,
// Book UI, dst -- tanpa perlu reference GameObject).
//
// Single source of truth untuk "sampai level mana player boleh main".
// Menggantikan LevelSelectionHandler.totalunlockedLevel +
// DataLevelHandler.totalunlockedLevel yang sebelumnya dua-duanya nyimpen
// state yang sama secara terpisah dan disambung lewat polling di Update().
//
// Level index berbasis 0 (level pertama = index 0), match sama
// urutan array LevelDatabase yang bakal dibuat setelah ini.
public static class LevelProgress {
    private const string ProgressKey = "level_progress_highest_unlocked";

    // Level pertama selalu kebuka meskipun belum pernah ada save sama
    // sekali, biar player baru nggak stuck di level selection kosong.
    private const int DefaultHighestUnlockedIndex = 0;

    public static int HighestUnlockedIndex =>
        PlayerPrefs.GetInt(ProgressKey, DefaultHighestUnlockedIndex);

    public static bool IsLevelUnlocked(int levelIndex) {
        return levelIndex <= HighestUnlockedIndex;
    }

    // Cuma boleh naik. Dipanggil GameOverController setelah player menang
    // sebuah level dengan levelIndex level itu + 1 (unlock level berikutnya).
    // Aman dipanggil berkali-kali / dengan index lebih rendah dari
    // progress saat ini -- tidak akan menurunkan progress yang sudah ada.
    public static void UnlockUpTo(int levelIndex) {
        if (levelIndex <= HighestUnlockedIndex) return;

        PlayerPrefs.SetInt(ProgressKey, levelIndex);
        PlayerPrefs.Save();
    }

    // Buat testing/debug: reset total progress balik ke default.
    public static void ResetProgress() {
        PlayerPrefs.DeleteKey(ProgressKey);
    }
}