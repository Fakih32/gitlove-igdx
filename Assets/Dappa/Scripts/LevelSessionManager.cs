using UnityEngine;
using UnityEngine.SceneManagement;

// Satu-satunya objek yang boleh "tau" soal level secara keseluruhan:
// timer, skor, urutan mekanik, transisi antar scene, dan kondisi menang/kalah.
// Dipasang di LevelSelectionScene, lalu bertahan sepanjang game berkat
// DontDestroyOnLoad.
//
// REVISI:
// 1. restartlevel() tidak lagi nge-hardcode index 1. Index restart sekarang
//    dibaca dari LevelData.GetRestartMechanicIndex() (default 1 = lewati
//    cutscene di index 0, level tanpa cutscene bisa set 0).
// 2. State awal sesi dipusatkan di CreateFreshState(), dipakai bareng
//    StartLevel() dan restartlevel(), jadi tidak ada lagi reset yang
//    duplikat dan bisa lupa satu field.
// 3. Scene di-load lewat TryLoadMechanicScene() yang mengecek index dan
//    nama scene dulu, supaya LoadScene("") tidak gagal diam-diam.
// 4. restartlevel() sekarang satu-satunya yang memuat scene saat restart.
//    Pemanggil (PauseLogic) tidak boleh memanggil LoadScene lagi setelahnya.
// 5. Field levelsaatini dihapus karena tidak dipakai di mana pun.
public class LevelSessionManager : MonoBehaviour {
    public static LevelSessionManager Instance;

    public readonly struct SessionState {
        public readonly float timeRemaining;
        public readonly int score;
        public readonly int mechanicIndex;
        public readonly bool levelFailed;

        public SessionState(float timeRemaining, int score, int mechanicIndex, bool levelFailed) {
            this.timeRemaining = timeRemaining;
            this.score = score;
            this.mechanicIndex = mechanicIndex;
            this.levelFailed = levelFailed;
        }
    }

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
        ApplyState(CreateFreshState(levelData, startMechanicIndex: 0));
        AudioManager.Instance?.PlayBgm(AudioScript.instance.Bgmgame);

        TryLoadMechanicScene(currentMechanicIndex);
    }

    // Mengulang level dengan timer & skor penuh, mulai dari
    // restartMechanicIndex milik level ini (umumnya melewati cutscene).
    // Method ini yang memuat scene, pemanggil tidak perlu (dan tidak boleh)
    // memanggil SceneManager.LoadScene lagi setelahnya.
    public void restartlevel() {
        if (currentLevel == null) {
            Debug.LogError("LevelSessionManager: restartlevel() dipanggil tapi currentLevel belum di-set");
            return;
        }

        ApplyState(CreateFreshState(currentLevel, currentLevel.GetRestartMechanicIndex()));
        TryLoadMechanicScene(currentMechanicIndex);
    }

    public void AddScore(int amount) {
        score += amount;
    }

    // Dipanggil oleh WordQuizController / DragDropController / cutscene saat
    // mekaniknya sendiri sudah kelar. Manager ini yang mutusin scene apa
    // berikutnya, bukan scene itu sendiri.
    public void OnMechanicComplete() {
        if (currentLevel == null) {
            Debug.LogError("LevelSessionManager: currentLevel belum di-set, panggil StartLevel() dulu");
            return;
        }

        currentMechanicIndex++;

        if (currentMechanicIndex < currentLevel.mechanicSceneNames.Length) {
            SceneManager.LoadScene(currentLevel.mechanicSceneNames[currentMechanicIndex]);
        } else {
            AudioManager.Instance?.PlayBgm(AudioScript.instance.BgmMenu);
            AudioManager.Instance?.PlaySfx(AudioScript.instance.Winsound);
            SceneManager.LoadScene(currentLevel.gameOverSceneName);
        }
    }

    // Dipanggil otomatis dari Update() kalau timeRemaining habis,
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

    void ApplyState(SessionState state) {
        timeRemaining = state.timeRemaining;
        score = state.score;
        currentMechanicIndex = state.mechanicIndex;
        levelFailed = state.levelFailed;
    }

    bool TryLoadMechanicScene(int mechanicIndex) {
        if (!CanLoadMechanic(currentLevel, mechanicIndex)) {
            Debug.LogError($"LevelSessionManager: mechanicSceneNames[{mechanicIndex}] tidak valid (kosong / di luar batas) untuk level '{currentLevel?.levelId}'");
            return false;
        }

        SceneManager.LoadScene(currentLevel.mechanicSceneNames[mechanicIndex]);
        return true;
    }

    public static SessionState CreateFreshState(LevelData level, int startMechanicIndex) {
        return new SessionState(level.timeLimit, 0, startMechanicIndex, false);
    }

    public static bool CanLoadMechanic(LevelData level, int mechanicIndex) {
        if (level == null || level.mechanicSceneNames == null) return false;
        if (mechanicIndex < 0 || mechanicIndex >= level.mechanicSceneNames.Length) return false;
        return !string.IsNullOrEmpty(level.mechanicSceneNames[mechanicIndex]);
    }
}

public enum ScoreTier {
    OneStar,
    TwoStar,
    ThreeStar
}