using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// ROMBAK dari versi sebelumnya.
// Perubahan utama:
// 1. totalunlockedLevel (state lokal manual + PlayerPrefs "TotalUnlockedLevel")
//    DIHAPUS -> baca dari LevelProgress (single source of truth yang sudah
//    dipakai GameOverController buat nyimpen progress setelah menang level).
// 2. levelData (LevelDatabase) ditambah -> selectlevel() sekarang beneran
//    manggil LevelSessionManager.Instance.StartLevel(levelData) dengan
//    LevelData yang benar, bukan cuma load scene gameplay begitu aja.
//    Ini yang tadinya jadi tugas LevelStarter (skrip sementara, sekarang
//    sudah tidak dipakai lagi).
// 3. currentlevel (int) dihapus -- konsumen lama (DragDropController lewat
//    DataLevelHandler) akan diarahkan baca LevelSessionManager.currentLevel
//    di langkah berikutnya, bukan field ini.
public class LevelSelectionHandler : MonoBehaviour {
    public static LevelSelectionHandler Instance { get; private set; }

    [Header("Sumber Data Level (urutan harus sama dengan levelButtons)")]
    public LevelDatabase levelDatabase;

    [Header("gambar level terkunci")]
    public List<Sprite> lockedLevelImages;
    [Header("gambar level terbuka")]
    public List<Sprite> unlockedLevelImages;

    [Header("Tombol Pilih Level")]
    public List<Button> levelButtons;
    [Header("Ukuran Gambar")]
    public List<Vector2> levelImageSizes;
    [Header("Game Scene")]
    public string gameScene;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start() {
        setupLevelButtons();
    }

    public void selectlevel(GameObject button) {
        int buttonIndex = levelButtons.IndexOf(button.GetComponent<Button>());
        if (buttonIndex < 0) return;

        LevelData levelToStart = ResolveLevelToStart(buttonIndex, LevelProgress.HighestUnlockedIndex, levelDatabase);

        if (levelToStart == null) {
            Debug.LogWarning($"Level index {buttonIndex} terkunci atau tidak ada di LevelDatabase.");
            return;
        }

        if (LevelSessionManager.Instance == null) {
            Debug.LogError("LevelSessionManager tidak ditemukan. Pastikan Managers sudah ada di scene ini.");
            return;
        }

        LevelSessionManager.Instance.StartLevel(levelToStart);
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameScene);
    }

    public void setupLevelButtons() {
        int highestUnlockedIndex = LevelProgress.HighestUnlockedIndex;

        for (int i = 0; i < levelButtons.Count; i++) {
            int index = i;
            bool unlocked = IsButtonUnlocked(index, highestUnlockedIndex);

            if (unlocked && index < unlockedLevelImages.Count) {
                levelButtons[i].GetComponent<Image>().sprite = unlockedLevelImages[index];
                levelButtons[i].GetComponent<RectTransform>().localScale= levelImageSizes[index];
                levelButtons[i].interactable = true;
            } else if (!unlocked && index < lockedLevelImages.Count) {
                levelButtons[i].GetComponent<Image>().sprite = lockedLevelImages[index];
                levelButtons[i].GetComponent<RectTransform>().localScale= levelImageSizes[index];
                levelButtons[i].interactable = false;
            }

            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() => selectlevel(levelButtons[index].gameObject));
        }
    }

    public static bool IsButtonUnlocked(int buttonIndex, int highestUnlockedIndex) {
        return buttonIndex <= highestUnlockedIndex;
    }

    public static LevelData ResolveLevelToStart(int buttonIndex, int highestUnlockedIndex, LevelDatabase database) {
        if (!IsButtonUnlocked(buttonIndex, highestUnlockedIndex)) return null;
        if (database == null) return null;

        return database.GetByIndex(buttonIndex);
    }
}