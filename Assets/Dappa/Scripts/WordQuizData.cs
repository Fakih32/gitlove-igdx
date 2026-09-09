using UnityEngine;

// ROMBAK dari versi sebelumnya.
// Perubahan: tambah field background per-LevelEntry, sepola
// DragDropLevelData.BackgroundImage. Background ini per LEVEL, bukan
// per-quiz -- jadi tetap sama meskipun soal ganti dalam 1 level yang
// sama, cuma berubah saat pindah level (Level 1 -> Level 2, dst).
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
        public Sprite background;
        public Quiz[] quizzes;
    }

    public LevelEntry[] levels;

    public Quiz[] GetQuizzesForLevel(int levelIndex) {
        foreach (var entry in levels) {
            if (entry.level == levelIndex) return entry.quizzes;
        }
        return null;
    }

    public Sprite GetBackgroundForLevel(int levelIndex) {
        foreach (var entry in levels) {
            if (entry.level == levelIndex) return entry.background;
        }
        return null;
    }
}