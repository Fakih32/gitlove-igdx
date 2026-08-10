using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// ROMBAK dari DraganddropLevelHandler.cs.
// Perubahan utama:
// 1. Logic menang/kalah/bintang (Checkwin, Gamewin, Gamelost, panel, star)
//    DIHAPUS dari sini -> pindah ke LevelSessionManager + GameOverController,
//    karena itu level-wide, bukan cuma milik mekanik drag & drop
// 2. Timer sendiri (lewat TimerScript.instance) dihapus -> LevelSessionManager
//    yang urus, termasuk kondisi waktu habis
// 3. Skor nambah lewat LevelSessionManager.AddScore(), bukan variabel lokal
//    currentpoint
// 4. Setelah semua target tercapai, lapor ke
//    LevelSessionManager.OnMechanicComplete() alih-alih urus menang sendiri
public class DragDropController : MonoBehaviour {
    public static DragDropController Instance;

    [Header("Data Level Drag & Drop")]
    public DragDropLevelData levelData;
    public int currentLevel;

    [Header("Target Posisi yang Benar")]
    public List<Transform> targets;
    [HideInInspector] public int targetsHit;
    private int targetsTotal;

    [Header("Gambar yang Di-drag")]
    public Image firstImage;
    public Image secondImage;
    public Image thirdImage;

    [Header("Posisi Tujuan di Scene")]
    public GameObject targetSlot1;
    public GameObject targetSlot2;
    public GameObject targetSlot3;

  

    void Awake() {
        Instance = this;
    }

    void Start() {
        LoadLevel();
        targetsTotal = targets.Count;
    }

    void LoadLevel() {
        foreach (var data in levelData.levels) {
            if (data.level == currentLevel) {
                firstImage.sprite = data.firstImage;
                secondImage.sprite = data.secondImage;
                thirdImage.sprite = data.thirdImage;

                targetSlot1.transform.position = data.firstImagePos;
                targetSlot2.transform.position = data.secondImagePos;
                targetSlot3.transform.position = data.thirdImagePos;
                return;
            }
        }

        Debug.LogWarning($"Level {currentLevel} tidak ditemukan di DragDropLevelData");
    }

    // Dipanggil dari DraggableItem tiap kali 1 item berhasil ditaruh di target yang benar
    public void OnTargetHit() {
        targetsHit++;
        //LevelSessionManager.Instance?.AddScore(pointsPerHit);

        if (targetsHit >= targetsTotal) {
            LevelSessionManager.Instance?.OnMechanicComplete();
        }
    }
}
