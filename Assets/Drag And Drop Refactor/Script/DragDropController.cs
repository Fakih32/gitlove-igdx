using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// ROMBAK dari versi sebelumnya.
// Perubahan utama:
// - Ketergantungan ke DataLevelHandler DIHAPUS TOTAL. Sebelumnya
//   currentLevel diambil dari DataLevelHandler.Instance.currentlevel
//   (int terpisah, disinkronkan lewat polling Update() dari
//   LevelSelectionHandler) -- ini sumber kebenaran ganda yang bikin
//   currentLevel gampang nyangkut/telat update.
//   Sekarang currentLevel diambil langsung dari
//   LevelSessionManager.Instance.currentLevel.levelIndex, satu-satunya
//   sumber kebenaran soal "level yang sedang dimainkan", yang sudah
//   di-set benar oleh LevelSelectionHandler.selectlevel() saat player
//   memilih level.
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
        currentLevel = ResolveCurrentLevelIndex(LevelSessionManager.Instance?.currentLevel);
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

                    targetSlot1.GetComponent<Image>().sprite = data.firstsiluet;
                    targetSlot2.GetComponent<Image>().sprite = data.secondsiluet;
                    targetSlot3.GetComponent<Image>().sprite = data.thridsiluet;
                firstImage.GetComponent<RectTransform>().localScale = data.firstimageScale;
                secondImage.GetComponent<RectTransform>().localScale = data.secondimageScale;
                thirdImage.GetComponent<RectTransform>().localScale = data.thirdimageScale;
                targetSlot1.GetComponent<RectTransform>().localScale = data.firstsiluetsize;
                 targetSlot2.GetComponent<RectTransform>().localScale = data.secondsiluetsize;
                targetSlot3.GetComponent<RectTransform>().localScale = data.thridsiluetsize;
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

    // Fallback ke 0 kalau LevelSessionManager/currentLevel belum ke-set --
    // seharusnya tidak pernah kejadian di alur normal (selalu lewat
    // LevelSelectionHandler.selectlevel() dulu), tapi tetap aman daripada
    // NullReferenceException kalau scene ini di-test langsung.
    public static int ResolveCurrentLevelIndex(LevelData currentLevel) {
        if (currentLevel == null) {
            Debug.LogWarning("DragDropController: LevelSessionManager.currentLevel null, fallback ke level index 0");
            return 0;
        }
        return currentLevel.levelIndex;
    }
}