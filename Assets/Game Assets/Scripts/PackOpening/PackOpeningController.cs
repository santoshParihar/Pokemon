#if PRIME_TWEEN_INSTALLED
using PrimeTween;
#endif
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Thin orchestrator for the booster-pack opening ceremony.
///
/// All SerializeField references are kept here so SceneSetupHelper wiring
/// remains unchanged. Domain logic is delegated to sub-components:
///   PackDrawer          — weighted random card draw  (static utility)
///   PackStoreUI         — store panel, cooldown, idle animations
///   PackRevealAnimator  — all 3D card animation phases
///   PackSummaryBuilder  — summary panel row spawning
///
/// Run "Pokemon TCG / Setup Main Scene UI" to build the UI hierarchy and
/// wire all Inspector references automatically.
/// </summary>
public class PackOpeningController : MonoBehaviour
{
    // ────────────────────────────────────────────────────────────────────────
    // Inspector fields  (all set by SceneSetupHelper — do NOT rename)
    // ────────────────────────────────────────────────────────────────────────

    [Header("Master Card Pool (same assets as MainUIManager)")]
    [SerializeField] private List<PokemonCardData> masterCardPool = new List<PokemonCardData>();

    [Header("Store Panel UI")]
    [SerializeField] private GameObject       storeRootPanel;
    [SerializeField] private Image            packArtImage;
    [SerializeField] private TextMeshProUGUI  packNameLabel;
    [SerializeField] private Button           openPackButton;
    [SerializeField] private TextMeshProUGUI  openPackButtonText;
    [SerializeField] private TextMeshProUGUI  cooldownTimerLabel;
    [SerializeField] private Image            packShimmerOverlay;

    [Header("Pack Opening Overlay")]
    [SerializeField] private GameObject  packOverlayPanel;
    [SerializeField] private CanvasGroup packOverlayCG;

    [Header("Pack Rip Animation")]
    [SerializeField] private RectTransform packRipRect;
    [SerializeField] private CanvasGroup   packRipCG;

    [Header("Card Reveal Area")]
    [SerializeField] private RectTransform cardRevealContainer;
    [SerializeField] private GameObject    card3DRevealPrefab;
    [SerializeField] private int           cardsPerPack   = 3;
    [SerializeField] private float         revealSpacingX = 1.5f;
    [SerializeField] private float         revealWorldY   = 0.3f;
    [SerializeField] private float         revealWorldZ   = -2.0f;

    [Header("Screen Flash Image")]
    [SerializeField] private Image glowBurstImage;

    [Header("Reveal Summary")]
    [SerializeField] private GameObject      summaryPanel;
    [SerializeField] private Button          addToCollectionButton;
    [SerializeField] private TextMeshProUGUI summaryLabel;
    [SerializeField] private Transform       summaryCardContainer;

    [Header("Rarity Colors")]
    [SerializeField] private Color colorCommon    = new Color(0.95f, 0.95f, 0.95f);
    [SerializeField] private Color colorUncommon  = new Color(0.55f, 0.9f,  0.55f);
    [SerializeField] private Color colorRare      = new Color(1f,    0.82f, 0.18f);
    [SerializeField] private Color colorEpic      = new Color(0.65f, 0.35f, 0.95f);
    [SerializeField] private Color colorLegendary = new Color(1f,    0.45f, 0.1f);

    [Header("Idle Ready Animation Settings")]
    [SerializeField] private float idleShakeRotation      = 3.0f;
    [SerializeField] private float idleShakeSpeed         = 0.12f;
    [SerializeField] private float idleScalePulse         = 1.04f;
    [SerializeField] private float idleDelayBetweenCycles = 0.0f;

    [Header("Callback")]
    [SerializeField] private MainUIManager mainUIManager;

    // ────────────────────────────────────────────────────────────────────────
    // Sub-components (created programmatically — not serialised)
    // ────────────────────────────────────────────────────────────────────────

    private PackStoreUI        storeUI;
    private PackRevealAnimator revealAnimator;
    private PackSummaryBuilder summaryBuilder;

    // ────────────────────────────────────────────────────────────────────────
    // Private state
    // ────────────────────────────────────────────────────────────────────────

    private List<PokemonCardData> drawnCards    = new List<PokemonCardData>();
    private bool                  packIsOpening = false;

    // ────────────────────────────────────────────────────────────────────────
    // Lifecycle
    // ────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Create sub-components on the same GameObject, then inject refs
        storeUI = gameObject.AddComponent<PackStoreUI>();
        storeUI.Initialize(
            packArtImage, packNameLabel,
            openPackButton, openPackButtonText,
            cooldownTimerLabel, packShimmerOverlay,
            addToCollectionButton,
            idleShakeRotation, idleShakeSpeed, idleScalePulse, idleDelayBetweenCycles,
            OnOpenPackClicked, OnAddToCollectionClicked);

        revealAnimator = gameObject.AddComponent<PackRevealAnimator>();
        revealAnimator.Initialize(
            packOverlayPanel, packOverlayCG,
            packRipRect, packRipCG,
            card3DRevealPrefab, revealWorldY, revealWorldZ,
            glowBurstImage,
            colorCommon, colorUncommon, colorRare, colorEpic, colorLegendary);

        summaryBuilder = gameObject.AddComponent<PackSummaryBuilder>();
        summaryBuilder.Initialize(
            summaryPanel, addToCollectionButton,
            summaryLabel, summaryCardContainer,
            colorCommon, colorUncommon, colorRare, colorEpic, colorLegendary);

        HideOverlayInstant();
        storeUI.SetupButtons();
    }

    private void Start()    => storeUI.SetupButtons();
    private void OnEnable() => storeUI.RefreshStorePanel();
    private void OnDisable() => storeUI.StopAll();

    // ────────────────────────────────────────────────────────────────────────
    // Pack opening entry point
    // ────────────────────────────────────────────────────────────────────────

    private void OnOpenPackClicked()
    {
        if (packIsOpening || !PlayerCollection.CanOpenFreePack()) return;
        storeUI.SetPackOpeningState(true);
        StartCoroutine(PackOpeningSequence());
    }

    private IEnumerator PackOpeningSequence()
    {
        packIsOpening = true;
        storeUI.SetPackOpeningState(true);
        storeUI.SetPackButtonInteractable(false);
        PlayerCollection.RecordPackOpened();

        // Draw cards using the static utility
        drawnCards = PackDrawer.Draw(masterCardPool, cardsPerPack);

        // Run phases via sub-components (coordinator starts the coroutines)
        yield return StartCoroutine(revealAnimator.Phase_OverlayFadeIn(summaryPanel));
        yield return StartCoroutine(revealAnimator.Phase_PackEntrance());
        yield return StartCoroutine(revealAnimator.Phase_PackRip());
        yield return StartCoroutine(revealAnimator.Phase_SpawnFaceDownCards(drawnCards));

        yield return new WaitForSeconds(0.4f);
        summaryBuilder.ShowSummaryPanel(drawnCards);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Add to Collection
    // ────────────────────────────────────────────────────────────────────────

    private void OnAddToCollectionClicked()
    {
        PlayerCollection.AddCards(drawnCards);
        StartCoroutine(CardsExplodeOut());
    }

    private IEnumerator CardsExplodeOut()
    {
        if (addToCollectionButton != null) addToCollectionButton.interactable = false;

        // Shrink all spawned cards
        yield return StartCoroutine(revealAnimator.ShrinkAndFadeCards());

        // Fade out overlay
        #if PRIME_TWEEN_INSTALLED
        if (packOverlayCG != null)
            yield return Tween.Alpha(packOverlayCG, 0f, 0.3f, Ease.OutCubic).ToYieldInstruction();
        #endif

        HideOverlayInstant();
        revealAnimator.Cleanup();
        drawnCards.Clear();
        packIsOpening = false;
        storeUI.SetPackOpeningState(false);

        if (mainUIManager != null)
            mainUIManager.SwitchToTab(MainUIManager.Tab.Collection);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    private void HideOverlayInstant()
    {
        if (packOverlayPanel != null) packOverlayPanel.SetActive(false);
        if (packOverlayCG != null)    packOverlayCG.alpha = 0f;
        if (summaryPanel != null)     summaryPanel.SetActive(false);
        if (packRipRect != null)      packRipRect.gameObject.SetActive(false);
        if (glowBurstImage != null)   glowBurstImage.gameObject.SetActive(false);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Debug context menu
    // ────────────────────────────────────────────────────────────────────────

    [ContextMenu("DEBUG — Clear Player Collection & Cooldown")]
    public void DEBUG_ClearData()
    {
        PlayerCollection.ClearAll();
        Debug.Log("[PackOpening] Player data cleared.");
    }

    [ContextMenu("DEBUG — Skip Cooldown")]
    public void DEBUG_SkipCooldown()
    {
        PlayerPrefs.SetString("LastPackOpenedTicks", "0");
        PlayerPrefs.Save();
        if (storeUI != null) storeUI.RefreshStorePanel();
        Debug.Log("[PackOpening] Cooldown bypassed.");
    }
}
