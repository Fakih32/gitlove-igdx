using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Comic-style cutscene player — horizontal panel strip.
///
/// Hierarchy
/// ─────────
///  PanelParent (empty GO → assign to "Panel Parent"; deactivated at end)
///  └── [GameObject with ComicCutscenePlayer]
///      ├── PanelContainer  (RectTransform — slides left to reveal each slot)
///      │   ├── Slot_0  ← assign panelImages[0], bubbleImages[0], dialogueTexts[0]
///      │   ├── Slot_1  ← assign panelImages[1], bubbleImages[1], dialogueTexts[1]
///      │   └── …
///      └── NextButton   (shown at the last panel; deactivates PanelParent on click)
///
/// • All slots START hidden (script hides them on Awake).
/// • Each panel: slot activates → transition plays (fade/slide/pop) → wait for input.
/// • PanelContainer slides to the next slot position simultaneously with the transition.
/// </summary>
public class ComicCutscenePlayer : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────
    [Header("Layout")]
    [Tooltip("Holds all panel slots side-by-side; slides left to reveal each panel.")]
    [SerializeField] private RectTransform panelContainer;

    [Tooltip("Root empty GO for the entire cutscene UI — SetActive(false) when done.")]
    [SerializeField] private GameObject panelParent;

    [Header("Panel Slots  (parallel lists, same length)")]
    [SerializeField] private List<Image>    panelImages;
    [SerializeField] private List<Image>    bubbleImages;
    [SerializeField] private List<TMP_Text> dialogueTexts;

    [Header("End")]
    [SerializeField] private Button nextButton;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource voiceSource;

    [Header("Data")]
    [SerializeField] private CutsceneSequenceSO sequence;

    // ── Events ─────────────────────────────────────────────────────────────
    public event System.Action OnCutsceneComplete;

    // ── Runtime state ──────────────────────────────────────────────────────
    private int             currentIndex;
    private CanvasGroup[]   slotCanvasGroups;   // one per slot, for alpha transitions
    private float[]         slotPositionsX;     // cached editor-placed x positions of each slot root

    // ───────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Build CanvasGroup cache and hide every slot immediately
        slotCanvasGroups = new CanvasGroup[panelImages.Count];
        for (int i = 0; i < panelImages.Count; i++)
        {
            GameObject root = GetSlotRoot(i);
            if (root == null) continue;

            CanvasGroup cg = root.GetComponent<CanvasGroup>();
            if (cg == null) cg = root.AddComponent<CanvasGroup>();
            slotCanvasGroups[i] = cg;

            root.SetActive(false);   // hidden until it's this slot's turn
        }

        // Next button: hide, wire up click
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
            nextButton.onClick.AddListener(FinishCutscene);
        }
    }

    private void Start() => Play();

    // ── Public API ─────────────────────────────────────────────────────────
    public void Play()
    {
        if (panelParent != null) panelParent.SetActive(true);

        currentIndex = 0;

        // Reset container to origin
        if (panelContainer != null)
            panelContainer.anchoredPosition = Vector2.zero;

        // Pre-position and populate every slot (they stay hidden)
        PrepareAllSlots();

        // Start showing from panel 0
        StartCoroutine(ShowCurrentPanel());
    }

    // ── Slot setup (called once on Play) ───────────────────────────────────
    private void PrepareAllSlots()
    {
        if (sequence == null || sequence.panels == null) return;

        int count = Mathf.Min(sequence.panels.Length, panelImages.Count);
        slotPositionsX = new float[count];

        for (int i = 0; i < count; i++)
        {
            ComicPanelSO data = sequence.panels[i];
            if (data == null) continue;

            // Read the slot's position exactly as placed in the editor — do NOT move it
            GameObject root = GetSlotRoot(i);
            if (root != null)
            {
                RectTransform rt = root.GetComponent<RectTransform>();
                slotPositionsX[i] = rt != null ? rt.anchoredPosition.x : 0f;
            }

            // Fill content
            if (panelImages[i] != null)
                panelImages[i].sprite = data.panelImage;

            bool hasBubble = data.bubbleSprite != null;
            if (i < bubbleImages.Count && bubbleImages[i] != null)
            {
                bubbleImages[i].gameObject.SetActive(hasBubble);
                if (hasBubble) bubbleImages[i].sprite = data.bubbleSprite;
            }

            if (i < dialogueTexts.Count && dialogueTexts[i] != null)
                dialogueTexts[i].text = data.dialogueText;
        }
    }

    // ── Main panel loop ────────────────────────────────────────────────────
    private IEnumerator ShowCurrentPanel()
    {
        if (sequence == null || currentIndex >= sequence.panels.Length) yield break;

        ComicPanelSO panel = sequence.panels[currentIndex];
        if (panel == null) yield break;

        // ── 1. Activate this slot (it was hidden) ──
        GameObject slotRoot = GetSlotRoot(currentIndex);
        if (slotRoot != null) slotRoot.SetActive(true);

        // ── 2. Prepare slot for its entrance ──
        CanvasGroup cg   = GetSlotCG(currentIndex);
        PrepareEntrance(panel, currentIndex, cg);

        // ── 3. Audio ──
        if (panel.sfxOnEnter && sfxSource)   sfxSource.PlayOneShot(panel.sfxOnEnter);
        if (panel.voiceOver  && voiceSource)  voiceSource.PlayOneShot(panel.voiceOver);

        // ── 4. Slide container to this slot's editor-placed position ──
        float targetX = slotPositionsX != null && currentIndex < slotPositionsX.Length
            ? -slotPositionsX[currentIndex]
            : 0f;
        yield return StartCoroutine(SlideAndTransitionIn(panel, currentIndex, targetX, cg));

        // ── 5. Ken Burns (optional slow zoom) ──
        Coroutine kb = panel.useKenBurnsEffect
            ? StartCoroutine(KenBurns(panel, currentIndex))
            : null;

        // ── 6. Show Next button only at last panel ──
        bool isLast = (currentIndex == sequence.panels.Length - 1);
        if (nextButton != null) nextButton.gameObject.SetActive(isLast);

        if (!isLast)
        {
            // ── 7. Wait for click / timer ──
            yield return StartCoroutine(WaitForAdvance(panel));

            if (kb != null) StopCoroutine(kb);
            ResetSlotScale(currentIndex);

            // ── 8. Advance ──
            currentIndex++;
            yield return StartCoroutine(ShowCurrentPanel());
        }
        // If it IS the last panel → do nothing, Next button handles it.
    }

    // ── Slide container + per-slot entrance effect (run simultaneously) ──────
    // IMPORTANT: slot anchoredPosition is NEVER touched here.
    // Slots always stay at their fixed (i * panelWidth) resting position.
    // Only panelContainer.anchoredPosition moves to reveal the correct slot.
    private IEnumerator SlideAndTransitionIn(ComicPanelSO panel, int index,
                                              float targetX, CanvasGroup cg)
    {
        float dur          = Mathf.Max(panel.transitionDuration, 0.05f);
        float t            = 0f;
        Vector2 slideStart = panelContainer.anchoredPosition;
        Vector2 slideEnd   = new Vector2(targetX, 0f);

        while (t < dur)
        {
            float p = Mathf.SmoothStep(0f, 1f, t / dur);

            // Container slides to reveal the slot — THIS is the slide transition
            panelContainer.anchoredPosition = Vector2.Lerp(slideStart, slideEnd, p);

            // Per-slot entrance: alpha and/or scale only (no position change)
            switch (panel.transitionIn)
            {
                case PanelTransitionType.Fade:
                case PanelTransitionType.SlideFromRight:
                case PanelTransitionType.SlideFromLeft:
                case PanelTransitionType.SlideFromTop:
                case PanelTransitionType.SlideFromBottom:
                    // Fade in as the container slides into position
                    cg.alpha = Mathf.Lerp(0f, 1f, p);
                    break;

                case PanelTransitionType.ComicPop:
                    cg.alpha = Mathf.Lerp(0f, 1f, p);
                    panelImages[index].transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, p);
                    break;

                default: // None / PanelWipe → instant reveal
                    cg.alpha = 1f;
                    break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        // Snap container to exact target; snap slot alpha/scale (never touch anchoredPosition)
        panelContainer.anchoredPosition         = slideEnd;
        cg.alpha                                = 1f;
        panelImages[index].transform.localScale = Vector3.one;
    }

    // ── Wait for player to advance ──────────────────────────────────────────
    private IEnumerator WaitForAdvance(ComicPanelSO panel)
    {
        float elapsed = 0f;
        while (true)
        {
            bool clicked = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
            bool timeUp  = !panel.waitForInput && elapsed >= panel.displayDuration;
            if (clicked || timeUp) break;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // ── Ken-Burns slow zoom ─────────────────────────────────────────────────
    private IEnumerator KenBurns(ComicPanelSO panel, int index)
    {
        if (index >= panelImages.Count || panelImages[index] == null) yield break;
        Transform tr = panelImages[index].transform;
        float t = 0f;
        while (t < panel.displayDuration)
        {
            tr.localScale = Vector3.one * Mathf.Lerp(1f, panel.zoomAmount, t / panel.displayDuration);
            t += Time.deltaTime;
            yield return null;
        }
    }

    // ── Finish ──────────────────────────────────────────────────────────────
    private void FinishCutscene()
    {
        OnCutsceneComplete?.Invoke();
        if (panelParent != null)
            panelParent.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    /// <summary>Returns the slot's root GameObject (parent of the Image component).</summary>
    private GameObject GetSlotRoot(int index)
    {
        if (index < 0 || index >= panelImages.Count || panelImages[index] == null)
            return null;

        Transform parent = panelImages[index].transform.parent;
        // If the Image's parent is the panelContainer itself, the Image IS the slot root.
        return (parent != null && parent != panelContainer)
            ? parent.gameObject
            : panelImages[index].gameObject;
    }

    private CanvasGroup GetSlotCG(int index)
    {
        if (index >= 0 && index < slotCanvasGroups.Length && slotCanvasGroups[index] != null)
            return slotCanvasGroups[index];

        // Fallback: create one on the fly (Unity-safe null check — avoid ?? with Unity objects)
        GameObject root = GetSlotRoot(index);
        if (root == null) return null;
        var cg = root.GetComponent<CanvasGroup>();
        if (cg == null) cg = root.AddComponent<CanvasGroup>();
        if (index < slotCanvasGroups.Length) slotCanvasGroups[index] = cg;
        return cg;
    }

    /// <summary>
    /// Prepares a slot's alpha/scale for its entrance animation.
    /// Never touches anchoredPosition — slots stay at their fixed (i * panelWidth) position.
    /// </summary>
    private void PrepareEntrance(ComicPanelSO panel, int index, CanvasGroup cg)
    {
        // Start fully transparent (all transition types fade in)
        if (cg != null) cg.alpha = 0f;

        // ComicPop also starts scaled down
        if (panel.transitionIn == PanelTransitionType.ComicPop)
            panelImages[index].transform.localScale = Vector3.one * 0.5f;
        else
            panelImages[index].transform.localScale = Vector3.one;
    }

    

    private void ResetSlotScale(int index)
    {
        if (index >= 0 && index < panelImages.Count && panelImages[index] != null)
            panelImages[index].transform.localScale = Vector3.one;
    }
}