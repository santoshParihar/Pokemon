using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

[ExecuteAlways]
public class Card2DUIController : MonoBehaviour
{
    [Header("Card Data")]
    [SerializeField] private PokemonCardData cardData;

    [Header("UI Image/Background Fields")]
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

    [Header("UI Text Fields")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI pokedexClassText;
    
    [Header("UI Attack Fields")]
    [SerializeField] private TextMeshProUGUI attack1Name;
    [SerializeField] private TextMeshProUGUI attack1Cost;
    [SerializeField] private TextMeshProUGUI attack1Damage;
    [SerializeField] private TextMeshProUGUI attack1Description;

    [SerializeField] private TextMeshProUGUI attack2Name;
    [SerializeField] private TextMeshProUGUI attack2Cost;
    [SerializeField] private TextMeshProUGUI attack2Damage;
    [SerializeField] private TextMeshProUGUI attack2Description;

    [Header("UI Stats Badge Fields")]
    [SerializeField] private TextMeshProUGUI badgeStageTmp;
    [SerializeField] private TextMeshProUGUI badgeCPTmp;
    [SerializeField] private TextMeshProUGUI badgeRetreatTmp;
    [SerializeField] private TextMeshProUGUI badgeWeakTmp;
    [SerializeField] private TextMeshProUGUI badgeResistTmp;
    [SerializeField] private TextMeshProUGUI badgeRarityTmp;

    private Button clickButton;
    private System.Action<PokemonCardData> onClickCallback;

    private void Awake()
    {
        UpdateCardUI();
        SetupButtonListener();
    }

    private void Start()
    {
        UpdateCardUI();
        SetupButtonListener();
    }

    private void OnValidate()
    {
        UpdateCardUI();
    }

    private void SetupButtonListener()
    {
        if (clickButton == null) clickButton = GetComponent<Button>();
        if (clickButton != null)
        {
            clickButton.onClick.RemoveAllListeners();
            clickButton.onClick.AddListener(() =>
            {
                if (onClickCallback != null && cardData != null)
                {
                    onClickCallback(cardData);
                }
            });
        }
    }

    public void SetupClick(System.Action<PokemonCardData> onClick)
    {
        onClickCallback = onClick;
        SetupButtonListener();
    }

    [ContextMenu("Refresh Card UI")]
    public void UpdateCardUI()
    {
        if (cardData == null) return;

        // Update Background Graphic
        if (bgImage != null)
        {
            Sprite bgSprite = null;
            if (cardData.customBackgroundSprite != null)
            {
                bgSprite = cardData.customBackgroundSprite;
            }
            else
            {
                foreach (var mapping in typeBackgrounds)
                {
                    if (mapping.type == cardData.cardType)
                    {
                        bgSprite = mapping.backgroundSprite;
                        break;
                    }
                }
            }

            if (bgSprite == null) bgSprite = defaultFrontSprite;
            bgImage.sprite = bgSprite;
        }

        // Base text elements
        if (nameText != null) nameText.text = cardData.pokemonName;
        if (hpText != null) hpText.text = $"{cardData.hp} HP";
        if (typeText != null) typeText.text = cardData.cardType.ToString();
        if (pokedexClassText != null) pokedexClassText.text = cardData.pokedexClass;

        if (pokemonImage != null)
        {
            pokemonImage.sprite = cardData.pokemonSprite;
            pokemonImage.enabled = cardData.pokemonSprite != null;
        }

        // Setup layouts for Ability & Attack fields (mimics the 3D details alignment)
        Vector2 leftAnchor = new Vector2(0f, 0.5f);
        Vector2 leftPivot = new Vector2(0f, 0.5f);
        Vector2 rightAnchor = new Vector2(1f, 0.5f);
        Vector2 rightPivot = new Vector2(1f, 0.5f);
        Vector2 descPivot = new Vector2(0f, 1f);

        if (attack1Cost != null) { attack1Cost.rectTransform.anchorMin = leftAnchor; attack1Cost.rectTransform.anchorMax = leftAnchor; attack1Cost.rectTransform.pivot = leftPivot; }
        if (attack1Name != null) { attack1Name.rectTransform.anchorMin = leftAnchor; attack1Name.rectTransform.anchorMax = leftAnchor; attack1Name.rectTransform.pivot = leftPivot; }
        if (attack2Cost != null) { attack2Cost.rectTransform.anchorMin = leftAnchor; attack2Cost.rectTransform.anchorMax = leftAnchor; attack2Cost.rectTransform.pivot = leftPivot; }
        if (attack2Name != null) { attack2Name.rectTransform.anchorMin = leftAnchor; attack2Name.rectTransform.anchorMax = leftAnchor; attack2Name.rectTransform.pivot = leftPivot; }

        if (attack1Damage != null) { attack1Damage.rectTransform.anchorMin = rightAnchor; attack1Damage.rectTransform.anchorMax = rightAnchor; attack1Damage.rectTransform.pivot = rightPivot; }
        if (attack2Damage != null) { attack2Damage.rectTransform.anchorMin = rightAnchor; attack2Damage.rectTransform.anchorMax = rightAnchor; attack2Damage.rectTransform.pivot = rightPivot; }

        if (attack1Description != null)
        {
            attack1Description.rectTransform.anchorMin = leftAnchor;
            attack1Description.rectTransform.anchorMax = leftAnchor;
            attack1Description.rectTransform.pivot = descPivot;
            attack1Description.rectTransform.anchoredPosition = new Vector2(25, 14);
            attack1Description.rectTransform.sizeDelta = new Vector2(530, 70);
            attack1Description.fontStyle = FontStyles.Bold;
            attack1Description.enableAutoSizing = true;
            attack1Description.fontSizeMin = 16;
            attack1Description.fontSizeMax = 22;
            attack1Description.enableWordWrapping = true;
            attack1Description.color = new Color(0.12f, 0.14f, 0.17f, 0.95f);
        }
        if (attack2Description != null)
        {
            attack2Description.rectTransform.anchorMin = leftAnchor;
            attack2Description.rectTransform.anchorMax = leftAnchor;
            attack2Description.rectTransform.pivot = descPivot;
            attack2Description.rectTransform.anchoredPosition = new Vector2(25, 14);
            attack2Description.rectTransform.sizeDelta = new Vector2(530, 70);
            attack2Description.fontStyle = FontStyles.Bold;
            attack2Description.enableAutoSizing = true;
            attack2Description.fontSizeMin = 16;
            attack2Description.fontSizeMax = 22;
            attack2Description.enableWordWrapping = true;
            attack2Description.color = new Color(0.12f, 0.14f, 0.17f, 0.95f);
        }

        System.Func<string, float> getCostWidth = (costText) =>
        {
            float baseWidth = 250f;
            if (string.IsNullOrEmpty(costText)) return baseWidth;
            string[] parts = costText.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            int activeParts = 0;
            foreach (var part in parts)
            {
                string t = part.Trim().ToUpper();
                if (t.StartsWith("[") && t.EndsWith("]")) t = t.Substring(1, t.Length - 2);
                if (t != "C" && t != "COLORLESS") activeParts++;
            }
            return baseWidth + activeParts * 85f;
        };

        RectTransform slot1Rect = (attack1Name != null) ? attack1Name.transform.parent as RectTransform : null;
        RectTransform slot2Rect = (attack2Name != null) ? attack2Name.transform.parent as RectTransform : null;

        bool hasOnlyOneAttack = !cardData.ability.hasAbility;

        if (slot1Rect != null && slot2Rect != null)
        {
            if (hasOnlyOneAttack)
            {
                slot1Rect.anchorMin = new Vector2(0f, 0.25f);
                slot1Rect.anchorMax = new Vector2(1f, 0.75f);
                slot1Rect.offsetMin = new Vector2(0, 5);
                slot1Rect.offsetMax = new Vector2(0, -5);
                slot1Rect.gameObject.name = "Attack";
                slot2Rect.gameObject.name = "DisabledSlot";
                slot2Rect.gameObject.SetActive(false);

                if (attack1Name != null) attack1Name.gameObject.name = "AttackName";
                if (attack1Cost != null) attack1Cost.gameObject.name = "AttackCost";
                if (attack1Damage != null) attack1Damage.gameObject.name = "AttackDamage";
                if (attack1Description != null) attack1Description.gameObject.name = "AttackDescription";
            }
            else
            {
                slot1Rect.anchorMin = new Vector2(0f, 0.5f);
                slot1Rect.anchorMax = new Vector2(1f, 1f);
                slot1Rect.offsetMin = new Vector2(0, 5);
                slot1Rect.offsetMax = new Vector2(0, -5);
                slot1Rect.gameObject.name = "Ability";
                
                slot2Rect.anchorMin = new Vector2(0f, 0f);
                slot2Rect.anchorMax = new Vector2(1f, 0.5f);
                slot2Rect.offsetMin = new Vector2(0, 5);
                slot2Rect.offsetMax = new Vector2(0, -5);
                slot2Rect.gameObject.name = "Attack";
                slot2Rect.gameObject.SetActive(true);

                if (attack1Name != null) attack1Name.gameObject.name = "AbilityName";
                if (attack1Cost != null) attack1Cost.gameObject.name = "AbilityLabel";
                if (attack1Damage != null) attack1Damage.gameObject.name = "UnusedDamageField";
                if (attack1Description != null) attack1Description.gameObject.name = "AbilityDescription";

                if (attack2Name != null) attack2Name.gameObject.name = "AttackName";
                if (attack2Cost != null) attack2Cost.gameObject.name = "AttackCost";
                if (attack2Damage != null) attack2Damage.gameObject.name = "AttackDamage";
                if (attack2Description != null) attack2Description.gameObject.name = "AttackDescription";
            }
        }

        if (attack1Cost != null)
        {
            attack1Cost.enableAutoSizing = false;
            attack1Cost.fontSize = 28;
            attack1Cost.enableWordWrapping = false;
        }
        if (attack2Cost != null)
        {
            attack2Cost.enableAutoSizing = false;
            attack2Cost.fontSize = 28;
            attack2Cost.enableWordWrapping = false;
        }

        // Details Populate
        if (cardData.ability.hasAbility)
        {
            float abilityLabelWidth = 250f + (string.IsNullOrEmpty(cardData.ability.abilityName) ? 0f : cardData.ability.abilityName.Length * 14f);
            if (attack1Cost != null)
            {
                attack1Cost.text = "<color=#1E222B><b>Ability</b></color> <color=#C1121F><b>[" + cardData.ability.abilityName + "]</b></color>";
                attack1Cost.rectTransform.anchoredPosition = new Vector2(25, 32);
                attack1Cost.rectTransform.sizeDelta = new Vector2(abilityLabelWidth, 36);
            }
            if (attack1Name != null) attack1Name.text = "";
            if (attack1Damage != null) attack1Damage.text = "";
            if (attack1Description != null) attack1Description.text = cardData.ability.abilityDescription;

            float costWidth2 = getCostWidth(cardData.attack1CostText);
            if (attack2Cost != null)
            {
                attack2Cost.text = "<color=#1E222B><b>Attack</b></color> " + FormatCostText(cardData.attack1CostText);
                attack2Cost.rectTransform.anchoredPosition = new Vector2(25, 32);
                attack2Cost.rectTransform.sizeDelta = new Vector2(costWidth2, 36);
            }
            if (attack2Name != null) attack2Name.text = "";
            if (attack2Damage != null) attack2Damage.text = cardData.attack1Damage > 0 ? $"<color=#1E222B><b>{cardData.attack1Damage}</b></color>" : "";
            if (attack2Description != null) attack2Description.text = cardData.attack1Description;
        }
        else
        {
            float costWidth1 = getCostWidth(cardData.attack1CostText);
            if (attack1Cost != null)
            {
                attack1Cost.text = "<color=#1E222B><b>Attack</b></color> " + FormatCostText(cardData.attack1CostText);
                attack1Cost.rectTransform.anchoredPosition = new Vector2(25, 32);
                attack1Cost.rectTransform.sizeDelta = new Vector2(costWidth1, 36);
            }
            if (attack1Name != null) attack1Name.text = "";
            if (attack1Damage != null) attack1Damage.text = cardData.attack1Damage > 0 ? $"<color=#1E222B><b>{cardData.attack1Damage}</b></color>" : "";
            if (attack1Description != null) attack1Description.text = cardData.attack1Description;

            if (attack2Name != null) attack2Name.text = "";
            if (attack2Cost != null) attack2Cost.text = "";
            if (attack2Damage != null) attack2Damage.text = "";
            if (attack2Description != null) attack2Description.text = "";
        }

        // Badges Setup
        if (badgeStageTmp != null) badgeStageTmp.text = $"<color=#1E222B><b>Stage: {cardData.stage}</b></color>";
        if (badgeCPTmp != null)
        {
            int dynamicCP = cardData.hp * 5 + cardData.attack1Damage * 3;
            badgeCPTmp.text = $"<color=#1E222B><b>CP {dynamicCP}</b></color>";
        }
        if (badgeRetreatTmp != null)
        {
            int cost = Mathf.Clamp(cardData.retreatCost, 0, 5);
            string retreatTextValue = cost == 0 ? "None" : cost.ToString();
            badgeRetreatTmp.text = $"<color=#1E222B><b>Retreat: {retreatTextValue}</b></color>";
        }
        if (badgeWeakTmp != null) badgeWeakTmp.text = $"<color=#1E222B><b>Weak: {cardData.weakness.ToString()} {cardData.weaknessValue}</b></color>";
        if (badgeResistTmp != null)
        {
            badgeResistTmp.text = cardData.hasResistance ? $"<color=#1E222B><b>Resist: {cardData.resistance.ToString()} {cardData.resistanceValue}</b></color>" : "<color=#1E222B><b>Resist: None</b></color>";
        }
        if (badgeRarityTmp != null)
        {
            int stars = Mathf.Clamp(cardData.rarityStars, 1, 5);
            string rarityName = "";
            switch (stars)
            {
                case 1: rarityName = "Common"; break;
                case 2: rarityName = "Uncommon"; break;
                case 3: rarityName = "Rare"; break;
                case 4: rarityName = "Epic"; break;
                case 5: rarityName = "Legendary"; break;
            }
            badgeRarityTmp.text = $"<color=#1E222B><b>Rarity: {rarityName}</b></color>";
        }
    }

    private string FormatCostText(string rawCost)
    {
        if (string.IsNullOrEmpty(rawCost)) return "";
        List<string> formattedParts = new List<string>();
        string[] tokens = rawCost.Split(' ');
        foreach (var token in tokens)
        {
            string t = token.Trim().ToUpper();
            if (t.StartsWith("[") && t.EndsWith("]")) t = t.Substring(1, t.Length - 2);
            switch (t)
            {
                case "C":
                case "COLORLESS":
                    break;
                case "G":
                case "GRASS":
                    formattedParts.Add("<color=#1E5F34><b>[Grass]</b></color>");
                    break;
                case "F":
                case "FIRE":
                    formattedParts.Add("<color=#9B2226><b>[Fire]</b></color>");
                    break;
                case "W":
                case "WATER":
                    formattedParts.Add("<color=#0A4F8F><b>[Water]</b></color>");
                    break;
                case "L":
                case "LIGHTNING":
                    formattedParts.Add("<color=#B58A03><b>[Lightning]</b></color>");
                    break;
                case "P":
                case "PSYCHIC":
                    formattedParts.Add("<color=#5A189A><b>[Psychic]</b></color>");
                    break;
                case "FTR":
                case "FIGHT":
                case "FIGHTING":
                    formattedParts.Add("<color=#7F3F10><b>[Fighting]</b></color>");
                    break;
                case "D":
                case "DARK":
                case "DARKNESS":
                    formattedParts.Add("<color=#1A2530><b>[Darkness]</b></color>");
                    break;
                case "M":
                case "METAL":
                case "STEEL":
                    formattedParts.Add("<color=#4E5E60><b>[Metal]</b></color>");
                    break;
                case "Y":
                case "DRAGON":
                    formattedParts.Add("<color=#B05C00><b>[Dragon]</b></color>");
                    break;
                default:
                    if (!string.IsNullOrEmpty(token)) formattedParts.Add($"<b>[{token}]</b>");
                    break;
            }
        }
        return string.Join(" ", formattedParts);
    }

    public void SetCardData(PokemonCardData data)
    {
        cardData = data;
        UpdateCardUI();
    }
}
