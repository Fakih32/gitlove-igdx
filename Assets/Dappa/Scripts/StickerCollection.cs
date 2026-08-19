using UnityEngine;

// BARU. Static helper (bukan MonoBehaviour, jadi bisa dipanggil dari mana
// aja tanpa perlu reference GameObject) buat nyimpen & baca status unlock
// stiker per level. Dipakai GameOverController buat nyimpen hasil,
// nanti dipakai juga sama scene Level Selection ("Books") buat baca
// stiker apa aja yang udah kebuka.
//
// Pakai PlayerPrefs karena datanya simpel (per level cuma 3 boolean) dan
// otomatis persisten antar sesi main tanpa perlu bikin sistem save file
// sendiri. Kalau nanti datanya makin kompleks (misal butuh deskripsi custom
// per stiker yang bisa berubah), baru worth dipertimbangkan pindah ke JSON.
public static class StickerCollection {
    public static void Unlock(string levelId, ScoreTier tier) {
        PlayerPrefs.SetInt(GetKey(levelId, tier), 1);
        PlayerPrefs.Save();
    }

    public static bool IsUnlocked(string levelId, ScoreTier tier) {
        return PlayerPrefs.GetInt(GetKey(levelId, tier), 0) == 1;
    }

    private static string GetKey(string levelId, ScoreTier tier) {
        return $"sticker_{levelId}_{tier}";
    }
}
