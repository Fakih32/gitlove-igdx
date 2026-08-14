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
    public Image backgroundImage;

    [Header("Posisi Tujuan di Scene")]
    public GameObject targetSlot1;
    public GameObject targetSlot2;
    public GameObject targetSlot3;

  

    void Awake() {
        Instance = this;
        if (DataLevelHandler.Instance != null) {
            currentLevel = DataLevelHandler.Instance.currentlevel;
        } else {
            Debug.LogWarning("DataLevelHandler.Instance is null, using default currentLevel");
        }
    }

    void Start() {
        LoadLevel();
        targetsTotal = targets.Count;
      
    }

    void LoadLevel() {
        if (levelData == null) {
            Debug.LogError("DragDropController: levelData belum di-set");
            return;
        }
       
        foreach (var data in levelData.levels) {
            if (data.level == currentLevel) {
                firstImage.sprite = data.firstImage;
                secondImage.sprite = data.secondImage;
                thirdImage.sprite = data.thirdImage;
                 firstImage.GetComponent<RectTransform>().localScale = data.firstimageScale;
            secondImage.GetComponent<RectTransform>().localScale = data.secondimageScale;
            thirdImage.GetComponent<RectTransform>().localScale = data.thirdimageScale;
                if (backgroundImage != null) {
                    backgroundImage.sprite = data.BackgroundImage;
                } else {
                    Debug.LogWarning("DragDropController: backgroundImage is not assigned in the Inspector!");
                }
                targetSlot1.GetComponent<RectTransform>().anchoredPosition = data.firstImagePos;
                targetSlot2.GetComponent<RectTransform>().anchoredPosition = data.secondImagePos;
                targetSlot3.GetComponent<RectTransform>().anchoredPosition = data.thirdImagePos;
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
