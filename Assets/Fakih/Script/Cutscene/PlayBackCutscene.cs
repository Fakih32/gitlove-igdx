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

    [Tooltip("The chat/dialogue bubble panel (RectTransform — needs a CanvasGroup on the same GO).")]
    [SerializeField] private RectTransform chatPanel;

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
        if (chatPanel != null)
        {
            chatPanelCG = chatPanel.GetComponent<CanvasGroup>();
            if (chatPanelCG == null) chatPanelCG = chatPanel.gameObject.AddComponent<CanvasGroup>();
            chatPanelRestPos = chatPanel.anchoredPosition;
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

 private void PrepareAllSlots()
{
    if (sequence == null || sequence.panels == null) return;

    int count = Mathf.Min(sequence.panels.Length, panelImages.Count);
    slotRestingPositions = new Vector2[count];

    for (int i = 0; i < count; i++)
    {
        ComicPanelSO data = sequence.panels[i];
        if (data == null) continue;

        GameObject root = GetSlotRoot(i);
        RectTransform rt = root != null
            ? root.GetComponent<RectTransform>()
            : null;

        // IMPORTANT:
        // Use the position that you placed in the Inspector.
        // Do NOT calculate it from the previous panel.
        Vector2 restPos = rt != null
            ? rt.anchoredPosition
            : Vector2.zero;

        slotRestingPositions[i] = restPos;

        // Set panel sprite
        if (panelImages[i] != null)
            panelImages[i].sprite = data.panelImage;

        // Bubble
        bool hasBubble = data.bubbleSprite != null;

        if (i < bubbleImages.Count && bubbleImages[i] != null)
        {
            bubbleImages[i].gameObject.SetActive(hasBubble);

            if (hasBubble)
                bubbleImages[i].sprite = data.bubbleSprite;
        }

        // Dialogue
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

        // ── 4. Slide container to this slot's position ──
       // 4. Keep the container fixed
Vector2 targetPos = Vector2.zero;

yield return StartCoroutine(
    SlideAndTransitionIn(panel, currentIndex, targetPos, cg)
);
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

            // Chat panel entrance — uses its own pChat progress
            if (chatPanel != null && chatPanelCG != null)
            {
                switch (panel.chatPanelTransitionType)
                {
                    case ChatPanelTransitionType.Fade:
                        chatPanelCG.alpha = Mathf.Lerp(0f, 1f, pChat);
                        break;

                    case ChatPanelTransitionType.Pop:
                        chatPanelCG.alpha = Mathf.Lerp(0f, 1f, pChat);
                        chatPanel.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, pChat);
                        break;

                    case ChatPanelTransitionType.SlideFromRight:
                        chatPanelCG.alpha = Mathf.Lerp(0f, 1f, pChat);
                        chatPanel.anchoredPosition = Vector2.Lerp(
                            chatPanelRestPos + new Vector2(chatPanel.rect.width, 0f),
                            chatPanelRestPos, pChat);
                        break;

                    case ChatPanelTransitionType.SlideFromLeft:
                        chatPanelCG.alpha = Mathf.Lerp(0f, 1f, pChat);
                        chatPanel.anchoredPosition = Vector2.Lerp(
                            chatPanelRestPos - new Vector2(chatPanel.rect.width, 0f),
                            chatPanelRestPos, pChat);
                        break;

                    case ChatPanelTransitionType.SlideFromTop:
                        chatPanelCG.alpha = Mathf.Lerp(0f, 1f, pChat);
                        chatPanel.anchoredPosition = Vector2.Lerp(
                            chatPanelRestPos + new Vector2(0f, chatPanel.rect.height),
                            chatPanelRestPos, pChat);
                        break;

                    case ChatPanelTransitionType.SlideFromBottom:
                        chatPanelCG.alpha = Mathf.Lerp(0f, 1f, pChat);
                        chatPanel.anchoredPosition = Vector2.Lerp(
                            chatPanelRestPos - new Vector2(0f, chatPanel.rect.height),
                            chatPanelRestPos, pChat);
                        break;

                    default: // None
                        chatPanelCG.alpha = 1f;
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
        if (chatPanel != null && chatPanelCG != null)
        {
            chatPanelCG.alpha            = 1f;
            chatPanel.localScale         = Vector3.one;
            chatPanel.anchoredPosition   = chatPanelRestPos;
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
        }

        // ComicPop also starts scaled down
        if (panel.transitionIn == PanelTransitionType.ComicPop)
            panelImages[index].transform.localScale = Vector3.one * 0.5f;
        else
            panelImages[index].transform.localScale = Vector3.one;

        // Reset chat panel to its pre-transition state
        if (chatPanel != null && chatPanelCG != null)
        {
            chatPanel.localScale = Vector3.one;
            switch (panel.chatPanelTransitionType)
            {
                case ChatPanelTransitionType.None:
                    chatPanelCG.alpha          = 1f;
                    chatPanel.anchoredPosition = chatPanelRestPos;
                    break;
                case ChatPanelTransitionType.Pop:
                    chatPanelCG.alpha          = 0f;
                    chatPanel.localScale       = Vector3.one * 0.5f;
                    chatPanel.anchoredPosition = chatPanelRestPos;
                    break;
                case ChatPanelTransitionType.SlideFromRight:
                    chatPanelCG.alpha          = 0f;
                    chatPanel.anchoredPosition = chatPanelRestPos + new Vector2(chatPanel.rect.width, 0f);
                    break;
                case ChatPanelTransitionType.SlideFromLeft:
                    chatPanelCG.alpha          = 0f;
                    chatPanel.anchoredPosition = chatPanelRestPos - new Vector2(chatPanel.rect.width, 0f);
                    break;
                case ChatPanelTransitionType.SlideFromTop:
                    chatPanelCG.alpha          = 0f;
                    chatPanel.anchoredPosition = chatPanelRestPos + new Vector2(0f, chatPanel.rect.height);
                    break;
                case ChatPanelTransitionType.SlideFromBottom:
                    chatPanelCG.alpha          = 0f;
                    chatPanel.anchoredPosition = chatPanelRestPos - new Vector2(0f, chatPanel.rect.height);
                    break;
                default: // Fade
                    chatPanelCG.alpha          = 0f;
                    chatPanel.anchoredPosition = chatPanelRestPos;
                    break;
            }
        }
    }

    

    private void ResetSlotScale(int index)
    {
        if (index >= 0 && index < panelImages.Count && panelImages[index] != null)
            panelImages[index].transform.localScale = Vector3.one;
    }
}