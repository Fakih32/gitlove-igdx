using UnityEngine;

// BARU. Data-driven config per level, biar nambah level baru
// tinggal bikin asset baru tanpa sentuh kode sama sekali.
[CreateAssetMenu(fileName = "LevelData", menuName = "Level/Level Data")]
public class LevelData : ScriptableObject {
    [Header("Urutan scene mekanik untuk level ini")]
    [Tooltip("Contoh: [\"DragDropScene\", \"WordQuizScene\"]")]
    public string[] mechanicSceneNames;

    public string gameOverSceneName = "GameOverScene";

    [Header("Waktu & Threshold Bintang")]
    public float timeLimit = 60f;
    public float threeStarTime = 20f;
    public float twoStarTime = 40f;
}
