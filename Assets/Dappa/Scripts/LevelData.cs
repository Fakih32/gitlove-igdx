using UnityEngine;

// UPDATE dari versi sebelumnya. Nambah levelIndex biar level punya
// posisi eksplisit dalam urutan main, dipakai LevelProgress buat
// nentuin "level ke berapa ini" tanpa parsing string levelId.
// levelId (string) tetap dipertahankan apa adanya karena itu kunci
// penyimpanan stiker yang sudah dipakai StickerCollection -- ubah ini
// beresiko putus koneksi ke data stiker yang sudah kesimpen pemain.
[CreateAssetMenu(fileName = "LevelData", menuName = "Level/Level Data")]
public class LevelData : ScriptableObject {
    [Header("Identitas Level (buat nyimpen progress/stiker)")]
    [Tooltip("Harus unik per level, jangan diubah-ubah setelah dipakai karena ini kunci penyimpanan progress pemain")]
    public string levelId = "level_1";

    [Header("Urutan Level (buat unlock-progression)")]
    [Tooltip("Posisi level ini dalam urutan main, mulai dari 0. Harus unik & berurutan tanpa lompat -- divalidasi lewat LevelDatabase.ValidateOrdering()")]
    public int levelIndex = 0;

    [Header("Urutan scene mekanik untuk level ini")]
    [Tooltip("Contoh: [\"DragDropScene\", \"WordQuizScene\"]")]
    public string[] mechanicSceneNames;

    public string gameOverSceneName = "GameOverScene";

    [Header("Waktu & Threshold Bintang")]
    public float timeLimit = 60f;
    public float threeStarTime = 20f;
    public float twoStarTime = 40f;
}