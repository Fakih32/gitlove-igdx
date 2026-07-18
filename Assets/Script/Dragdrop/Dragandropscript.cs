using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Dragandropscript : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Canvas rootCanvas;
    public static Dragandropscript instance;
    public GameObject objectawal;
    public RectTransform tujuan;
    private Vector3 startPosition;         // initial world position of objectawal
    private Vector2 dragOffset;
    void Start()
    {
        if (objectawal != null)
        {
            var rect = objectawal.GetComponent<RectTransform>();
            if (rect != null)
                startPosition = rect.position;
        }
    }

    // --- Drag event handlers ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransform rect = objectawal.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out dragOffset
        );

        dragOffset = rect.localPosition - (Vector3)dragOffset;
    }
    public void OnDrag(PointerEventData eventData)
    {
        RectTransform rect = objectawal.GetComponent<RectTransform>();

        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        rect.localPosition = localPoint + dragOffset;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        RectTransform transformawal = objectawal.GetComponent<RectTransform>();

        // --- Determine if the drop is successful ---
        bool isOverTarget = IsOverTargetWithMargin(transformawal, tujuan, margin: 15f);

        // --- Evaluate conditions (order matters) ---
    }
    private bool IsOverTargetWithMargin(RectTransform dragged, RectTransform target, float margin = 0f)
    {
        if (dragged == null || target == null) return false;

        // Get world corners of both rects (order: bottom-left, top-left, top-right, bottom-right)
        Vector3[] draggedCorners = new Vector3[4];
        Vector3[] targetCorners = new Vector3[4];
        dragged.GetWorldCorners(draggedCorners);
        target.GetWorldCorners(targetCorners);

        // Convert to Rect (world space)
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
