using UnityEngine;

// UPDATE dari versi sebelumnya. Nambah levelIndex biar level punya
// posisi eksplisit dalam urutan main, dipakai LevelProgress buat
// nentuin "level ke berapa ini" tanpa parsing string levelId.
// levelId (string) tetap dipertahankan apa adanya karena itu kunci
// penyimpanan stiker yang sudah dipakai StickerCollection -- ubah ini
// beresiko putus koneksi ke data stiker yang sudah kesimpen pemain.
//
// UPDATE: nambah restartMechanicIndex. Sebelumnya restartlevel() di
// LevelSessionManager nge-hardcode index 1 (lewati cutscene di index 0).
// Sekarang angka itu jadi data per level: level tanpa cutscene tinggal
// set 0, tidak perlu ubah kode.
[CreateAssetMenu(fileName = "LevelData", menuName = "Level/Level Data")]
public class LevelData : ScriptableObject {
    [Header("Identitas Level (buat nyimpen progress/stiker)")]
    [Tooltip("Harus unik per level, jangan diubah-ubah setelah dipakai karena ini kunci penyimpanan progress pemain")]
    public string levelId = "level_1";

    [Header("Urutan Level (buat unlock-progression)")]
    [Tooltip("Posisi level ini dalam urutan main, mulai dari 0. Harus unik & berurutan tanpa lompat -- divalidasi lewat LevelDatabase.ValidateOrdering()")]
    public int levelIndex = 0;

    [Header("Urutan scene mekanik untuk level ini")]
    [Tooltip("Contoh: [\"CutsceneScene\", \"DragDropScene\", \"WordQuizScene\"]")]
    public string[] mechanicSceneNames;

    [Tooltip("Index di mechanicSceneNames yang dimuat saat restart. Umumnya 1 kalau index 0 adalah cutscene intro (restart melewati cutscene). Isi 0 kalau level ini tidak punya cutscene.")]
    public int restartMechanicIndex = 1;

    public string gameOverSceneName = "GameOverScene";

    [Header("Waktu & Threshold Bintang")]
    public float timeLimit = 60f;
    public float threeStarTime = 20f;
    public float twoStarTime = 40f;

    // Index yang aman dipakai buat restart. Kalau restartMechanicIndex
    // salah isi (negatif / di luar array), fallback ke 0 daripada crash
    // atau load scene kosong.
    public int GetRestartMechanicIndex() {
        if (mechanicSceneNames == null || mechanicSceneNames.Length == 0) return 0;
        if (restartMechanicIndex < 0 || restartMechanicIndex >= mechanicSceneNames.Length) return 0;
        return restartMechanicIndex;
    }
}