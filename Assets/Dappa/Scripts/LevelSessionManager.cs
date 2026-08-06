using UnityEngine;
using UnityEngine.SceneManagement;

// Satu-satunya objek yang boleh "tau" soal level secara keseluruhan:
// timer, skor, urutan mekanik, transisi antar scene, dan kondisi menang/kalah.
// Dipasang di scene paling awal (Bootstrap/MainMenu), lalu bertahan
// sepanjang game berkat DontDestroyOnLoad.
public class LevelSessionManager : MonoBehaviour {
    public static LevelSessionManager Instance;

    [Header("Level yang sedang dimainkan")]
    public LevelData currentLevel;

    [HideInInspector] public float timeRemaining;
    [HideInInspector] public int score;
    [HideInInspector] public int currentMechanicIndex = 0;
    [HideInInspector] public bool levelFailed = false;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    void Update() {
        if (timeRemaining <= 0f) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f) {
            timeRemaining = 0f;
            HandleTimeUp();
        }
    }

    public void StartLevel(LevelData levelData) {
        currentLevel = levelData;
        timeRemaining = levelData.timeLimit;
        score = 0;
        currentMechanicIndex = 0;
        levelFailed = false;

        SceneManager.LoadScene(currentLevel.mechanicSceneNames[currentMechanicIndex]);
    }

    public void AddScore(int amount) {
        score += amount;
    }

    // Dipanggil oleh WordQuizController / DragDropController saat mekaniknya
    // sendiri sudah kelar. Manager ini yang mutusin scene apa berikutnya,
    // bukan scene itu sendiri.
    public void OnMechanicComplete() {
        if (currentLevel == null) {
            Debug.LogError("LevelSessionManager: currentLevel belum di-set, panggil StartLevel() dulu");
            return;
        }

        currentMechanicIndex++;

        if (currentMechanicIndex < currentLevel.mechanicSceneNames.Length) {
            SceneManager.LoadScene(currentLevel.mechanicSceneNames[currentMechanicIndex]);
        } else {
            SceneManager.LoadScene(currentLevel.gameOverSceneName);
        }
    }

    // BARU: dipanggil otomatis dari Update() kalau timeRemaining habis,
    // dari mekanik manapun yang sedang aktif. Level dianggap gagal,
    // langsung lompat ke scene Game Over tanpa peduli sisa soal.
    void HandleTimeUp() {
        levelFailed = true;
        SceneManager.LoadScene(currentLevel.gameOverSceneName);
    }

    public ScoreTier GetScoreTier() {
        float timeUsed = currentLevel.timeLimit - timeRemaining;

        if (timeUsed <= currentLevel.threeStarTime) return ScoreTier.ThreeStar;
        if (timeUsed <= currentLevel.twoStarTime) return ScoreTier.TwoStar;
        return ScoreTier.OneStar;
    }
}

public enum ScoreTier {
    OneStar,
    TwoStar,
    ThreeStar
}
