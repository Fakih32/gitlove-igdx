using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// BARU. Sepenuhnya independen dari OtherUIlevelselection -- tidak
// bergantung ke script itu sama sekali, baik untuk buka/tutup panel
// maupun isi kontennya. BookController urus semuanya sendiri: tombol
// buka buku, tombol tutup buku, populate grid stiker, dan panel detail.
//
// Alasan dipisah total: OtherUIlevelselection itu scriptnya rekan kalian
// (urus navigasi Main Menu, Setting), sedangkan Book adalah fitur milik
// kalian sendiri -- supaya tidak ada dua orang mengedit file yang sama,
// dan supaya logic Book tidak tercampur dengan concern lain yang tidak
// berhubungan.
public class BookController : MonoBehaviour {

    public const string LockedTitle = "???";

    [System.Serializable]
    public struct SlotViewData {
        public Sprite sprite;
        public string title;
        public string description;
        public bool interactable;
    }

    private static readonly ScoreTier[] TierOrder = { ScoreTier.OneStar, ScoreTier.TwoStar, ScoreTier.ThreeStar };

    [Header("Tombol Buka/Tutup Buku (independen, tidak pakai punya OtherUIlevelselection)")]
    public Button openBookButton;
    public Button closeBookButton;
    public GameObject bookPanel;

    [Header("Sumber Data")]
    public LevelDatabase levelDatabase;
    public StickerDatabase stickerDatabase;

    [Header("Grid Slot")]
    public Transform slotsParent;
    public GameObject stickerSlotPrefab;

    [Header("Panel Detail")]
    public GameObject detailPanel;
    public Image detailImage;
    public Text detailTitleText;
    public Text detailDescriptionText;

    void Awake() {
        if (openBookButton != null) {
            openBookButton.onClick.AddListener(OpenBook);
        }
        if (closeBookButton != null) {
            closeBookButton.onClick.AddListener(CloseBook);
        }

        if (bookPanel != null) {
            bookPanel.SetActive(false);
        }
        if (detailPanel != null) {
            detailPanel.SetActive(false);
        }
    }

    public void OpenBook() {
        if (bookPanel == null) return;

        bookPanel.SetActive(GetPanelActiveState(shouldOpen: true));
        PopulateBook();
    }

    public void CloseBook() {
        if (bookPanel == null) return;

        bookPanel.SetActive(GetPanelActiveState(shouldOpen: false));
        CloseDetail();
    }

    public void PopulateBook() {
        if (levelDatabase == null || stickerDatabase == null) {
            Debug.LogError("BookController: levelDatabase/stickerDatabase belum di-assign.");
            return;
        }
        if (slotsParent == null || stickerSlotPrefab == null) {
            Debug.LogError("BookController: slotsParent/stickerSlotPrefab belum di-assign.");
            return;
        }

        foreach (Transform child in slotsParent) {
            Destroy(child.gameObject);
        }

        LevelData[] sortedLevels = SortedLevels(levelDatabase.levels);

        foreach (var level in sortedLevels) {
            if (level == null) continue;

            foreach (var tier in TierOrder) {
                bool unlocked = StickerCollection.IsUnlocked(level.levelId, tier);
                SlotViewData viewData = ResolveSlotViewData(level.levelId, tier, stickerDatabase, unlocked);

                CreateSlot(viewData);
            }
        }
    }

    void CreateSlot(SlotViewData viewData) {
        GameObject slot = Instantiate(stickerSlotPrefab, slotsParent);
        slot.SetActive(true);

        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null) {
            slotImage.sprite = viewData.sprite;
        }

        Button slotButton = slot.GetComponent<Button>();
        if (slotButton != null) {
            slotButton.interactable = viewData.interactable;
            slotButton.onClick.RemoveAllListeners();

            if (viewData.interactable) {
                slotButton.onClick.AddListener(() => ShowDetail(viewData));
            }
        }
    }

    void ShowDetail(SlotViewData viewData) {
        if (detailPanel == null) return;

        detailPanel.SetActive(true);

        if (detailImage != null) detailImage.sprite = viewData.sprite;
        if (detailTitleText != null) detailTitleText.text = viewData.title;
        if (detailDescriptionText != null) detailDescriptionText.text = viewData.description;
    }

    public void CloseDetail() {
        if (detailPanel != null) {
            detailPanel.SetActive(false);
        }
    }

    public static bool GetPanelActiveState(bool shouldOpen) {
        return shouldOpen;
    }

    public static LevelData[] SortedLevels(LevelData[] levels) {
        if (levels == null) return new LevelData[0];
        return levels.Where(l => l != null).OrderBy(l => l.levelIndex).ToArray();
    }

    public static SlotViewData ResolveSlotViewData(string levelId, ScoreTier tier, StickerDatabase database, bool unlocked) {
        if (!unlocked) {
            return new SlotViewData {
                sprite = database.lockedSprite,
                title = LockedTitle,
                description = "",
                interactable = false
            };
        }

        StickerDatabase.StickerEntry entry = database.GetEntry(levelId, tier);

        if (entry == null) {
            Debug.LogWarning($"BookController: sticker '{levelId}'/{tier} sudah unlocked tapi belum ada entry di StickerDatabase.");
            return new SlotViewData {
                sprite = database.lockedSprite,
                title = LockedTitle,
                description = "",
                interactable = false
            };
        }

        return new SlotViewData {
            sprite = entry.unlockedSprite,
            title = entry.title,
            description = entry.description,
            interactable = true
        };
    }
}