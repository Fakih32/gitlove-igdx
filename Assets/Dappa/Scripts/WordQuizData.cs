using UnityEngine;

// ROMBAK dari versi sebelumnya.
// Perubahan: quizzes yang tadinya flat array (dipakai sama rata buat
// semua level) sekarang dibungkus per-LevelEntry, sepola DragDropLevelData.
// Alasan sama: WordQuizScene dipakai bareng oleh semua level (3 level,
// 1 scene), jadi soal yang muncul harus beda-beda tergantung level mana
// yang sedang dimainkan, bukan satu set soal yang sama terus.
[CreateAssetMenu(fileName = "WordQuizData", menuName = "Word Quiz/Quiz Data")]
public class WordQuizData : ScriptableObject {
    [System.Serializable]
    public class Quiz {
        public Sprite image;
        public string correctWord;
    }

    [System.Serializable]
    public class LevelEntry {
        [Tooltip("Harus match dengan LevelData.levelIndex (0-based)")]
        public int level;
        public Quiz[] quizzes;
    }

    public LevelEntry[] levels;

    public Quiz[] GetQuizzesForLevel(int levelIndex) {
        foreach (var entry in levels) {
            if (entry.level == levelIndex) return entry.quizzes;
        }
        return null;
    }
}