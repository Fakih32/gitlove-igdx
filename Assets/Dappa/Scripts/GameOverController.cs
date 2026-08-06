using UnityEngine;

// BARU. Sebelumnya logic ini (Checkwin, Gamewin, Gamelost, panel, star)
// nyangkut di DraganddropLevelHandler dan cuma jalan kalau drag & drop
// yang terakhir selesai. Sekarang dipisah ke scene sendiri karena kondisi
// menang/kalah itu level-wide -- dipicu dari mekanik manapun yang terakhir
// jalan, atau dari LevelSessionManager kalau waktu habis duluan.
public class GameOverController : MonoBehaviour {
    [Header("Panel")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Bintang")]
    public GameObject firstStar;
    public GameObject secondStar;
    public GameObject thirdStar;

    void Start() {
        if (LevelSessionManager.Instance == null) {
            Debug.LogError("LevelSessionManager tidak ditemukan di scene ini");
            return;
        }

        if (LevelSessionManager.Instance.levelFailed) {
            ShowLose();
        } else {
            ShowWin(LevelSessionManager.Instance.GetScoreTier());
        }
    }

    void ShowLose() {
        losePanel.SetActive(true);
    }

    void ShowWin(ScoreTier tier) {
        winPanel.SetActive(true);
        firstStar.SetActive(false);
        secondStar.SetActive(false);
        thirdStar.SetActive(false);

        if (tier == ScoreTier.OneStar) {
            firstStar.SetActive(true);
        } else if (tier == ScoreTier.TwoStar) {
            firstStar.SetActive(true);
            secondStar.SetActive(true);
        } else if (tier == ScoreTier.ThreeStar) {
            firstStar.SetActive(true);
            secondStar.SetActive(true);
            thirdStar.SetActive(true);
        }
    }
}
