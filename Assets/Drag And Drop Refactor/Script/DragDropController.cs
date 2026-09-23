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

    [Header("Konfigurasi Mekanik Ini")]
    [Tooltip("Jumlah quiz yang dimainkan per level. Jika 0, akan otomatis memainkan seluruh quiz yang ada di DDquiz level ini.")]
    public int questionsPerLevel = 0;

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

    private DragDropLevelData.DDquiz[] currentLevelQuizzes;
    private int currentQuizIndex = 0;
    private int questionsAnswered = 0;

    void Awake() {
        Instance = this;
        currentLevel = ResolveCurrentLevelIndex(LevelSessionManager.Instance?.currentLevel);
    }

    void Start() {
        LoadLevel();
    }

    public void LoadLevelIndex(int levelIndex) {
        currentLevel = levelIndex;
        LoadLevel();
    }

    public void NextLevel() {
        LoadLevelIndex(currentLevel + 1);
    }

    void LoadLevel() {
        if (levelData == null) {
            Debug.LogError("DragDropController: levelData belum di-set");
            return;
        }

        currentLevelQuizzes = levelData.getquizdragdrop(currentLevel);
        Sprite bg = levelData.getspritebg(currentLevel);

        if (currentLevelQuizzes == null || currentLevelQuizzes.Length == 0) {
            Debug.LogWarning($"DragDropController: Level {currentLevel} tidak ditemukan atau tidak punya quiz.");
            return;
        }

        // Set background
        if (backgroundImage != null)
            backgroundImage.sprite = bg;
        else
            Debug.LogWarning("DragDropController: backgroundImage is not assigned in the Inspector!");

        if (questionsPerLevel > currentLevelQuizzes.Length) {
            Debug.LogWarning($"DragDropController: questionsPerLevel ({questionsPerLevel}) lebih besar dari jumlah quiz yang tersedia ({currentLevelQuizzes.Length}) untuk level ini -- soal akan berulang.");
        }

        currentQuizIndex = 0;
        questionsAnswered = 0;
        LoadQuiz(currentQuizIndex);
    }

    void LoadQuiz(int quizIndex) {
        CancelInvoke(nameof(NextQuiz));

        if (currentLevelQuizzes == null || currentLevelQuizzes.Length == 0) {
            Debug.LogError("DragDropController: currentLevelQuizzes belum di-setup dengan benar");
            return;
        }

        targets.Clear();
        targetsHit = 0;

        // Warning check for LayoutGroup interference on parents
        if (dragImageParent != null && dragImageParent.GetComponent<LayoutGroup>() != null) {
            Debug.LogWarning("DragDropController WARNING: dragImageParent has a LayoutGroup component (Grid/Horizontal/Vertical Layout). Unity Layout Groups WILL OVERRIDE and force object positions to layout slots, ignoring Dragpos data!");
        }
        if (destinyObjectParent != null && destinyObjectParent.GetComponent<LayoutGroup>() != null) {
            Debug.LogWarning("DragDropController WARNING: destinyObjectParent has a LayoutGroup component (Grid/Horizontal/Vertical Layout). Unity Layout Groups WILL OVERRIDE and force object positions to layout slots, ignoring Destinypos data!");
        }

        DragDropLevelData.DDquiz quiz = currentLevelQuizzes[quizIndex % currentLevelQuizzes.Length];

        if (quiz.Dragobject == null || quiz.Dragobject.Count == 0) {
            Debug.LogWarning($"DragDropController: DDquiz[{quizIndex}] tidak punya Dragobject.");
            return;
        }

        int itemCount = quiz.Dragobject.Count;

        // Helper untuk data non-posisi (sprite, scale): fallback ke 0 jika list hanya 1
        T SafeGet<T>(List<T> list, int idx) => (list != null && list.Count > idx) ? list[idx]
                                              : (list != null && list.Count > 0) ? list[0]
                                              : default;

        // Helper khusus posisi: HANYA ambil jika index idx benar-benar ada
        bool HasPosAt(List<Vector2> list, int idx, out Vector2 pos) {
            if (list != null && idx >= 0 && idx < list.Count) {
                pos = list[idx];
                return true;
            }
            pos = Vector2.zero;
            return false;
        }

        for (int j = 0; j < itemCount; j++) {
            // --- Pastikan dragimage[j] ada ---
            if (j >= dragimage.Count || dragimage[j] == null) {
                if (SpawnDragImage(j) == null) {
                    Debug.LogWarning($"DragDropController: gagal spawn dragimage[{j}] (quiz[{quizIndex}] item[{j}])");
                    continue;
                }
            }

            // --- Pastikan destinyobject[j] ada ---
            if (j >= destinyobject.Count || destinyobject[j] == null) {
                if (SpawnDestinyObject(j) == null) {
                    Debug.LogWarning($"DragDropController: gagal spawn destinyobject[{j}] (quiz[{quizIndex}] item[{j}])");
                    continue;
                }
            }

            // --- Apply data drag image ---
            {
                Image img = dragimage[j];
                img.gameObject.SetActive(true);
                RectTransform imgRect = img.GetComponent<RectTransform>();

                Sprite dragSprite = SafeGet(quiz.Dragobject, j);
                if (dragSprite != null) img.sprite = dragSprite;

                Vector2 scale = SafeGet(quiz.Imagescale, j);
                if (scale != default)
                    imgRect.localScale = new Vector3(scale.x, scale.y, 1f);

                if (HasPosAt(quiz.Dragpos, j, out Vector2 dragPos)) {
                    imgRect.anchoredPosition = dragPos;
                    Debug.Log($"DragDropController: dragimage[{j}] (quiz[{quizIndex}] item[{j}]) Dragpos set to {dragPos}");
                } else {
                    Debug.LogWarning($"DragDropController: quiz[{quizIndex}] item[{j}] tidak punya entry Dragpos di index [{j}]. Posisi saat ini: {imgRect.anchoredPosition}");
                }
            }

            // --- Apply data destiny object ---
            {
                GameObject dest = destinyobject[j];
                dest.SetActive(true);
                Image destImg = dest.GetComponent<Image>();
                RectTransform destRect = dest.GetComponent<RectTransform>();

                Sprite siluet = SafeGet(quiz.SiluetDrag, j);
                if (destImg != null && siluet != null)
                    destImg.sprite = siluet;

                if (destRect != null) {
                    bool hasScale = quiz.DestinyImagescale != null && quiz.DestinyImagescale.Count > 0;
                    if (hasScale) {
                        Vector2 destScale = SafeGet(quiz.DestinyImagescale, j);
                        destRect.localScale = new Vector3(destScale.x, destScale.y, 1f);
                    }

                    if (HasPosAt(quiz.Destinypos, j, out Vector2 destPos)) {
                        destRect.anchoredPosition = destPos;
                        Debug.Log($"DragDropController: destinyobject[{j}] (quiz[{quizIndex}] item[{j}]) Destinypos set to {destPos}");
                    } else {
                        Debug.LogWarning($"DragDropController: quiz[{quizIndex}] item[{j}] tidak punya entry Destinypos di index [{j}]. Posisi saat ini: {destRect.anchoredPosition}");
                    }
                }

                // Daftarkan ke targets list agar targetsTotal otomatis sesuai
                if (!targets.Contains(dest.transform))
                    targets.Add(dest.transform);
            }

            // --- Wire DraggableItem ---
            {
                Image img = dragimage[j];
                DraggableItem draggable = img.GetComponent<DraggableItem>();
                if (draggable == null)
                    draggable = img.gameObject.AddComponent<DraggableItem>();

                draggable.draggedObject = img.gameObject;
                draggable.target = destinyobject[j].GetComponent<RectTransform>();
                draggable.rootCanvas = img.GetComponentInParent<Canvas>();

                // Setup start position and reset state
                RectTransform imgRect = img.GetComponent<RectTransform>();
                draggable.SetStartPosition(imgRect.anchoredPosition);
                draggable.ResetState();
            }
        }

        // Deactivate unused dragimage and destinyobject instances for this quiz
        for (int i = itemCount; i < dragimage.Count; i++) {
            if (dragimage[i] != null) dragimage[i].gameObject.SetActive(false);
        }
        for (int i = itemCount; i < destinyobject.Count; i++) {
            if (destinyobject[i] != null) destinyobject[i].SetActive(false);
        }

        targetsTotal = targets.Count;
        Debug.Log($"DragDropController: LoadQuiz[{quizIndex}] selesai — {itemCount} item drag-destiny di-setup untuk level {currentLevel}");
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

        if (targetsHit >= targetsTotal) {
            LevelSessionManager.Instance?.AddScore(100);
            Invoke(nameof(NextQuiz), 1.0f);
        }
    }

    void NextQuiz() {
        questionsAnswered++;
        currentQuizIndex++;

        int totalQuestions = (questionsPerLevel > 0 && questionsPerLevel <= currentLevelQuizzes.Length) 
            ? questionsPerLevel 
            : currentLevelQuizzes.Length;

        if (questionsAnswered < totalQuestions) {
            LoadQuiz(currentQuizIndex);
        } else {
            Debug.Log($"DragDropController: Selesai {questionsAnswered} quiz untuk level {currentLevel}. Menyelesaikan mekanik...");
            if (LevelSessionManager.Instance != null) {
                LevelSessionManager.Instance.OnMechanicComplete();
            } else {
                Debug.LogWarning("DragDropController: LevelSessionManager.Instance is null. Pastikan jalankan dari MainMenu/LevelSelection agar bisa pindah scene otomatis.");
            }
        }
    }

    public static int ResolveCurrentLevelIndex(LevelData currentLevel) {
        if (currentLevel == null) {
            Debug.LogWarning("DragDropController: LevelSessionManager.currentLevel null, fallback ke level index 0");
            return 0;
        }
        return currentLevel.levelIndex;
    }
}