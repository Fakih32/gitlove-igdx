using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComicCutscenePlayer : MonoBehaviour
{
    [SerializeField] private CutsceneSequenceSO sequence;
    [SerializeField] private Image panelImage;
    [SerializeField] private Image bubbleImage;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private RectTransform panelRect;

    public event System.Action OnCutsceneComplete;
    private bool advanceRequested;

    public void Play()
    {
        gameObject.SetActive(true);
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        foreach (var panel in sequence.panels)
            yield return StartCoroutine(PlayPanel(panel));

        OnCutsceneComplete?.Invoke();
        gameObject.SetActive(false);
    }

    private IEnumerator PlayPanel(ComicPanelSO panel)
    {
        panelImage.sprite = panel.panelImage;
        dialogueText.text = panel.dialogueText;
        bubbleImage.gameObject.SetActive(panel.bubbleSprite != null);
        if (panel.bubbleSprite) bubbleImage.sprite = panel.bubbleSprite;

        if (panel.sfxOnEnter) sfxSource.PlayOneShot(panel.sfxOnEnter);
        if (panel.voiceOver) voiceSource.PlayOneShot(panel.voiceOver);

        yield return StartCoroutine(PlayTransition(panel));
        if (panel.useKenBurnsEffect) StartCoroutine(KenBurns(panel));

        advanceRequested = false;
        float t = 0f;
        while (t < panel.displayDuration || (panel.waitForInput && !advanceRequested))
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                advanceRequested = true;
            if (panel.waitForInput && advanceRequested) break;
            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator PlayTransition(ComicPanelSO panel)
    {
        float dur = panel.transitionDuration, t = 0f;
        Vector2 startPos = panel.transitionIn switch
        {
            PanelTransitionType.SlideFromRight => new Vector2(Screen.width, 0),
            PanelTransitionType.SlideFromLeft => new Vector2(-Screen.width, 0),
            PanelTransitionType.SlideFromTop => new Vector2(0, Screen.height),
            PanelTransitionType.SlideFromBottom => new Vector2(0, -Screen.height),
            _ => Vector2.zero
        };
        CanvasGroup cg = panelImage.GetComponent<CanvasGroup>();

        while (t < dur)
        {
            float p = t / dur;
            if (panel.transitionIn == PanelTransitionType.Fade && cg)
                cg.alpha = Mathf.Lerp(0, 1, p);
            else
                panelRect.anchoredPosition = Vector2.Lerp(startPos, Vector2.zero, p);
            t += Time.deltaTime;
            yield return null;
        }
        panelRect.anchoredPosition = Vector2.zero;
        if (cg) cg.alpha = 1;
    }

    private IEnumerator KenBurns(ComicPanelSO panel)
    {
        float t = 0f;
        while (t < panel.displayDuration)
        {
            panelRect.localScale = Vector3.one * Mathf.Lerp(1f, panel.zoomAmount, t / panel.displayDuration);
            t += Time.deltaTime;
            yield return null;
        }
    }
}