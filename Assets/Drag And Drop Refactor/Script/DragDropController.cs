using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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

        foreach (var data in levelData.levels) {
            if (data.level == currentLevel) {
                for(int z = 0; z <dragimage.Count ; z++) {
                    
                
                for(int i = 0; i <data.Dragobject.Count ; i++) {
                    dragimage[z].sprite = data.Dragobject[i];
                }
                 for(int w = 0; w <data.Imagescale.Count ; w++) {
                    dragimage[z].GetComponent<RectTransform>().localScale = data.Imagescale[w];
                }
                }
                 for(int z = 0; z <destinyobject.Count ; z++) {
                    
                
                for(int i = 0; i <data.SiluetDrag.Count ; i++) {
                    dragimage[z].sprite = data.Dragobject[i];
                }
                 for(int f = 0; f <data.DestinyImagescale.Count ; f++) {
                    dragimage[z].GetComponent<RectTransform>().localScale = data.DestinyImagescale[f];
                }
                for(int g = 0; g <data.Destinypos.Count ; g++) {
                    dragimage[z].GetComponent<RectTransform>().anchoredPosition = data.Destinypos[g];
                }
                }

              
                if (backgroundImage != null) {
                    backgroundImage.sprite = data.BackgroundImage;
                } else {
                    Debug.LogWarning("DragDropController: backgroundImage is not assigned in the Inspector!");
                }
               
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