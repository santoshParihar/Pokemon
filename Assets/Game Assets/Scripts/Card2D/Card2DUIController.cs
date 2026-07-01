using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Thin coordinator for the 2D Pokémon card prefab.
/// All display logic is delegated to single-responsibility static handler classes:
///   Card2DBackgroundHandler  — background sprite
///   Card2DTextHandler        — name / HP / type / pokedex class
///   Card2DAttackHandler      — attack &amp; ability slot layout + cost formatting
///   Card2DBadgeHandler       — stage / CP / retreat / weakness / resist / rarity badges
/// </summary>
public class Card2DUIController : MonoBehaviour
{
    // ── Data ─────────────────────────────────────────────────────────────────

    [Header("Card Data")]
    [SerializeField] private PokemonCardData cardData;

    public PokemonCardData CardData => cardData;

    // ── Background ───────────────────────────────────────────────────────────

    [Header("UI Image / Background Fields")]
    [SerializeField] private Image bgImage;
    [SerializeField] private Image pokemonImage;

    [System.Serializable]
    public struct TypeSpriteMapping
    {
        public PokemonType type;
        public Sprite backgroundSprite;
    }
    [SerializeField] private List<TypeSpriteMapping> typeBackgrounds = new List<TypeSpriteMapping>();
    [SerializeField] private Sprite defaultFrontSprite;

    // ── Text ─────────────────────────────────────────────────────────────────

    [Header("UI Text Fields")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI pokedexClassText;
    [SerializeField] private TextMeshProUGUI priceText;

    // ── Attacks ──────────────────────────────────────────────────────────────

    [Header("UI Attack Fields")]
    [SerializeField] private TextMeshProUGUI attack1Name;
    [SerializeField] private TextMeshProUGUI attack1Cost;
    [SerializeField] private TextMeshProUGUI attack1Damage;
    [SerializeField] private TextMeshProUGUI attack1Description;

    [SerializeField] private TextMeshProUGUI attack2Name;
    [SerializeField] private TextMeshProUGUI attack2Cost;
    [SerializeField] private TextMeshProUGUI attack2Damage;
    [SerializeField] private TextMeshProUGUI attack2Description;

    // ── Badges ───────────────────────────────────────────────────────────────

    [Header("UI Stats Badge Fields")]
    [SerializeField] private TextMeshProUGUI badgeStageTmp;
    [SerializeField] private TextMeshProUGUI badgeCPTmp;
    [SerializeField] private TextMeshProUGUI badgeRetreatTmp;
    [SerializeField] private TextMeshProUGUI badgeWeakTmp;
    [SerializeField] private TextMeshProUGUI badgeResistTmp;
    [SerializeField] private TextMeshProUGUI badgeRarityTmp;

    // ── Click handling ───────────────────────────────────────────────────────

    private Button clickButton;
    private System.Action<PokemonCardData> onClickCallback;

    // ── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()    { UpdateCardUI(); SetupButtonListener(); }
    private void Start()    { UpdateCardUI(); SetupButtonListener(); }
    private void OnValidate() => UpdateCardUI();

    // ── Public API ───────────────────────────────────────────────────────────

    public void SetCardData(PokemonCardData data)
    {
        cardData = data;
        UpdateCardUI();
    }

    public void SetupClick(System.Action<PokemonCardData> onClick)
    {
        onClickCallback = onClick;
        SetupButtonListener();
    }

    /// <summary>Refreshes all card UI fields from the current cardData.</summary>
    [ContextMenu("Refresh Card UI")]
    public void UpdateCardUI()
    {
        if (cardData == null) return;

        // Delegate each concern to its dedicated handler
        Card2DBackgroundHandler.Apply(cardData, bgImage, typeBackgrounds, defaultFrontSprite);
        Card2DTextHandler.Apply(cardData, nameText, hpText, typeText, pokedexClassText, pokemonImage);
        if (priceText != null) priceText.text = $"${cardData.marketPrice:F2}";
        Card2DAttackHandler.Apply(cardData,
            attack1Name, attack1Cost, attack1Damage, attack1Description,
            attack2Name, attack2Cost, attack2Damage, attack2Description);
        Card2DBadgeHandler.Apply(cardData,
            badgeStageTmp, badgeCPTmp, badgeRetreatTmp,
            badgeWeakTmp, badgeResistTmp, badgeRarityTmp);
    }

    // ── Private ──────────────────────────────────────────────────────────────

    private void SetupButtonListener()
    {
        if (clickButton == null) clickButton = GetComponent<Button>();
        if (clickButton == null) return;
        clickButton.onClick.RemoveAllListeners();
        clickButton.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSound();
            if (onClickCallback != null && cardData != null)
                onClickCallback(cardData);
        });
    }
}
