using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// ROMBAK dari versi sebelumnya.
// Perubahan utama:
// - Ketergantungan ke DataLevelHandler DIHAPUS TOTAL.
// - LoadLevel() menggunakan struktur database DragDropLevelData:
//   Setiap DDquiz[q] berisi List<Sprite> Dragobject, List<Sprite> SiluetDrag, dst.
//   Setiap elemen [j] dalam list itu adalah SATU pasangan drag-destiny.
//   Jadi total item = jumlah seluruh elemen di semua Dragobject list.
//
//   Contoh: DDquiz[0].Dragobject punya 3 sprite → 3 drag image + 3 destiny object
//           DDquiz[1].Dragobject punya 2 sprite → 2 drag image + 2 destiny object
//           Total: 5 pasangan
//
//   Kalau dragimage / destinyobject di Inspector kurang dari yang dibutuhkan,
//   objek baru di-spawn otomatis dari prefab (atau clone index-0 sebagai fallback).
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

    [Header("Prefab & Parent – Drag Image")]
    [Tooltip("Prefab yang di-clone ketika quiz punya lebih banyak item dari dragimage list. " +
             "Wajib ada komponen Image + DraggableItem. " +
             "Jika kosong, akan clone dragimage[0] sebagai fallback.")]
    public Image dragImagePrefab;

    [Tooltip("Parent transform tempat drag image baru di-spawn. " +
             "Jika kosong, spawn di bawah parent dragimage[0].")]
    public Transform dragImageParent;

    [Header("Prefab & Parent – Destiny Object")]
    [Tooltip("Prefab yang di-clone ketika quiz punya lebih banyak item dari destinyobject list. " +
             "Wajib ada komponen Image. " +
             "Jika kosong, akan clone destinyobject[0] sebagai fallback.")]
    public GameObject destinyObjectPrefab;

    [Tooltip("Parent transform tempat destiny object baru di-spawn. " +
             "Jika kosong, spawn di bawah parent destinyobject[0].")]
    public Transform destinyObjectParent;

    [Header("Posisi Tujuan di Scene")]
    public List<GameObject> destinyobject;

    void Awake() {
        Instance = this;
        currentLevel = ResolveCurrentLevelIndex(LevelSessionManager.Instance?.currentLevel);
    }

    void Start() {
        LoadLevel();
        // targetsTotal dihitung setelah LoadLevel() selesai spawn semua destiny objects
        targetsTotal = targets.Count;
    }

    void LoadLevel() {
        if (levelData == null) {
            Debug.LogError("DragDropController: levelData belum di-set");
            return;
        }

        DragDropLevelData.DDquiz[] quizzes = levelData.getquizdragdrop(currentLevel);
        Sprite bg = levelData.getspritebg(currentLevel);

        if (quizzes == null || quizzes.Length == 0) {
            Debug.LogWarning($"DragDropController: Level {currentLevel} tidak ditemukan atau tidak punya quiz.");
            return;
        }

        // Set background
        if (backgroundImage != null)
            backgroundImage.sprite = bg;
        else
            Debug.LogWarning("DragDropController: backgroundImage is not assigned in the Inspector!");

        // Bersihkan targets agar tidak tercampur dengan entry lama dari Inspector.
        // LoadLevel akan mengisi ulang dari destinyobject yang benar-benar dipakai.
        targets.Clear();
        targetsHit = 0;

        // k = indeks global untuk dragimage[] & destinyobject[]
        // Setiap elemen [j] dalam DDquiz.Dragobject → satu pasangan drag-destiny
        int k = 0;
        for (int q = 0; q < quizzes.Length; q++) {
            DragDropLevelData.DDquiz quiz = quizzes[q];

            if (quiz.Dragobject == null || quiz.Dragobject.Count == 0) {
                Debug.LogWarning($"DragDropController: DDquiz[{q}] tidak punya Dragobject, skip.");
                continue;
            }

            int itemCount = quiz.Dragobject.Count;

            for (int j = 0; j < itemCount; j++, k++) {
                // Helper: ambil elemen ke-j atau fallback ke 0 jika list terlalu pendek
                T SafeGet<T>(List<T> list, int idx) => (list != null && list.Count > idx) ? list[idx]
                                                      : (list != null && list.Count > 0) ? list[0]
                                                      : default;

                // --- Pastikan dragimage[k] ada ---
                if (k >= dragimage.Count || dragimage[k] == null) {
                    if (SpawnDragImage(k) == null) {
                        Debug.LogWarning($"DragDropController: gagal spawn dragimage[{k}] (quiz[{q}] item[{j}])");
                        continue;
                    }
                }

                // --- Pastikan destinyobject[k] ada ---
                if (k >= destinyobject.Count || destinyobject[k] == null) {
                    if (SpawnDestinyObject(k) == null) {
                        Debug.LogWarning($"DragDropController: gagal spawn destinyobject[{k}] (quiz[{q}] item[{j}])");
                        continue;
                    }
                }

                // --- Apply data drag image ---
                {
                    Image img = dragimage[k];
                    RectTransform imgRect = img.GetComponent<RectTransform>();

                    Sprite dragSprite = SafeGet(quiz.Dragobject, j);
                    if (dragSprite != null) img.sprite = dragSprite;

                    Vector2 scale = SafeGet(quiz.Imagescale, j);
                    if (scale != default)
                        imgRect.localScale = new Vector3(scale.x, scale.y, 1f);
                }

                // --- Apply data destiny object ---
                {
                    GameObject dest = destinyobject[k];
                    Image destImg = dest.GetComponent<Image>();
                    RectTransform destRect = dest.GetComponent<RectTransform>();

                    Sprite siluet = SafeGet(quiz.SiluetDrag, j);
                    if (destImg != null && siluet != null)
                        destImg.sprite = siluet;

                    if (destRect != null) {
                        // Cek ada datanya dulu (jangan cek != default karena (0,0) adalah posisi valid)
                        bool hasScale = quiz.DestinyImagescale != null && quiz.DestinyImagescale.Count > 0;
                        bool hasPos   = quiz.Destinypos != null && quiz.Destinypos.Count > 0;

                        if (hasScale) {
                            Vector2 destScale = SafeGet(quiz.DestinyImagescale, j);
                            destRect.localScale = new Vector3(destScale.x, destScale.y, 1f);
                        }

                        if (hasPos) {
                            Vector2 pos = SafeGet(quiz.Destinypos, j);
                            destRect.anchoredPosition = pos;
                        }
                    }

                    // Daftarkan ke targets list agar targetsTotal otomatis sesuai
                    if (!targets.Contains(dest.transform))
                        targets.Add(dest.transform);
                }

                // --- Wire DraggableItem ---
                // draggedObject = Image GO-nya sendiri, target = destiny yang berkorespondensi
                {
                    Image img = dragimage[k];
                    DraggableItem draggable = img.GetComponent<DraggableItem>();
                    if (draggable == null)
                        draggable = img.gameObject.AddComponent<DraggableItem>();

                    draggable.draggedObject = img.gameObject;
                    draggable.target = destinyobject[k].GetComponent<RectTransform>();
                    draggable.rootCanvas = img.GetComponentInParent<Canvas>();

                    // Reset state — clone atau pre-existing bisa saja punya placedOnTarget = true
                    // dari sesi sebelumnya; pastikan selalu bisa di-drag ulang saat level load
                    draggable.ResetState();
                }
            }
        }

        Debug.Log($"DragDropController: LoadLevel selesai — {k} pasangan drag-destiny di-setup untuk level {currentLevel}");
    }

    // ---------------------------------------------------------------------------
    // Spawn Helpers
    // ---------------------------------------------------------------------------

    private Image SpawnDragImage(int index) {
        Image template = dragImagePrefab;

        if (template == null && dragimage.Count > 0 && dragimage[0] != null)
            template = dragimage[0];

        if (template == null) {
            Debug.LogError("DragDropController: dragImagePrefab tidak di-set " +
                           "dan dragimage list kosong — tidak bisa spawn drag image baru.");
            return null;
        }

        Transform parent = dragImageParent;
        if (parent == null && dragimage.Count > 0 && dragimage[0] != null)
            parent = dragimage[0].transform.parent;
        if (parent == null)
            parent = transform;

        Image spawned = Instantiate(template, parent);
        spawned.gameObject.name = $"DragImage_{index}";
        spawned.gameObject.SetActive(true);

        while (dragimage.Count <= index)
            dragimage.Add(null);

        dragimage[index] = spawned;
        Debug.Log($"DragDropController: dragimage[{index}] di-spawn otomatis → {spawned.name}");
        return spawned;
    }

    private GameObject SpawnDestinyObject(int index) {
        GameObject template = destinyObjectPrefab;

        if (template == null && destinyobject.Count > 0 && destinyobject[0] != null)
            template = destinyobject[0];

        if (template == null) {
            Debug.LogError("DragDropController: destinyObjectPrefab tidak di-set " +
                           "dan destinyobject list kosong — tidak bisa spawn destiny object baru.");
            return null;
        }

        Transform parent = destinyObjectParent;
        if (parent == null && destinyobject.Count > 0 && destinyobject[0] != null)
            parent = destinyobject[0].transform.parent;
        if (parent == null)
            parent = transform;

        GameObject spawned = Instantiate(template, parent);
        spawned.name = $"DestinyObject_{index}";
        spawned.SetActive(true);

        while (destinyobject.Count <= index)
            destinyobject.Add(null);

        destinyobject[index] = spawned;
        Debug.Log($"DragDropController: destinyobject[{index}] di-spawn otomatis → {spawned.name}");
        return spawned;
    }

    // ---------------------------------------------------------------------------

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