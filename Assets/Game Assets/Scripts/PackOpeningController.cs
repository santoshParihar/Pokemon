#if PRIME_TWEEN_INSTALLED
using PrimeTween;
#endif
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Drives the booster-pack opening ceremony using PrimeTween.
///
/// Run "Pokemon TCG / Setup Pack Opening UI" from the menu bar to build the
/// full hierarchy automatically and wire all Inspector references.
///
/// Flow:
///   Store tab → "Open Pack" button → pack rip animation
///   → 3 face-down cards slide in → tap each to flip → "Add to Collection"
///   → cards explode out → Collection tab opens
/// </summary>
public class PackOpeningController : MonoBehaviour
{
    // ────────────────────────────────────────────────────────────────────────
    // Inspector fields  (all set automatically by PackOpeningSetupHelper)
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
    [SerializeField] private Image            packShimmerOverlay;   // Semi-transparent white overlay on pack

    [Header("Pack Opening Overlay")]
    [SerializeField] private GameObject       packOverlayPanel;     // Full-screen dark canvas
    [SerializeField] private CanvasGroup      packOverlayCG;

    [Header("Pack Rip Animation")]
    [SerializeField] private RectTransform    packRipRect;
    [SerializeField] private CanvasGroup      packRipCG;

    [Header("Card Reveal Area")]
    [SerializeField] private RectTransform    cardRevealContainer;   // UI cells for tap input
    [SerializeField] private GameObject       card3DRevealPrefab;    // CharizardCard prefab
    [SerializeField] private int              cardsPerPack = 3;
    [SerializeField] private float            revealSpacingX = 1.5f;
    [SerializeField] private float            revealWorldY   = 0.3f;
    [SerializeField] private float            revealWorldZ   = -2.0f;

    [Header("Screen Flash Image")]
    [SerializeField] private Image            glowBurstImage;       // Fullscreen, alpha normally 0

    [Header("Reveal Summary")]
    [SerializeField] private GameObject       summaryPanel;
    [SerializeField] private Button           addToCollectionButton;
    [SerializeField] private TextMeshProUGUI  summaryLabel;

    [Header("Rarity Colors")]
    [SerializeField] private Color colorCommon    = new Color(0.95f, 0.95f, 0.95f);
    [SerializeField] private Color colorUncommon  = new Color(0.55f, 0.9f,  0.55f);
    [SerializeField] private Color colorRare      = new Color(1f,    0.82f, 0.18f);
    [SerializeField] private Color colorEpic      = new Color(0.65f, 0.35f, 0.95f);
    [SerializeField] private Color colorLegendary = new Color(1f,    0.45f, 0.1f);

    [Header("Idle Ready Animation Settings")]
    [SerializeField] private float            idleShakeRotation = 3.0f;      // Rotation angle (Z degrees) for wiggle
    [SerializeField] private float            idleShakeSpeed = 0.12f;        // Time in seconds for each wiggle step
    [SerializeField] private float            idleScalePulse = 1.04f;        // Target scale for the pulse
    [SerializeField] private float            idleDelayBetweenCycles = 0.0f;  // Delay before repeating the shake (0 for continuous loop)

    [Header("Callback")]
    [SerializeField] private MainUIManager mainUIManager;

    // ────────────────────────────────────────────────────────────────────────
    // Private state
    // ────────────────────────────────────────────────────────────────────────

    private List<PokemonCardData> drawnCards          = new List<PokemonCardData>();
    private List<GameObject>      spawnedRevealCards   = new List<GameObject>(); // UI tap cells
    private List<GameObject>      spawned3DCards       = new List<GameObject>(); // world-space 3D cards
    private List<bool>            cardFlipped          = new List<bool>();
    private int                   flippedCount         = 0;
    private bool                  packIsOpening        = false;
    private bool[]                cardRevealComplete;  // true after each card's flip+fly-away is done

    #if PRIME_TWEEN_INSTALLED
    private Sequence shimmerSequence;
    private Tween cooldownTween;   // not a tween — we just track with a coroutine below
    private Sequence idlePackSequence;
    #endif

    private Coroutine cooldownCoroutine;

    // ────────────────────────────────────────────────────────────────────────
    // Lifecycle
    // ────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        HideOverlayInstant();
        SetupButtons();
    }

    private void Start() => SetupButtons();

    private void OnEnable() => RefreshStorePanel();

    private void OnDisable()
    {
        #if PRIME_TWEEN_INSTALLED
        shimmerSequence.Stop();
        idlePackSequence.Stop();
        #endif
        if (cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Store Panel
    // ────────────────────────────────────────────────────────────────────────

    private void SetupButtons()
    {
        if (openPackButton != null)
        {
            openPackButton.onClick.RemoveAllListeners();
            openPackButton.onClick.AddListener(OnOpenPackClicked);
        }
        if (addToCollectionButton != null)
        {
            addToCollectionButton.onClick.RemoveAllListeners();
            addToCollectionButton.onClick.AddListener(OnAddToCollectionClicked);
        }
        if (packNameLabel != null) packNameLabel.text = "Kanto Starter Pack";
    }

    public void RefreshStorePanel()
    {
        #if PRIME_TWEEN_INSTALLED
        shimmerSequence.Stop();
        if (packShimmerOverlay != null)
        {
            packShimmerOverlay.rectTransform.anchoredPosition = new Vector2(-800f, 0f);
        }
        #endif

        // ── Cooldown label (uses a coroutine — PrimeTween doesn't drive UI text) ──
        if (cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(CooldownLabelRoutine());
    }

    private IEnumerator CooldownLabelRoutine()
    {
        while (true)
        {
            bool ready = PlayerCollection.CanOpenFreePack();
            if (openPackButton != null) openPackButton.interactable = ready && !packIsOpening;
            if (openPackButtonText != null)
                openPackButtonText.text = ready ? "✨  Open Free Pack" : "⏳  Next Pack In...";
            if (cooldownTimerLabel != null)
            {
                if (ready)
                    cooldownTimerLabel.text = "Pack ready!";
                else
                {
                    System.TimeSpan rem = PlayerCollection.CooldownRemaining();
                    cooldownTimerLabel.text = $"{rem.Hours:D2}:{rem.Minutes:D2}:{rem.Seconds:D2}";
                }
            }

            #if PRIME_TWEEN_INSTALLED
            if (ready && !packIsOpening)
            {
                // Start wiggle if not playing
                if (!idlePackSequence.isAlive && packArtImage != null)
                {
                    packArtImage.rectTransform.localScale = Vector3.one;
                    packArtImage.rectTransform.localRotation = Quaternion.identity;

                    var seq = Sequence.Create(cycles: -1);
                    if (idleDelayBetweenCycles > 0f)
                    {
                        seq.Chain(Tween.Delay(idleDelayBetweenCycles));
                    }
                    
                    seq.Group(Tween.Scale(packArtImage.rectTransform, endValue: new Vector3(idleScalePulse, idleScalePulse, 1f), duration: idleShakeSpeed * 1.5f, ease: Ease.OutQuad))
                       .Chain(Tween.Rotation(packArtImage.rectTransform, endValue: new Vector3(0, 0, -idleShakeRotation), duration: idleShakeSpeed, ease: Ease.InOutQuad))
                       .Chain(Tween.Rotation(packArtImage.rectTransform, endValue: new Vector3(0, 0, idleShakeRotation), duration: idleShakeSpeed * 1.5f, ease: Ease.InOutQuad))
                       .Chain(Tween.Rotation(packArtImage.rectTransform, endValue: new Vector3(0, 0, -idleShakeRotation * 0.7f), duration: idleShakeSpeed * 1.2f, ease: Ease.InOutQuad))
                       .Chain(Tween.Rotation(packArtImage.rectTransform, endValue: new Vector3(0, 0, 0f), duration: idleShakeSpeed, ease: Ease.OutQuad))
                       .Group(Tween.Scale(packArtImage.rectTransform, endValue: Vector3.one, duration: idleShakeSpeed * 1.5f, ease: Ease.InOutQuad));

                    idlePackSequence = seq;
                }

                // Start shine sweep if not playing
                if (!shimmerSequence.isAlive && packShimmerOverlay != null)
                {
                    packShimmerOverlay.rectTransform.anchoredPosition = new Vector2(-800f, 0f);

                    shimmerSequence = Sequence.Create(cycles: -1)
                        .Chain(Tween.UIAnchoredPositionX(packShimmerOverlay.rectTransform, endValue: 800f, duration: 1.6f, ease: Ease.InOutQuad))
                        .Chain(Tween.Delay(2.0f))
                        .Chain(Tween.UIAnchoredPositionX(packShimmerOverlay.rectTransform, endValue: -800f, duration: 0f));
                }
            }
            else
            {
                // Stop wiggle
                if (idlePackSequence.isAlive)
                {
                    idlePackSequence.Stop();
                    if (packArtImage != null)
                    {
                        packArtImage.rectTransform.localScale = Vector3.one;
                        packArtImage.rectTransform.localRotation = Quaternion.identity;
                    }
                }

                // Stop shine sweep
                if (shimmerSequence.isAlive)
                {
                    shimmerSequence.Stop();
                    if (packShimmerOverlay != null)
                    {
                        packShimmerOverlay.rectTransform.anchoredPosition = new Vector2(-800f, 0f);
                    }
                }
            }
            #endif

            yield return new WaitForSeconds(1f);
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Entry point
    // ────────────────────────────────────────────────────────────────────────

    private void OnOpenPackClicked()
    {
        if (packIsOpening || !PlayerCollection.CanOpenFreePack()) return;
        #if PRIME_TWEEN_INSTALLED
        if (idlePackSequence.isAlive)
        {
            idlePackSequence.Stop();
            if (packArtImage != null)
            {
                packArtImage.rectTransform.localScale = Vector3.one;
                packArtImage.rectTransform.localRotation = Quaternion.identity;
            }
        }
        if (shimmerSequence.isAlive)
        {
            shimmerSequence.Stop();
            if (packShimmerOverlay != null)
            {
                packShimmerOverlay.rectTransform.anchoredPosition = new Vector2(-800f, 0f);
            }
        }
        #endif
        StartCoroutine(PackOpeningSequence());
    }

    // ────────────────────────────────────────────────────────────────────────
    // Master sequence (coroutine — awaits PrimeTween via ToYieldInstruction)
    // ────────────────────────────────────────────────────────────────────────

    private IEnumerator PackOpeningSequence()
    {
        packIsOpening = true;
        if (openPackButton != null) openPackButton.interactable = false;
        PlayerCollection.RecordPackOpened();

        DrawCards();

        yield return StartCoroutine(Phase_OverlayFadeIn());
        yield return StartCoroutine(Phase_PackEntrance());
        yield return StartCoroutine(Phase_PackRip());
        // Phase_SpawnFaceDownCards now drives the entire one-at-a-time reveal sequence
        // and returns only after the last card is flipped & flown away.
        yield return StartCoroutine(Phase_SpawnFaceDownCards());

        yield return new WaitForSeconds(0.4f);
        ShowSummaryPanel();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 1 — Draw random cards
    // ────────────────────────────────────────────────────────────────────────

    private void DrawCards()
    {
        drawnCards.Clear();
        if (masterCardPool == null || masterCardPool.Count == 0)
        {
            Debug.LogWarning("[PackOpening] masterCardPool is empty!");
            return;
        }

        // Weighted pool: rarityStars 1→weight 8, 2→4, 3→2, 4→1, 5→1
        int[] weights = { 8, 4, 2, 1, 1 };
        List<PokemonCardData> weightedPool = new List<PokemonCardData>();
        foreach (var card in masterCardPool)
        {
            if (card == null) continue;
            int w = weights[Mathf.Clamp(card.rarityStars, 1, 5) - 1];
            for (int i = 0; i < w; i++) weightedPool.Add(card);
        }

        // Guarantee the highest rarity card in the last slot if it's rare+
        PokemonCardData guaranteed = null;
        int maxRarity = 0;
        foreach (var c in masterCardPool)
            if (c != null && c.rarityStars > maxRarity) { maxRarity = c.rarityStars; guaranteed = c; }

        for (int i = 0; i < cardsPerPack; i++)
        {
            bool isLastSlot = i == cardsPerPack - 1;
            PokemonCardData pick = isLastSlot && guaranteed != null && maxRarity >= 3
                ? guaranteed
                : weightedPool[Random.Range(0, weightedPool.Count)];
            drawnCards.Add(pick);
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 2 — Overlay fade-in
    // ────────────────────────────────────────────────────────────────────────

    private IEnumerator Phase_OverlayFadeIn()
    {
        if (packOverlayPanel != null) packOverlayPanel.SetActive(true);
        if (summaryPanel != null) summaryPanel.SetActive(false);

        if (packOverlayCG != null)
        {
            packOverlayCG.alpha = 0f;
            packOverlayCG.interactable = packOverlayCG.blocksRaycasts = true;
            #if PRIME_TWEEN_INSTALLED
            yield return Tween.Alpha(packOverlayCG, 1f, 0.4f, Ease.OutCubic)
                              .ToYieldInstruction();
            #else
            yield return new WaitForSeconds(0.4f);
            #endif
        }
        else yield return null;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 3 — Pack entrance + anticipation shake
    // ────────────────────────────────────────────────────────────────────────

    private IEnumerator Phase_PackEntrance()
    {
        if (packRipRect == null) yield break;

        packRipRect.gameObject.SetActive(true);
        if (packRipCG != null) packRipCG.alpha = 1f;
        packRipRect.localScale = Vector3.zero;

        #if PRIME_TWEEN_INSTALLED
        // Bounce in with OutBack
        yield return Tween.Scale(packRipRect, Vector3.one, 0.5f, Ease.OutBack)
                          .ToYieldInstruction();

        yield return new WaitForSeconds(0.3f);

        // Anticipation shake (ShakeLocalPosition)
        yield return Tween.ShakeLocalPosition(packRipRect, strength: new Vector3(14f, 8f, 0), duration: 0.55f, frequency: 18)
                          .ToYieldInstruction();
        #else
        packRipRect.localScale = Vector3.one;
        yield return new WaitForSeconds(0.85f);
        #endif
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 4 — Pack rip + screen flash
    // ────────────────────────────────────────────────────────────────────────

    private IEnumerator Phase_PackRip()
    {
        if (packRipRect == null) yield break;

        #if PRIME_TWEEN_INSTALLED
        // Burst scale up and fade out simultaneously (preventing paper-thin look)
        var scaleSeq = Sequence.Create()
            .Group(Tween.Scale(packRipRect, endValue: new Vector3(1.4f, 1.4f, 1f), duration: 0.45f, ease: Ease.OutQuad));

        if (packRipCG != null)
            scaleSeq.Group(Tween.Alpha(packRipCG, 0f, 0.4f, Ease.OutQuad));

        yield return scaleSeq.ToYieldInstruction();
        #else
        yield return new WaitForSeconds(0.45f);
        #endif

        packRipRect.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.1f); // transition instantly to first card spawn
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 5 — Spawn & auto-reveal cards one at a time (portrait-safe, no tap needed)
    // ────────────────────────────────────────────────────────────────────────

    private IEnumerator Phase_SpawnFaceDownCards()
    {
        foreach (var old in spawned3DCards) if (old) Destroy(old);
        spawned3DCards.Clear();
        cardFlipped.Clear();
        flippedCount = 0;

        if (card3DRevealPrefab == null) { Debug.LogWarning("[PackOpening] card3DRevealPrefab not assigned!"); yield break; }

        int count = drawnCards.Count;
        for (int i = 0; i < count; i++) cardFlipped.Add(false);

        // Each card is fully self-contained: spawn → back-face entrance → auto-flip front → fly away
        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(RevealCardRoutine(i));
            yield return new WaitForSeconds(0.6f); // breath between cards (was 0.2s)
        }
    }

    /// <summary>
    /// Full single-card reveal: pop in back-face → pause → auto-flip to front → spin → fly away.
    /// No tap/button required. Works with or without PrimeTween.
    /// </summary>
    private IEnumerator RevealCardRoutine(int idx)
    {
        Vector3 finalPos = new Vector3(0f, revealWorldY, revealWorldZ);

        // ── Spawn card back-face (Y=180), zero scale, dramatic Z tilt ───────
        GameObject card3D = Instantiate(card3DRevealPrefab, finalPos, Quaternion.Euler(0f, 180f, -22f));
        card3D.transform.localScale = Vector3.zero;

        CardRotator rot = card3D.GetComponent<CardRotator>();
        if (rot != null) rot.enabled = false;

        while (spawned3DCards.Count <= idx) spawned3DCards.Add(null);
        spawned3DCards[idx] = card3D;

        // ── STAGE 1: Card Spawns Face-Down ─────────────────────────────────
        // The card scales up from 0 to 1 with an overshoot bounce (OutBack) 
        // while rotating from a -22 degree tilt to a +4 degree Z-tilt.
        #if PRIME_TWEEN_INSTALLED
        StartCoroutine(FlashScreen(new Color(0.70f, 0.82f, 1f), 0.28f)); // blue-white flash
        yield return Sequence.Create()
            .Group(Tween.Scale(card3D.transform,
                               endValue: Vector3.one, duration: 0.52f, ease: Ease.OutBack))
            .Group(Tween.LocalRotation(card3D.transform,
                                       startValue: Quaternion.Euler(0f, 180f, -22f),
                                       endValue:   Quaternion.Euler(0f, 180f,   4f),
                                       duration: 0.52f, ease: Ease.OutBack))
            .ToYieldInstruction();

        // ── STAGE 2: Card Z-Rotation Settles ────────────────────────────────
        // Tilts the card back upright from its Z-tilt to align perfectly straight.
        yield return Tween.LocalRotation(card3D.transform,
            startValue: Quaternion.Euler(0f, 180f, 4f),
            endValue:   Quaternion.Euler(0f, 180f, 0f),
            duration: 0.13f, ease: Ease.OutQuad).ToYieldInstruction();

        // ── STAGE 3: Card Landing Settle ───────────────────────────────────
        // Simulates a satisfying "landing thud" by briefly squishing and bouncing back.
        yield return Tween.Scale(card3D.transform, new Vector3(1.09f, 0.93f, 1f), 0.08f).ToYieldInstruction();
        yield return Tween.Scale(card3D.transform, Vector3.one, 0.08f, Ease.OutQuad).ToYieldInstruction();

        // ── STAGE 4: Anticipation Pause ────────────────────────────────────
        // Pauses face-down briefly to build anticipation before the flip.
        yield return new WaitForSeconds(0.75f); // was 0.38f

        // ── STAGE 5: Snappy Wiggle & Flip ──────────────────────────────────
        // The card wiggles side-to-side on Z, pulses scale, scales X down to 0,
        // swaps rotation to front-face, and scales X back up.
        yield return Sequence.Create()
            .Group(Tween.Scale(card3D.transform, endValue: new Vector3(1.04f, 1.04f, 1f), duration: 0.10f, ease: Ease.OutQuad))
            .Chain(Tween.LocalRotation(card3D.transform, endValue: Quaternion.Euler(0f, 180f, -3f), duration: 0.05f, ease: Ease.InOutQuad))
            .Chain(Tween.LocalRotation(card3D.transform, endValue: Quaternion.Euler(0f, 180f, 3f), duration: 0.08f, ease: Ease.InOutQuad))
            .Chain(Tween.LocalRotation(card3D.transform, endValue: Quaternion.Euler(0f, 180f, 0f), duration: 0.05f, ease: Ease.OutQuad))
            .Group(Tween.Scale(card3D.transform, endValue: Vector3.one, duration: 0.08f, ease: Ease.InOutQuad))
            .ToYieldInstruction();

        // Scale X from 1 to 0 (half flip)
        yield return Tween.ScaleX(card3D.transform, endValue: 0f, duration: 0.32f, ease: Ease.InQuad)
            .ToYieldInstruction();

        // Instantly set rotation to front face (0 degrees)
        card3D.transform.localRotation = Quaternion.identity;

        // Scale X back from 0 to 1 (complete flip)
        yield return Tween.ScaleX(card3D.transform, endValue: 1f, duration: 0.32f, ease: Ease.OutQuad)
            .ToYieldInstruction();

        // ── STAGE 6: Reveal Pop ────────────────────────────────────────────
        // Performs a brief scale overshoot pop to draw attention to the front face.
        yield return Tween.Scale(card3D.transform, Vector3.one * 1.18f, 0.15f, Ease.OutBack).ToYieldInstruction();
        yield return Tween.Scale(card3D.transform, Vector3.one,          0.12f, Ease.OutQuad).ToYieldInstruction();

        #else
        // ── Fallback: manual lerp animations (no PrimeTween) ─────────────────
        yield return StartCoroutine(LerpScale(card3D.transform, Vector3.zero, Vector3.one, 0.5f));
        card3D.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        yield return new WaitForSeconds(0.4f);
        yield return StartCoroutine(LerpRotation(card3D.transform,
            Quaternion.Euler(0f, 180f, 0f), Quaternion.identity, 0.4f));
        #endif

        // ── STAGE 7: Front Face Showing & Float Loop ───────────────────────
        // Starts the endless floating loop (moving up/down on Y, wiggling on Z).
        cardFlipped[idx] = true;

        if (rot == null) rot = card3D.AddComponent<CardRotator>();
        rot.autoSpin = false; // Disable Y-spin which makes 2D sprites look paper-thin
        rot.enabled = true;   // Still allow manual drag inspection
        flippedCount++;

        // Gentle floating hover & tilt loop
        #if PRIME_TWEEN_INSTALLED
        Tween.LocalPosition(card3D.transform, startValue: finalPos, endValue: finalPos + new Vector3(0f, 0.12f, 0f), duration: 1.0f, ease: Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
        Tween.LocalRotation(card3D.transform, startValue: Quaternion.identity, endValue: Quaternion.Euler(0f, 0f, 2.5f), duration: 1.0f, ease: Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
        #endif

        // ── STAGE 8: Admire Pause ──────────────────────────────────────────
        // Keeps the card front-face up on screen so the player can view the details.
        yield return new WaitForSeconds(1.6f); // was 0.90f

        // ── STAGE 9: Fly card up & shrink away ──────────────────────────────
        rot.enabled = false;
        #if PRIME_TWEEN_INSTALLED
        Tween.StopAll(card3D.transform); // Stop the hover loop before flying away
        Tween.Scale(card3D.transform, Vector3.zero, 0.36f, Ease.InCubic);
        yield return Tween.Position(card3D.transform,
            card3D.transform.position + new Vector3(0f, 5f, 0f),
            0.36f, Ease.InCubic).ToYieldInstruction();
        #else
        yield return StartCoroutine(LerpScale(card3D.transform, Vector3.one, Vector3.zero, 0.36f));
        #endif
    }

    // ── Simple lerp helpers used in #else fallback ────────────────────────────

    private IEnumerator LerpScale(Transform t, Vector3 from, Vector3 to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            if (t == null) yield break;
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(from, to, elapsed / dur);
            yield return null;
        }
        if (t != null) t.localScale = to;
    }

    private IEnumerator LerpRotation(Transform t, Quaternion from, Quaternion to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            if (t == null) yield break;
            elapsed += Time.deltaTime;
            t.localRotation = Quaternion.Slerp(from, to, elapsed / dur);
            yield return null;
        }
        if (t != null) t.localRotation = to;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Rarity burst VFX
    // ────────────────────────────────────────────────────────────────────────

    private IEnumerator RarityBurst(int stars)
    {
        Color c = RarityColor(stars);
        int flashes = stars >= 4 ? 3 : stars >= 3 ? 2 : 1;
        for (int f = 0; f < flashes; f++)
        {
            yield return StartCoroutine(FlashScreen(c, 0.2f));
            yield return new WaitForSeconds(0.04f);
        }
        if (stars >= 5)
            yield return StartCoroutine(FlashScreen(colorLegendary, 0.55f));
    }

    private IEnumerator FlashScreen(Color color, float duration)
    {
        if (glowBurstImage == null) yield break;
        glowBurstImage.color = new Color(color.r, color.g, color.b, 0f);
        glowBurstImage.gameObject.SetActive(true);

        #if PRIME_TWEEN_INSTALLED
        yield return Tween.Alpha(glowBurstImage, 0.55f, duration * 0.3f, Ease.OutCubic).ToYieldInstruction();
        yield return Tween.Alpha(glowBurstImage, 0f,    duration * 0.7f, Ease.InCubic).ToYieldInstruction();
        #else
        yield return new WaitForSeconds(duration);
        #endif

        glowBurstImage.gameObject.SetActive(false);
    }

    private Color RarityColor(int stars) => stars switch
    {
        1 => colorCommon,
        2 => colorUncommon,
        3 => colorRare,
        4 => colorEpic,
        _ => colorLegendary
    };

    // ────────────────────────────────────────────────────────────────────────
    // Phase 7 — Summary panel
    // ────────────────────────────────────────────────────────────────────────

    private void ShowSummaryPanel()
    {
        if (summaryPanel == null) return;

        if (summaryLabel != null)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>Cards Received!</b>\n");
            foreach (var card in drawnCards)
                if (card != null)
                    sb.AppendLine($"• {card.pokemonName}  <color=#888888>({RarityName(card.rarityStars)})</color>");
            summaryLabel.text = sb.ToString();
        }

        summaryPanel.SetActive(true);

        #if PRIME_TWEEN_INSTALLED
        summaryPanel.transform.localScale = Vector3.zero;
        Tween.Scale(summaryPanel.transform, Vector3.one, 0.45f, Ease.OutBack);
        #endif
    }

    private string RarityName(int stars) => stars switch
    {
        1 => "Common", 2 => "Uncommon", 3 => "Rare", 4 => "Epic", _ => "Legendary"
    };

    // ────────────────────────────────────────────────────────────────────────
    // "Add to Collection"
    // ────────────────────────────────────────────────────────────────────────

    private void OnAddToCollectionClicked()
    {
        PlayerCollection.AddCards(drawnCards);
        StartCoroutine(CardsExplodeOut());
    }

    private IEnumerator CardsExplodeOut()
    {
        if (addToCollectionButton != null) addToCollectionButton.interactable = false;

        Vector2[] corners = {
            new Vector2(-650f,  750f),
            new Vector2( 650f,  750f),
            new Vector2(-650f, -750f),
        };

        #if PRIME_TWEEN_INSTALLED
        for (int i = 0; i < spawned3DCards.Count; i++)
        {
            GameObject c = spawned3DCards[i];
            if (c == null) continue;
            // Disable rotator so tween has control
            CardRotator rot = c.GetComponent<CardRotator>();
            if (rot != null) rot.enabled = false;
            Tween.Scale(c.transform, Vector3.zero, 0.4f, Ease.InCubic);
        }
        yield return new WaitForSeconds(0.45f);

        // Fade out overlay
        if (packOverlayCG != null)
            yield return Tween.Alpha(packOverlayCG, 0f, 0.3f, Ease.OutCubic).ToYieldInstruction();
        #else
        yield return new WaitForSeconds(0.5f);
        #endif

        HideOverlayInstant();
        CleanupRevealCards();
        packIsOpening = false;

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

    private void CleanupRevealCards()
    {
        foreach (var go in spawnedRevealCards) if (go) Destroy(go);
        foreach (var go in spawned3DCards)     if (go) Destroy(go);
        spawnedRevealCards.Clear();
        spawned3DCards.Clear();
        cardFlipped.Clear();
        drawnCards.Clear();
        flippedCount = 0;
        if (packOverlayPanel != null) packOverlayPanel.SetActive(false);
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
        RefreshStorePanel();
        Debug.Log("[PackOpening] Cooldown bypassed.");
    }
}
