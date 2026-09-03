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
/// • Each ComicPanelSO entry in sequence.panels maps to a SLOT, not 1:1 by index:
///     - panel.isNextPanel == true  → advance to the NEXT slot and play its entrance.
///     - panel.isNextPanel == false → STAY on the current slot; refresh its sprite /
///       bubble / dialogue in place and replay the entrance transition on that slot.
///   (The very first panel in the sequence always lands on Slot_0.)
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
    // currentPanelIndex walks through sequence.panels[] one entry at a time (always +1).
    // currentSlotIndex is which physical slot (Slot_0, Slot_1, ...) is currently showing.
    // These are DIFFERENT counters: a panel entry with isNextPanel == false reuses the
    // same slot as the previous entry instead of advancing to a new one.
    private int             currentPanelIndex;
    private int             currentSlotIndex;
    private CanvasGroup[]   slotCanvasGroups;   // one per slot, for alpha transitions
    private Vector2[]       slotRestingPositions; // cached design-time anchoredPositions of each slot root
    private CanvasGroup     chatPanelCG;        // CanvasGroup on the chatPanel GO
    private Vector2         chatPanelRestPos;   // editor-placed resting anchoredPosition of chatPanel

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

        // Cache chat panel CanvasGroup and its resting position
        

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

        currentPanelIndex = 0;
        currentSlotIndex  = -1; // becomes 0 the moment the first panel is shown

        // Reset container to origin
        if (panelContainer != null)
            panelContainer.anchoredPosition = Vector2.zero;

        // Cache each slot's design-time resting position (no data pushed here anymore —
        // which slot a given ComicPanelSO lands on is decided at runtime by isNextPanel).
        PrepareAllSlots();

        // Start showing from panel 0
        StartCoroutine(ShowCurrentPanel());
    }

    private void PrepareAllSlots()
    {
        int slotCount = panelImages.Count;
        slotRestingPositions = new Vector2[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            GameObject root = GetSlotRoot(i);
            RectTransform rt = root != null
                ? root.GetComponent<RectTransform>()
                : null;

            // IMPORTANT:
            // Use the position that you placed in the Inspector.
            // Do NOT calculate it from the previous panel.
            slotRestingPositions[i] = rt != null
                ? rt.anchoredPosition
                : Vector2.zero;
        }
    }

    // ── Main panel loop ────────────────────────────────────────────────────
    private IEnumerator ShowCurrentPanel()
    {
        if (sequence == null || currentPanelIndex >= sequence.panels.Length) yield break;

        ComicPanelSO panel = sequence.panels[currentPanelIndex];
        if (panel == null) yield break;

        // ── 0. Decide which slot this entry belongs on ──
        // First panel ever -> slot 0.
        // isNextPanel == true  -> advance to a new slot.
        // isNextPanel == false -> stay on the current slot, just refresh its content.
        if (currentSlotIndex < 0 || panel.isNextPanel)
            currentSlotIndex++;

        if (currentSlotIndex >= panelImages.Count)
        {
            Debug.LogWarning(
                $"ComicCutscenePlayer: panel {currentPanelIndex} wants slot {currentSlotIndex}, " +
                $"but only {panelImages.Count} slots exist. Stopping.");
            yield break;
        }

        int slot = currentSlotIndex;

        // ── 1. Push this panel's content onto the target slot ──
        if (!panel.stayonthefirstslot)
        {
            
        
        if (panelImages[slot] != null)
            panelImages[slot].sprite = panel.panelImage;
        
       
        

        

        // ── 2. Activate this slot (harmless if it's already active because it's being reused) ──
        GameObject slotRoot = GetSlotRoot(slot);
        if (slotRoot != null) slotRoot.SetActive(true);

        // ── 3. Prepare slot for its entrance ──
        CanvasGroup cg = GetSlotCG(slot);
        PrepareEntrance(panel, slot, cg);
        
    
        
        // ── 4. Audio ──
        if (panel.sfxOnEnter && sfxSource)   sfxSource.PlayOneShot(panel.sfxOnEnter);
        if (panel.voiceOver  && voiceSource) voiceSource.PlayOneShot(panel.voiceOver);

        // ── 5. Keep the container fixed ──
        Vector2 targetPos = Vector2.zero;

        yield return StartCoroutine(
            SlideAndTransitionIn(panel, slot, targetPos, cg)
        );
        }
        else
        {
             
              panelImages[0].sprite = panel.panelImage;
               bool hasBubble = panel.bubbleSprite != null;
       
        }
        // ── 6. Ken Burns (optional slow zoom) ──
        Coroutine kb = panel.useKenBurnsEffect
            ? StartCoroutine(KenBurns(panel, slot))
            : null;

        // ── 7. Show Next button only after the LAST data entry, not the last slot ──
        bool isLast = (currentPanelIndex == sequence.panels.Length - 1);
        if (nextButton != null) nextButton.gameObject.SetActive(isLast);

        if (!isLast)
        {
            // ── 8. Wait for click / timer ──
            yield return StartCoroutine(WaitForAdvance(panel));

            if (kb != null) StopCoroutine(kb);
            ResetSlotScale(slot);

            // ── 9. Advance to the next data entry (slot may or may not change) ──
            currentPanelIndex++;
            yield return StartCoroutine(ShowCurrentPanel());
        }
        // If it IS the last panel → do nothing, Next button handles it.
    }

    // ── Slide container + per-slot entrance effect (run simultaneously) ──────
    // IMPORTANT: panelContainer is snapped to the correct targetPos instantly,
    // and the individual slot entrance transition is played relative to its resting position.
    // Chat panel runs on its own independent chatPanelTransitionDuration timer.
    private IEnumerator SlideAndTransitionIn(ComicPanelSO panel, int index,
                                              Vector2 targetPos, CanvasGroup cg)
    {
        float panelDur     = Mathf.Max(panel.transitionDuration, 0.05f);
        float chatDur      = Mathf.Max(panel.chatPanelTransitionDuration, 0.05f);
        float totalDur     = Mathf.Max(panelDur, chatDur);
        float t            = 0f;
        Vector2 slideEnd   = targetPos;

        // Snap container to reveal the target slot position instantly
        if (panelContainer != null)
        {
            panelContainer.anchoredPosition = slideEnd;
        }

        GameObject slotRoot = GetSlotRoot(index);
        RectTransform slotRt = slotRoot != null ? slotRoot.GetComponent<RectTransform>() : null;
        Vector2 slotRestPos = (slotRestingPositions != null && index < slotRestingPositions.Length)
            ? slotRestingPositions[index]
            : Vector2.zero;

        while (t < totalDur)
        {
            float p      = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / panelDur));
            float pChat  = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / chatDur));

            // Slot entrance animation
            if (slotRt != null)
            {
                float offsetWidth = slotRt.rect.width > 0f ? slotRt.rect.width : Screen.width;
                float offsetHeight = slotRt.rect.height > 0f ? slotRt.rect.height : Screen.height;

                switch (panel.transitionIn)
                {
                    case PanelTransitionType.Fade:
                        cg.alpha = Mathf.Lerp(0f, 1f, p);
                        slotRt.anchoredPosition = slotRestPos;
                        break;

                    case PanelTransitionType.ComicPop:
                        cg.alpha = Mathf.Lerp(0f, 1f, p);
                        panelImages[index].transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, p);
                        slotRt.anchoredPosition = slotRestPos;
                        break;

                    case PanelTransitionType.SlideFromRight:
                        cg.alpha = Mathf.Lerp(0f, 1f, p);
                        slotRt.anchoredPosition = Vector2.Lerp(
                            slotRestPos + new Vector2(offsetWidth, 0f),
                            slotRestPos, p);
                        break;

                    case PanelTransitionType.SlideFromLeft:
                        cg.alpha = Mathf.Lerp(0f, 1f, p);
                        slotRt.anchoredPosition = Vector2.Lerp(
                            slotRestPos - new Vector2(offsetWidth, 0f),
                            slotRestPos, p);
                        break;

                    case PanelTransitionType.SlideFromTop:
                        cg.alpha = Mathf.Lerp(0f, 1f, p);
                        slotRt.anchoredPosition = Vector2.Lerp(
                            slotRestPos + new Vector2(0f, offsetHeight),
                            slotRestPos, p);
                        break;

                    case PanelTransitionType.SlideFromBottom:
                        cg.alpha = Mathf.Lerp(0f, 1f, p);
                        slotRt.anchoredPosition = Vector2.Lerp(
                            slotRestPos - new Vector2(0f, offsetHeight),
                            slotRestPos, p);
                        break;

                    default: // None / PanelWipe → instant reveal
                        cg.alpha = 1f;
                        slotRt.anchoredPosition = slotRestPos;
                        break;
                }
            }

           
            t += Time.deltaTime;
            yield return null;
        }

        // Snap everything to final state
        if (panelContainer != null)
        {
            panelContainer.anchoredPosition = slideEnd;
        }
        cg.alpha                                = 1f;
        panelImages[index].transform.localScale = Vector3.one;
        if (slotRt != null)
        {
            slotRt.anchoredPosition = slotRestPos;
        }
       
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
    /// Also resets the chat panel to its pre-transition starting state.
    /// </summary>
    private void PrepareEntrance(ComicPanelSO panel, int index, CanvasGroup cg)
    {
        // Start fully transparent (all transition types fade in)
        if (cg != null) cg.alpha = 0f;

        // Reset/prepare slot position and scale based on transition type
        GameObject root = GetSlotRoot(index);
        if (root != null)
        {
            RectTransform rt = root.GetComponent<RectTransform>();
            if (rt != null && slotRestingPositions != null && index < slotRestingPositions.Length)
            {
                Vector2 restPos = slotRestingPositions[index];
                float offsetWidth = rt.rect.width > 0f ? rt.rect.width : Screen.width;
                float offsetHeight = rt.rect.height > 0f ? rt.rect.height : Screen.height;

                switch (panel.transitionIn)
                {
                    case PanelTransitionType.SlideFromRight:
                        rt.anchoredPosition = restPos + new Vector2(offsetWidth, 0f);
                        break;
                    case PanelTransitionType.SlideFromLeft:
                        rt.anchoredPosition = restPos - new Vector2(offsetWidth, 0f);
                        break;
                    case PanelTransitionType.SlideFromTop:
                        rt.anchoredPosition = restPos + new Vector2(0f, offsetHeight);
                        break;
                    case PanelTransitionType.SlideFromBottom:
                        rt.anchoredPosition = restPos - new Vector2(0f, offsetHeight);
                        break;
                    default:
                        rt.anchoredPosition = restPos;
                        break;
                }
            
        }

        // ComicPop also starts scaled down
        if (panel.transitionIn == PanelTransitionType.ComicPop)
            panelImages[index].transform.localScale = Vector3.one * 0.5f;
        else
            panelImages[index].transform.localScale = Vector3.one;

        // Reset chat panel to its pre-transition state
        
        }
    }

    private void ResetSlotScale(int index)
    {
        if (index >= 0 && index < panelImages.Count && panelImages[index] != null)
            panelImages[index].transform.localScale = Vector3.one;
    }
}