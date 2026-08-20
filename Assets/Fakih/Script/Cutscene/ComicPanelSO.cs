using UnityEngine;

[CreateAssetMenu(fileName = "New Comic Panel", menuName = "Cutscene/Comic Panel")]
public class ComicPanelSO : ScriptableObject
{
    [Header("Visual")]
    public Sprite panelImage;
    public Sprite bubbleSprite;          // optional speech bubble overlay

    [Header("Text")]
    [TextArea(2, 5)] public string dialogueText;

    [Header("Timing")]
    public float displayDuration = 2.5f;
    public bool waitForInput = true;

    [Header("Transition In")]
    public PanelTransitionType transitionIn = PanelTransitionType.SlideFromRight;
    public float transitionDuration = 0.4f;

    [Header("Audio")]
    public AudioClip sfxOnEnter;
    public AudioClip voiceOver;

    [Header("Comic Effects")]
    public bool useKenBurnsEffect = true;
    public float zoomAmount = 1.15f;
}

public enum PanelTransitionType
{
    None, Fade, SlideFromRight, SlideFromLeft,
    SlideFromTop, SlideFromBottom, PanelWipe, ComicPop
}
