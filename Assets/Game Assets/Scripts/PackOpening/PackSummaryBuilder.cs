using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Builds and displays the pack opening summary panel at the end of a reveal.
/// Spawns one icon-name-HP-rarity row per drawn card inside a VerticalLayoutGroup container.
/// Initialised by PackOpeningController and lives on the same GameObject.
/// </summary>
public class PackSummaryBuilder : MonoBehaviour
{
    // ── Injected refs ────────────────────────────────────────────────────────

    private GameObject       summaryPanel;
    private Button           addToCollectionButton;
    private TextMeshProUGUI  summaryLabel;
    private Transform        summaryCardContainer;

    // ── Rarity colours ───────────────────────────────────────────────────────

    private Color colorCommon, colorUncommon, colorRare, colorEpic, colorLegendary;

    // ── Initialisation ───────────────────────────────────────────────────────

    /// <summary>Inject all refs. Called by PackOpeningController.Awake().</summary>
    public void Initialize(
        GameObject summaryPanel, Button addToCollectionButton,
        TextMeshProUGUI summaryLabel, Transform summaryCardContainer,
        Color colorCommon, Color colorUncommon, Color colorRare,
        Color colorEpic, Color colorLegendary)
    {
        this.summaryPanel         = summaryPanel;
        this.addToCollectionButton = addToCollectionButton;
        this.summaryLabel          = summaryLabel;
        this.summaryCardContainer  = summaryCardContainer;
        this.colorCommon           = colorCommon;
        this.colorUncommon         = colorUncommon;
        this.colorRare             = colorRare;
        this.colorEpic             = colorEpic;
        this.colorLegendary        = colorLegendary;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>Clears old rows, spawns a new icon row per card, then shows the panel.</summary>
    public void ShowSummaryPanel(List<PokemonCardData> drawnCards)
    {
        if (summaryPanel == null) return;

        if (addToCollectionButton != null) addToCollectionButton.interactable = true;

        // Clear legacy label
        if (summaryLabel != null) summaryLabel.text = "";

        // Rebuild card rows
        if (summaryCardContainer != null)
        {
            for (int i = summaryCardContainer.childCount - 1; i >= 0; i--)
                Destroy(summaryCardContainer.GetChild(i).gameObject);

            foreach (var card in drawnCards)
            {
                if (card != null) SpawnCardRow(card);
            }
        }

        summaryPanel.SetActive(true);

        #if PRIME_TWEEN_INSTALLED
        summaryPanel.transform.localScale = Vector3.zero;
        PrimeTween.Tween.Scale(summaryPanel.transform, Vector3.one, 0.45f, PrimeTween.Ease.OutBack);
        #endif
    }

    // ── Private row builder ──────────────────────────────────────────────────

    private void SpawnCardRow(PokemonCardData card)
    {
        // Row root — HorizontalLayoutGroup arranges icon + labels left-to-right
        GameObject row       = new GameObject("CardRow", typeof(RectTransform));
        row.transform.SetParent(summaryCardContainer, false);

        LayoutElement rowLE   = row.AddComponent<LayoutElement>();
        rowLE.minHeight       = 96f;
        rowLE.preferredHeight = 96f;
        rowLE.flexibleWidth   = 1f;

        HorizontalLayoutGroup hlg  = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment         = TextAnchor.MiddleLeft;
        hlg.spacing                = 18f;
        hlg.padding                = new RectOffset(14, 14, 10, 10);
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;

        Image rowBg = row.AddComponent<Image>();
        rowBg.color = new Color(1f, 1f, 1f, 0.05f); // subtle row separator

        // ── Icon ────────────────────────────────────────────────────────────
        GameObject iconObj    = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(row.transform, false);
        LayoutElement iconLE  = iconObj.AddComponent<LayoutElement>();
        iconLE.minWidth        = 76f;
        iconLE.preferredWidth  = 76f;
        iconLE.flexibleWidth   = 0f;
        Image iconImg          = iconObj.GetComponent<Image>();
        iconImg.sprite         = card.pokemonSprite;
        iconImg.preserveAspect = true;
        iconImg.color          = card.pokemonSprite != null ? Color.white : new Color(0f, 0f, 0f, 0f);

        // ── Name (flexible — fills remaining space) ──────────────────────────
        GameObject nameObj       = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameObj.transform.SetParent(row.transform, false);
        LayoutElement nameLE     = nameObj.AddComponent<LayoutElement>();
        nameLE.preferredWidth    = 200f;
        nameLE.flexibleWidth     = 1f;
        TextMeshProUGUI nameTMP  = nameObj.GetComponent<TextMeshProUGUI>();
        nameTMP.text             = $"<b>{card.pokemonName}</b>";
        nameTMP.fontSize         = 32f;
        nameTMP.color            = Color.white;
        nameTMP.alignment        = TextAlignmentOptions.MidlineLeft;
        nameTMP.overflowMode     = TextOverflowModes.Ellipsis;

        // ── HP (fixed width) ────────────────────────────────────────────────
        GameObject hpObj        = new GameObject("HP", typeof(RectTransform), typeof(TextMeshProUGUI));
        hpObj.transform.SetParent(row.transform, false);
        LayoutElement hpLE      = hpObj.AddComponent<LayoutElement>();
        hpLE.preferredWidth     = 120f;
        hpLE.flexibleWidth      = 0f;
        TextMeshProUGUI hpTMP   = hpObj.GetComponent<TextMeshProUGUI>();
        hpTMP.text              = $"<color=#7EC8E3><b>HP {card.hp}</b></color>";
        hpTMP.fontSize          = 26f;
        hpTMP.alignment         = TextAlignmentOptions.Center;
        hpTMP.color             = Color.white;

        // ── Rarity (fixed width, right-aligned) ─────────────────────────────
        GameObject rarityObj       = new GameObject("Rarity", typeof(RectTransform), typeof(TextMeshProUGUI));
        rarityObj.transform.SetParent(row.transform, false);
        LayoutElement rarityLE     = rarityObj.AddComponent<LayoutElement>();
        rarityLE.preferredWidth    = 150f;
        rarityLE.flexibleWidth     = 0f;
        TextMeshProUGUI rarityTMP  = rarityObj.GetComponent<TextMeshProUGUI>();
        Color  rarCol              = RarityColor(card.rarityStars);
        string hex                 = ColorUtility.ToHtmlStringRGB(rarCol);
        rarityTMP.text             = $"<b><color=#{hex}>{RarityName(card.rarityStars)}</color></b>";
        rarityTMP.fontSize         = 26f;
        rarityTMP.alignment        = TextAlignmentOptions.MidlineRight;
        rarityTMP.color            = Color.white;
    }

    // ── Rarity helpers ───────────────────────────────────────────────────────

    private string RarityName(int stars) => stars switch
    {
        1 => "Common", 2 => "Uncommon", 3 => "Rare", 4 => "Epic", _ => "Legendary"
    };

    private Color RarityColor(int stars) => stars switch
    {
        1 => colorCommon, 2 => colorUncommon, 3 => colorRare, 4 => colorEpic, _ => colorLegendary
    };
}
