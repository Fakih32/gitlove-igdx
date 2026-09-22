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
//
// - LoadLevel() direfactor total untuk menggunakan struktur database baru:
//   DragDropLevelData.LevelEntry.dragdropquiz[] (DDquiz[]).
//   Setiap DDquiz[i] berkorespondensi 1-to-1 dengan:
//     dragimage[i]      -> sprite drag + scale drag
//     destinyobject[i]  -> sprite siluet + scale tujuan + posisi tujuan
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
    public List<Image> dragimage;
    public Image backgroundImage;

    [Header("Posisi Tujuan di Scene")]
    public List<GameObject> destinyobject;

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

        // Ambil quiz array & background dari database sesuai level aktif
        DragDropLevelData.DDquiz[] quizzes = levelData.getquizdragdrop(currentLevel);
        Sprite bg = levelData.getspritebg(currentLevel);

        if (quizzes == null || quizzes.Length == 0) {
            Debug.LogWarning($"DragDropController: Level {currentLevel} tidak ditemukan atau tidak punya quiz di DragDropLevelData");
            return;
        }

        // Set background
        if (backgroundImage != null) {
            backgroundImage.sprite = bg;
        } else {
            Debug.LogWarning("DragDropController: backgroundImage is not assigned in the Inspector!");
        }

        // Setiap DDquiz[i] dipasangkan 1-to-1 dengan dragimage[i] dan destinyobject[i]
        for (int i = 0; i < quizzes.Length; i++) {
            DragDropLevelData.DDquiz quiz = quizzes[i];

            // --- Setup drag image ---
            if (i < dragimage.Count && dragimage[i] != null) {
                Image img = dragimage[i];
                RectTransform imgRect = img.GetComponent<RectTransform>();

                if (quiz.Dragobject != null && quiz.Dragobject.Count > 0)
                    img.sprite = quiz.Dragobject[0];

                if (quiz.Imagescale != null && quiz.Imagescale.Count > 0)
                    imgRect.localScale = (Vector3)quiz.Imagescale[0] + Vector3.forward;
            } else {
                Debug.LogWarning($"DragDropController: dragimage[{i}] tidak ada atau null");
            }

            // --- Setup destiny object (siluet / slot tujuan) ---
            if (i < destinyobject.Count && destinyobject[i] != null) {
                GameObject dest = destinyobject[i];
                Image destImg = dest.GetComponent<Image>();
                RectTransform destRect = dest.GetComponent<RectTransform>();

                if (destImg != null && quiz.SiluetDrag != null && quiz.SiluetDrag.Count > 0)
                    destImg.sprite = quiz.SiluetDrag[0];

                if (destRect != null) {
                    if (quiz.DestinyImagescale != null && quiz.DestinyImagescale.Count > 0)
                        destRect.localScale = (Vector3)quiz.DestinyImagescale[0] + Vector3.forward;

                    if (quiz.Destinypos != null && quiz.Destinypos.Count > 0)
                        destRect.anchoredPosition = quiz.Destinypos[0];
                }
            } else {
                Debug.LogWarning($"DragDropController: destinyobject[{i}] tidak ada atau null");
            }
        }
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