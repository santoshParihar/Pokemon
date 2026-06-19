#if PRIME_TWEEN_INSTALLED
using PrimeTween;
#endif
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles all 3D card animation phases during the pack opening ceremony:
/// overlay fade-in, pack entrance, pack rip, per-card reveal sequences,
/// and the card explosion/shrink at the end.
/// Initialised by PackOpeningController and lives on the same GameObject.
/// </summary>
public class PackRevealAnimator : MonoBehaviour
{
    // ── Injected refs ────────────────────────────────────────────────────────

    private GameObject    packOverlayPanel;
    private CanvasGroup   packOverlayCG;
    private RectTransform packRipRect;
    private CanvasGroup   packRipCG;
    private GameObject    card3DRevealPrefab;
    private float         revealWorldY;
    private float         revealWorldZ;
    private Image         glowBurstImage;

    // ── Rarity colours ───────────────────────────────────────────────────────

    private Color colorCommon, colorUncommon, colorRare, colorEpic, colorLegendary;

    // ── Internal state ───────────────────────────────────────────────────────

    private readonly List<GameObject> spawned3DCards = new List<GameObject>();
    private readonly List<bool>       cardFlipped    = new List<bool>();

    /// <summary>Read-only view of spawned 3D cards (used by coordinator for CardsExplodeOut).</summary>
    public IReadOnlyList<GameObject> SpawnedCards => spawned3DCards;

    // ── Initialisation ───────────────────────────────────────────────────────

    /// <summary>Inject all refs. Called by PackOpeningController.Awake().</summary>
    public void Initialize(
        GameObject packOverlayPanel, CanvasGroup packOverlayCG,
        RectTransform packRipRect, CanvasGroup packRipCG,
        GameObject card3DRevealPrefab,
        float revealWorldY, float revealWorldZ,
        Image glowBurstImage,
        Color colorCommon, Color colorUncommon, Color colorRare,
        Color colorEpic, Color colorLegendary)
    {
        this.packOverlayPanel   = packOverlayPanel;
        this.packOverlayCG      = packOverlayCG;
        this.packRipRect        = packRipRect;
        this.packRipCG          = packRipCG;
        this.card3DRevealPrefab = card3DRevealPrefab;
        this.revealWorldY       = revealWorldY;
        this.revealWorldZ       = revealWorldZ;
        this.glowBurstImage     = glowBurstImage;
        this.colorCommon        = colorCommon;
        this.colorUncommon      = colorUncommon;
        this.colorRare          = colorRare;
        this.colorEpic          = colorEpic;
        this.colorLegendary     = colorLegendary;
    }

    // ── Public phase coroutines ──────────────────────────────────────────────

    /// <summary>Phase 1 — fade the dark overlay in and hide the summary panel.</summary>
    public IEnumerator Phase_OverlayFadeIn(GameObject summaryPanel)
    {
        if (packOverlayPanel != null) packOverlayPanel.SetActive(true);
        if (summaryPanel != null)     summaryPanel.SetActive(false);

        if (packOverlayCG != null)
        {
            packOverlayCG.alpha = 0f;
            packOverlayCG.interactable = packOverlayCG.blocksRaycasts = true;
            #if PRIME_TWEEN_INSTALLED
            yield return Tween.Alpha(packOverlayCG, 1f, 0.4f, Ease.OutCubic).ToYieldInstruction();
            #else
            yield return new WaitForSeconds(0.4f);
            #endif
        }
        else yield return null;
    }

    /// <summary>Phase 2 — pack bounces into frame and shakes.</summary>
    public IEnumerator Phase_PackEntrance()
    {
        if (packRipRect == null) yield break;

        packRipRect.gameObject.SetActive(true);
        if (packRipCG != null) packRipCG.alpha = 1f;
        packRipRect.localScale = Vector3.zero;

        #if PRIME_TWEEN_INSTALLED
        yield return Tween.Scale(packRipRect, Vector3.one, 0.5f, Ease.OutBack).ToYieldInstruction();
        yield return new WaitForSeconds(0.3f);
        yield return Tween.ShakeLocalPosition(packRipRect, new Vector3(14f, 8f, 0f), 0.55f, 18).ToYieldInstruction();
        #else
        packRipRect.localScale = Vector3.one;
        yield return new WaitForSeconds(0.85f);
        #endif
    }

    /// <summary>Phase 3 — pack bursts open and disappears.</summary>
    public IEnumerator Phase_PackRip()
    {
        if (packRipRect == null) yield break;

        #if PRIME_TWEEN_INSTALLED
        var seq = Sequence.Create()
            .Group(Tween.Scale(packRipRect, new Vector3(1.4f, 1.4f, 1f), 0.45f, Ease.OutQuad));
        if (packRipCG != null)
            seq.Group(Tween.Alpha(packRipCG, 0f, 0.4f, Ease.OutQuad));
        yield return seq.ToYieldInstruction();
        #else
        yield return new WaitForSeconds(0.45f);
        #endif

        packRipRect.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>Phase 4 — spawn and reveal each card one at a time.</summary>
    public IEnumerator Phase_SpawnFaceDownCards(List<PokemonCardData> drawnCards)
    {
        foreach (var old in spawned3DCards) if (old) Destroy(old);
        spawned3DCards.Clear();
        cardFlipped.Clear();

        if (card3DRevealPrefab == null)
        {
            Debug.LogWarning("[PackRevealAnimator] card3DRevealPrefab is not assigned!");
            yield break;
        }

        int count = drawnCards.Count;
        for (int i = 0; i < count; i++) cardFlipped.Add(false);

        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(RevealCardRoutine(i));
            yield return new WaitForSeconds(0.6f); // breath between cards
        }
    }

    /// <summary>Shrinks all spawned cards simultaneously, then waits for them to vanish.</summary>
    public IEnumerator ShrinkAndFadeCards()
    {
        #if PRIME_TWEEN_INSTALLED
        foreach (var c in spawned3DCards)
        {
            if (c == null) continue;
            var rot = c.GetComponent<CardRotator>();
            if (rot != null) rot.enabled = false;
            Tween.Scale(c.transform, Vector3.zero, 0.4f, Ease.InCubic);
        }
        yield return new WaitForSeconds(0.45f);
        #else
        yield return new WaitForSeconds(0.5f);
        #endif
    }

    /// <summary>Destroys all spawned 3D cards and clears internal state.</summary>
    public void Cleanup()
    {
        foreach (var go in spawned3DCards) if (go) Destroy(go);
        spawned3DCards.Clear();
        cardFlipped.Clear();
    }

    // ── Private animation stages ─────────────────────────────────────────────

    private IEnumerator RevealCardRoutine(int idx)
    {
        Vector3 finalPos = new Vector3(0f, revealWorldY, revealWorldZ);
        GameObject card3D = Instantiate(card3DRevealPrefab, finalPos, Quaternion.Euler(0f, 180f, -22f));
        card3D.transform.localScale = Vector3.zero;

        CardRotator rot = card3D.GetComponent<CardRotator>();
        if (rot != null) rot.enabled = false;

        while (spawned3DCards.Count <= idx) spawned3DCards.Add(null);
        spawned3DCards[idx] = card3D;

        #if PRIME_TWEEN_INSTALLED
        // STAGE 1: Card Spawns Face-Down — scale + tilt in with bounce
        StartCoroutine(FlashScreen(new Color(0.70f, 0.82f, 1f), 0.28f));
        yield return Sequence.Create()
            .Group(Tween.Scale(card3D.transform, Vector3.one, 0.52f, Ease.OutBack))
            .Group(Tween.LocalRotation(card3D.transform,
                       Quaternion.Euler(0f, 180f, -22f),
                       Quaternion.Euler(0f, 180f,   4f), 0.52f, Ease.OutBack))
            .ToYieldInstruction();

        // STAGE 2: Z-Rotation settles upright
        yield return Tween.LocalRotation(card3D.transform,
            Quaternion.Euler(0f, 180f, 4f),
            Quaternion.Euler(0f, 180f, 0f), 0.13f, Ease.OutQuad).ToYieldInstruction();

        // STAGE 3: Landing thud squish
        yield return Tween.Scale(card3D.transform, new Vector3(1.09f, 0.93f, 1f), 0.08f).ToYieldInstruction();
        yield return Tween.Scale(card3D.transform, Vector3.one, 0.08f, Ease.OutQuad).ToYieldInstruction();

        // STAGE 4: Anticipation pause
        yield return new WaitForSeconds(0.75f);

        // STAGE 5: Snappy wiggle then flip
        yield return Sequence.Create()
            .Group(Tween.Scale(card3D.transform, new Vector3(1.04f, 1.04f, 1f), 0.10f, Ease.OutQuad))
            .Chain(Tween.LocalRotation(card3D.transform, Quaternion.Euler(0f, 180f, -3f), 0.08f, Ease.InOutQuad))
            .Chain(Tween.LocalRotation(card3D.transform, Quaternion.Euler(0f, 180f,  0f), 0.08f, Ease.OutQuad))
            .Group(Tween.Scale(card3D.transform, Vector3.one, 0.08f, Ease.InOutQuad))
            .ToYieldInstruction();

        // Scale X → 0 (half-flip), swap rotation, scale X → 1 (complete flip)
        yield return Tween.ScaleX(card3D.transform, 0f, 0.32f, Ease.InQuad).ToYieldInstruction();
        card3D.transform.localRotation = Quaternion.identity;
        yield return Tween.ScaleX(card3D.transform, 1f, 0.32f, Ease.OutQuad).ToYieldInstruction();

        // STAGE 6: Reveal pop
        yield return Tween.Scale(card3D.transform, Vector3.one * 1.18f, 0.15f, Ease.OutBack).ToYieldInstruction();
        yield return Tween.Scale(card3D.transform, Vector3.one,          0.12f, Ease.OutQuad).ToYieldInstruction();
        #else
        // Fallback: manual lerp animations (no PrimeTween)
        yield return StartCoroutine(LerpScale(card3D.transform, Vector3.zero, Vector3.one, 0.5f));
        card3D.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        yield return new WaitForSeconds(0.4f);
        yield return StartCoroutine(LerpRotation(card3D.transform, Quaternion.Euler(0f, 180f, 0f), Quaternion.identity, 0.4f));
        #endif

        // STAGE 7: Front face visible — enable rotator and start float loop
        cardFlipped[idx] = true;
        if (rot == null) rot = card3D.AddComponent<CardRotator>();
        rot.autoSpin = false; // Disable Y-spin (makes 2D sprites paper-thin)
        rot.enabled  = true;

        #if PRIME_TWEEN_INSTALLED
        Tween.LocalPosition(card3D.transform, finalPos, finalPos + new Vector3(0f, 0.12f, 0f),
                            1.0f, Ease.InOutSine, -1, CycleMode.Yoyo);
        Tween.LocalRotation(card3D.transform, Quaternion.identity, Quaternion.Euler(0f, 0f, 2.5f),
                            1.0f, Ease.InOutSine, -1, CycleMode.Yoyo);
        #endif

        // STAGE 8: Admire pause
        yield return new WaitForSeconds(1.6f);

        // STAGE 9: Fly up and shrink away
        rot.enabled = false;
        #if PRIME_TWEEN_INSTALLED
        Tween.StopAll(card3D.transform);
        Tween.Scale(card3D.transform, Vector3.zero, 0.36f, Ease.InCubic);
        yield return Tween.Position(card3D.transform,
            card3D.transform.position + new Vector3(0f, 5f, 0f),
            0.36f, Ease.InCubic).ToYieldInstruction();
        #else
        yield return StartCoroutine(LerpScale(card3D.transform, Vector3.one, Vector3.zero, 0.36f));
        #endif
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

    private IEnumerator RarityBurst(int stars)
    {
        Color c      = RarityColor(stars);
        int   flashes = stars >= 4 ? 3 : stars >= 3 ? 2 : 1;
        for (int f = 0; f < flashes; f++)
        {
            yield return StartCoroutine(FlashScreen(c, 0.2f));
            yield return new WaitForSeconds(0.04f);
        }
        if (stars >= 5) yield return StartCoroutine(FlashScreen(colorLegendary, 0.55f));
    }

    private Color RarityColor(int stars) => stars switch
    {
        1 => colorCommon, 2 => colorUncommon, 3 => colorRare, 4 => colorEpic, _ => colorLegendary
    };

    // ── Fallback lerp helpers (used when PrimeTween is not installed) ─────────

    private IEnumerator LerpScale(Transform t, Vector3 from, Vector3 to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur) { if (t == null) yield break; elapsed += Time.deltaTime; t.localScale = Vector3.Lerp(from, to, elapsed / dur); yield return null; }
        if (t != null) t.localScale = to;
    }

    private IEnumerator LerpRotation(Transform t, Quaternion from, Quaternion to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur) { if (t == null) yield break; elapsed += Time.deltaTime; t.localRotation = Quaternion.Slerp(from, to, elapsed / dur); yield return null; }
        if (t != null) t.localRotation = to;
    }
}
