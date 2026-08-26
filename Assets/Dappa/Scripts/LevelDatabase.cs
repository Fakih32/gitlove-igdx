using UnityEngine;
using System.Linq;

// BARU. Kumpulan semua LevelData dalam satu urutan main, satu source
// of truth buat LevelSelectionHandler (render tombol) dan Book/wiki
// stiker (enumerasi semua level buat ditampilin status unlock-nya).
//
// Ini yang bikin LevelSelectionHandler nggak perlu lagi nyimpen list
// sprite locked/unlocked manual per-index secara terpisah dari data
// level yang sebenernya -- rawan mismatch kalau urutan level berubah.
[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Level/Level Database")]
public class LevelDatabase : ScriptableObject {
    public LevelData[] levels;

    public int Count => levels?.Length ?? 0;

    public LevelData GetByIndex(int levelIndex) {
        if (levels == null) return null;
        return levels.FirstOrDefault(level => level != null && level.levelIndex == levelIndex);
    }

    // Panggil ini dari custom Editor / OnValidate saat setup, biar
    // ketauan dari awal kalau ada level yang lupa di-assign index-nya
    // atau ke-duplikat, sebelum jadi bug runtime yang aneh pas main.
    public bool ValidateOrdering() {
        if (levels == null || levels.Length == 0) return true;

        int[] sortedIndices = levels
            .Where(level => level != null)
            .Select(level => level.levelIndex)
            .OrderBy(i => i)
            .ToArray();

        for (int i = 0; i < sortedIndices.Length; i++) {
            if (sortedIndices[i] != i) return false;
        }

        return true;
    }
}