using UnityEngine;

// BARU. Sumber konten (judul, deskripsi, sprite) untuk tiap stiker
// collectible, dipisah dari StickerCollection karena StickerCollection
// cuma nyimpen STATUS unlock (boolean per levelId+tier di PlayerPrefs),
// bukan konten yang ditampilkan. Database ini yang dibaca BookController
// buat nampilin isi buku catatan perjalanan: 1 LevelData bisa punya
// sampai 3 StickerEntry (Bronze/Silver/Gold, sesuai ScoreTier).
[CreateAssetMenu(fileName = "StickerDatabase", menuName = "Level/Sticker Database")]
public class StickerDatabase : ScriptableObject {

    [System.Serializable]
    public class StickerEntry {
        public ScoreTier tier;
        public Sprite unlockedSprite;
        public string title;
        [TextArea(3, 6)]
        public string description;
    }

    [System.Serializable]
    public class LevelStickerSet {
        public LevelData level;
        public StickerEntry[] stickers;
    }

    [Header("Sprite placeholder buat stiker yang belum terbuka")]
    public Sprite lockedSprite;

    public LevelStickerSet[] levelStickers;

    public StickerEntry GetEntry(string levelId, ScoreTier tier) {
        foreach (var set in levelStickers) {
            if (set.level == null || set.level.levelId != levelId) continue;

            foreach (var entry in set.stickers) {
                if (entry.tier == tier) return entry;
            }
        }
        return null;
    }
}