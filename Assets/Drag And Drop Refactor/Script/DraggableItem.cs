using UnityEngine;
using UnityEngine.EventSystems;

// ROMBAK dari Dragandropscript.cs.
// Perubahan utama:
// 1. Nama diganti: ini nempel ke 1 item yang di-drag, bukan sistem
//    drag-and-drop secara keseluruhan, jadi "Item" lebih pas dari "script"
// 2. AudioScript.instance -> AudioManager.Instance, dan AudioClip-nya
//    sekarang jadi field milik script ini sendiri (dragStartSfx, dropSfx,
//    correctSfx), bukan nempel di AudioManager -> pola yang sama kayak
//    WordQuizController, biar AudioManager cuma "pemutar", bukan penyimpan klip
// 3. DraganddropLevelHandler.instance.tujuancount += 1 + Addpoint()
//    -> DragDropController.Instance.OnTargetHit() (1 pemanggilan)
public class DraggableItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
    public Canvas rootCanvas;
    public GameObject draggedObject;
    public RectTransform target;

    [Header("Sound Effects")]
    public AudioClip dragStartSfx;
    public AudioClip dropSfx;
    public AudioClip correctSfx;

    private Vector3 startPosition;
    private Vector2 dragOffset;
    private bool placedOnTarget = false;

    void Start() {
        if (draggedObject != null) {
            var rect = draggedObject.GetComponent<RectTransform>();
            if (rect != null) startPosition = rect.position;
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (placedOnTarget) return;

        RectTransform rect = draggedObject.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out dragOffset
        );

        dragOffset = rect.localPosition - (Vector3)dragOffset;
        AudioManager.Instance?.PlaySfx(dragStartSfx);
    }

    public void OnDrag(PointerEventData eventData) {
        if (placedOnTarget) return;

        RectTransform rect = draggedObject.GetComponent<RectTransform>();
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        rect.localPosition = localPoint + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData) {
        AudioManager.Instance?.PlaySfx(dropSfx);

        if (placedOnTarget) return;

        RectTransform draggedRect = draggedObject.GetComponent<RectTransform>();

        if (IsOverTargetWithMargin(draggedRect, target, margin: 15f)) {
            draggedObject.transform.position = target.position;
            placedOnTarget = true;

            DragDropController.Instance?.OnTargetHit();
            AudioManager.Instance?.PlaySfx(correctSfx);
        }
    }

    private bool IsOverTargetWithMargin(RectTransform dragged, RectTransform target, float margin = 0f) {
        if (dragged == null || target == null) return false;

        Vector3[] draggedCorners = new Vector3[4];
        Vector3[] targetCorners = new Vector3[4];
        dragged.GetWorldCorners(draggedCorners);
        target.GetWorldCorners(targetCorners);

        Rect draggedRect = new Rect(
            draggedCorners[0].x - margin,
            draggedCorners[0].y - margin,
            draggedCorners[2].x - draggedCorners[0].x + 2 * margin,
            draggedCorners[2].y - draggedCorners[0].y + 2 * margin
        );
        Rect targetRect = new Rect(
            targetCorners[0].x,
            targetCorners[0].y,
            targetCorners[2].x - targetCorners[0].x,
            targetCorners[2].y - targetCorners[0].y
        );

        return draggedRect.Overlaps(targetRect);
    }
}
