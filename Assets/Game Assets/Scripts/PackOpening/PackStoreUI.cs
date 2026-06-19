#if PRIME_TWEEN_INSTALLED
using PrimeTween;
#endif
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using TMPro;

/// <summary>
/// Manages the Store panel UI for pack opening:
/// button wiring, cooldown countdown label, idle pack wiggle, and shimmer sweep animation.
/// Initialised by PackOpeningController via Initialize() and lives on the same GameObject.
/// </summary>
public class PackStoreUI : MonoBehaviour
{
    // ── Injected refs ────────────────────────────────────────────────────────

    private Image            packArtImage;
    private TextMeshProUGUI  packNameLabel;
    private Button           openPackButton;
    private TextMeshProUGUI  openPackButtonText;
    private TextMeshProUGUI  cooldownTimerLabel;
    private Image            packShimmerOverlay;
    private Button           addToCollectionButton;

    // ── Idle animation settings ──────────────────────────────────────────────

    private float idleShakeRotation;
    private float idleShakeSpeed;
    private float idleScalePulse;
    private float idleDelayBetweenCycles;

    // ── Callbacks from coordinator ───────────────────────────────────────────

    private Action onOpenPackClicked;
    private Action onAddToCollectionClicked;

    // ── State ────────────────────────────────────────────────────────────────

    private Coroutine cooldownCoroutine;

    #if PRIME_TWEEN_INSTALLED
    private Sequence shimmerSequence;
    private Sequence idlePackSequence;
    #endif

    // ── Initialisation ───────────────────────────────────────────────────────

    /// <summary>Inject all refs. Called by PackOpeningController.Awake() before any other method.</summary>
    public void Initialize(
        Image packArtImage, TextMeshProUGUI packNameLabel,
        Button openPackButton, TextMeshProUGUI openPackButtonText,
        TextMeshProUGUI cooldownTimerLabel, Image packShimmerOverlay,
        Button addToCollectionButton,
        float idleShakeRotation, float idleShakeSpeed,
        float idleScalePulse, float idleDelayBetweenCycles,
        Action onOpenPackClicked, Action onAddToCollectionClicked)
    {
        this.packArtImage             = packArtImage;
        this.packNameLabel            = packNameLabel;
        this.openPackButton           = openPackButton;
        this.openPackButtonText       = openPackButtonText;
        this.cooldownTimerLabel       = cooldownTimerLabel;
        this.packShimmerOverlay       = packShimmerOverlay;
        this.addToCollectionButton    = addToCollectionButton;
        this.idleShakeRotation        = idleShakeRotation;
        this.idleShakeSpeed           = idleShakeSpeed;
        this.idleScalePulse           = idleScalePulse;
        this.idleDelayBetweenCycles   = idleDelayBetweenCycles;
        this.onOpenPackClicked        = onOpenPackClicked;
        this.onAddToCollectionClicked = onAddToCollectionClicked;
    }

    private bool isPackOpening = false;

    // ── Public API ───────────────────────────────────────────────────────────

    public void SetupButtons()
    {
        if (openPackButton != null)
        {
            openPackButton.onClick.RemoveAllListeners();
            openPackButton.onClick.AddListener(() => onOpenPackClicked?.Invoke());
        }
        if (addToCollectionButton != null)
        {
            addToCollectionButton.onClick.RemoveAllListeners();
            addToCollectionButton.onClick.AddListener(() => onAddToCollectionClicked?.Invoke());
        }
        if (packNameLabel != null) packNameLabel.text = "Kanto Starter Pack";
    }

    public void RefreshStorePanel()
    {
        isPackOpening = false;
        #if PRIME_TWEEN_INSTALLED
        shimmerSequence.Stop();
        if (packShimmerOverlay != null)
            packShimmerOverlay.rectTransform.anchoredPosition = new Vector2(-800f, 0f);
        #endif

        if (cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(CooldownLabelRoutine());
    }

    public void StopAll()
    {
        #if PRIME_TWEEN_INSTALLED
        shimmerSequence.Stop();
        idlePackSequence.Stop();
        #endif
        if (cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
    }

    public void SetPackButtonInteractable(bool interactable)
    {
        if (openPackButton != null) openPackButton.interactable = interactable;
    }

    public void SetPackOpeningState(bool active)
    {
        isPackOpening = active;
        if (active)
        {
            StopIdleAnimations();
        }
    }

    /// <summary>Stops idle wiggle and shimmer. Called when the pack opening begins.</summary>
    public void StopIdleAnimations()
    {
        #if PRIME_TWEEN_INSTALLED
        if (idlePackSequence.isAlive)
        {
            idlePackSequence.Stop();
            if (packArtImage != null)
            {
                packArtImage.rectTransform.localScale    = Vector3.one;
                packArtImage.rectTransform.localRotation = Quaternion.identity;
            }
        }
        if (shimmerSequence.isAlive)
        {
            shimmerSequence.Stop();
            if (packShimmerOverlay != null)
                packShimmerOverlay.rectTransform.anchoredPosition = new Vector2(-800f, 0f);
        }
        #endif
    }

    // ── Private ──────────────────────────────────────────────────────────────

    private IEnumerator CooldownLabelRoutine()
    {
        while (true)
        {
            bool ready = PlayerCollection.CanOpenFreePack();

            if (openPackButton != null)     openPackButton.interactable = ready && !isPackOpening;
            if (openPackButtonText != null) openPackButtonText.text = ready ? "✨  Open Free Pack" : "⏳  Next Pack In...";

            if (cooldownTimerLabel != null)
            {
                if (ready)
                    cooldownTimerLabel.text = "Pack ready!";
                else
                {
                    TimeSpan rem = PlayerCollection.CooldownRemaining();
                    cooldownTimerLabel.text = $"{rem.Hours:D2}:{rem.Minutes:D2}:{rem.Seconds:D2}";
                }
            }

            #if PRIME_TWEEN_INSTALLED
            if (ready && !isPackOpening) RunIdleAnimations();
            else                         StopIdleAnimations();
            #endif

            yield return new WaitForSeconds(1f);
        }
    }

    #if PRIME_TWEEN_INSTALLED
    private void RunIdleAnimations()
    {
        // Idle pack wiggle
        if (!idlePackSequence.isAlive && packArtImage != null)
        {
            packArtImage.rectTransform.localScale    = Vector3.one;
            packArtImage.rectTransform.localRotation = Quaternion.identity;

            var seq = Sequence.Create(cycles: -1);
            if (idleDelayBetweenCycles > 0f) seq.Chain(Tween.Delay(idleDelayBetweenCycles));

            seq.Group(Tween.Scale(packArtImage.rectTransform, new Vector3(idleScalePulse, idleScalePulse, 1f), idleShakeSpeed * 1.5f, Ease.OutQuad))
               .Chain(Tween.Rotation(packArtImage.rectTransform, new Vector3(0, 0, -idleShakeRotation),        idleShakeSpeed,         Ease.InOutQuad))
               .Chain(Tween.Rotation(packArtImage.rectTransform, new Vector3(0, 0,  idleShakeRotation),        idleShakeSpeed * 1.5f,  Ease.InOutQuad))
               .Chain(Tween.Rotation(packArtImage.rectTransform, new Vector3(0, 0, -idleShakeRotation * 0.7f), idleShakeSpeed * 1.2f,  Ease.InOutQuad))
               .Chain(Tween.Rotation(packArtImage.rectTransform, new Vector3(0, 0, 0f),                        idleShakeSpeed,         Ease.OutQuad))
               .Group(Tween.Scale(packArtImage.rectTransform, Vector3.one,                                      idleShakeSpeed * 1.5f,  Ease.InOutQuad));

            idlePackSequence = seq;
        }

        // Shimmer sweep
        if (!shimmerSequence.isAlive && packShimmerOverlay != null)
        {
            packShimmerOverlay.rectTransform.anchoredPosition = new Vector2(-800f, 0f);
            shimmerSequence = Sequence.Create(cycles: -1)
                .Chain(Tween.UIAnchoredPositionX(packShimmerOverlay.rectTransform, 800f,  1.6f, Ease.InOutQuad))
                .Chain(Tween.Delay(2.0f))
                .Chain(Tween.UIAnchoredPositionX(packShimmerOverlay.rectTransform, -800f, 0f));
        }
    }
    #endif
}
