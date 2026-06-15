using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public enum CardCanvasSide
{
    FrontSide,
    BackSide
}

[ExecuteAlways]
public class CardUIController : MonoBehaviour
{
    [Header("Card Data")]
    [SerializeField] private PokemonCardData cardData;

    [Header("Card Physical Dimensions (for Canvas Alignment)")]
    [SerializeField] private float cardWidth = 1.0f;
    [SerializeField] private float cardHeight = 1.4f;
    [SerializeField] private float cardThickness = 0.02f;

    [Header("Visual Mappings")]
    [SerializeField] private Material baseFrontMaterial;
    [SerializeField] private Material baseBackMaterial;
    [SerializeField] private Material baseEdgeMaterial;
    
    [System.Serializable]
    public struct TypeTextureMapping
    {
        public PokemonType type;
        public Texture2D backgroundTexture;
    }
    [SerializeField] private List<TypeTextureMapping> typeBackgrounds = new List<TypeTextureMapping>();
    [SerializeField] private Texture2D cardBackTexture;
    [SerializeField] private Texture2D defaultFrontTexture;

    [Header("Canvas & Transform Setup")]
    [SerializeField] private Canvas worldSpaceCanvas;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private CardCanvasSide canvasSide = CardCanvasSide.FrontSide;

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

    [Header("UI Sub-Panel Fields")]
    [SerializeField] private Image pokemonImage;
    [SerializeField] private TextMeshProUGUI weaknessText;
    [SerializeField] private TextMeshProUGUI resistanceText;

    [Header("UI Text Icon Replacements (ASCII *)")]
    [SerializeField] private TextMeshProUGUI retreatText;
    [SerializeField] private TextMeshProUGUI rarityText;

    [Header("UI Stats Badge Fields")]
    [SerializeField] private TextMeshProUGUI badgeStageTmp;
    [SerializeField] private TextMeshProUGUI badgeCPTmp;
    [SerializeField] private TextMeshProUGUI badgeRetreatTmp;
    [SerializeField] private TextMeshProUGUI badgeWeakTmp;
    [SerializeField] private TextMeshProUGUI badgeResistTmp;
    [SerializeField] private TextMeshProUGUI badgeRarityTmp;

    private MeshRenderer meshRenderer;

    private Material instancedFrontMaterial;
    private Material instancedBackMaterial;
    private Material instancedEdgeMaterial;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        UpdateCardUI();
    }

    private void Start()
    {
        UpdateCardUI();
    }

    private void OnValidate()
    {
        UpdateCardUI();
    }

    private void Update()
    {
        // Keep canvas aligned in editor mode as well
        AlignCanvas();
    }

    [ContextMenu("Refresh Card UI")]
    public void UpdateCardUI()
    {
        if (cardData == null) return;

        // Ensure component is cached
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

        // 1. Update Materials
        ApplyMaterials();

        // 2. Update Texts and Images
        if (nameText != null) nameText.text = cardData.pokemonName;
        if (hpText != null) hpText.text = $"{cardData.hp} HP";
        if (typeText != null) typeText.text = cardData.cardType.ToString();
        if (pokedexClassText != null) pokedexClassText.text = cardData.pokedexClass;

        if (pokemonImage != null)
        {
            pokemonImage.sprite = cardData.pokemonSprite;
            pokemonImage.enabled = cardData.pokemonSprite != null;
        }

        // Setup Auto-sizing for cost fields to prevent wrapping/overlapping
        if (attack1Cost != null)
        {
            attack1Cost.enableAutoSizing = true;
            attack1Cost.fontSizeMin = 10;
            attack1Cost.fontSizeMax = 28;
            attack1Cost.enableWordWrapping = false;
        }
        if (attack2Cost != null)
        {
            attack2Cost.enableAutoSizing = true;
            attack2Cost.fontSizeMin = 10;
            attack2Cost.fontSizeMax = 28;
            attack2Cost.enableWordWrapping = false;
        }

        // Force non-stretching anchors and left-aligned pivots for attacks and costs to enable absolute positioning without overlaps
        Vector2 leftAnchor = new Vector2(0f, 0.5f);
        Vector2 leftPivot = new Vector2(0f, 0.5f);
        if (attack1Cost != null) { attack1Cost.rectTransform.anchorMin = leftAnchor; attack1Cost.rectTransform.anchorMax = leftAnchor; attack1Cost.rectTransform.pivot = leftPivot; }
        if (attack1Name != null) { attack1Name.rectTransform.anchorMin = leftAnchor; attack1Name.rectTransform.anchorMax = leftAnchor; attack1Name.rectTransform.pivot = leftPivot; }
        if (attack2Cost != null) { attack2Cost.rectTransform.anchorMin = leftAnchor; attack2Cost.rectTransform.anchorMax = leftAnchor; attack2Cost.rectTransform.pivot = leftPivot; }
        if (attack2Name != null) { attack2Name.rectTransform.anchorMin = leftAnchor; attack2Name.rectTransform.anchorMax = leftAnchor; attack2Name.rectTransform.pivot = leftPivot; }

        Vector2 rightAnchor = new Vector2(1f, 0.5f);
        Vector2 rightPivot = new Vector2(1f, 0.5f);
        if (attack1Damage != null) { attack1Damage.rectTransform.anchorMin = rightAnchor; attack1Damage.rectTransform.anchorMax = rightAnchor; attack1Damage.rectTransform.pivot = rightPivot; }
        if (attack2Damage != null) { attack2Damage.rectTransform.anchorMin = rightAnchor; attack2Damage.rectTransform.anchorMax = rightAnchor; attack2Damage.rectTransform.pivot = rightPivot; }

        Vector2 descPivot = new Vector2(0f, 1f); // Top-left pivot for predictable vertical layouts
        if (attack1Description != null)
        {
            attack1Description.rectTransform.anchorMin = leftAnchor;
            attack1Description.rectTransform.anchorMax = leftAnchor;
            attack1Description.rectTransform.pivot = descPivot;
            attack1Description.rectTransform.anchoredPosition = new Vector2(25, 14);
            attack1Description.rectTransform.sizeDelta = new Vector2(530, 70);
            attack1Description.fontStyle = FontStyles.Bold; // Bold font style as requested!
            attack1Description.fontSize = 22; // Aligned size for multi-line support
            attack1Description.color = new Color(0.12f, 0.14f, 0.17f, 0.95f); // Rich dark charcoal
        }
        if (attack2Description != null)
        {
            attack2Description.rectTransform.anchorMin = leftAnchor;
            attack2Description.rectTransform.anchorMax = leftAnchor;
            attack2Description.rectTransform.pivot = descPivot;
            attack2Description.rectTransform.anchoredPosition = new Vector2(25, 14);
            attack2Description.rectTransform.sizeDelta = new Vector2(530, 70);
            attack2Description.fontStyle = FontStyles.Bold; // Bold font style as requested!
            attack2Description.fontSize = 22; // Aligned size for multi-line support
            attack2Description.color = new Color(0.12f, 0.14f, 0.17f, 0.95f); // Rich dark charcoal
        }



        // Helper function to get custom width based on energy cost string token length (accounting for "Attack" title prefix and ignoring colorless C)
        System.Func<string, float> getCostWidth = (costText) =>
        {
            float baseWidth = 250f; // Width of "Attack" word plus space (increased to prevent clipping/autosizing shrink)
            if (string.IsNullOrEmpty(costText)) return baseWidth;
            string[] parts = costText.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            int activeParts = 0;
            foreach (var part in parts)
            {
                string t = part.Trim().ToUpper();
                if (t.StartsWith("[") && t.EndsWith("]"))
                {
                    t = t.Substring(1, t.Length - 2);
                }
                if (t != "C" && t != "COLORLESS") activeParts++;
            }
            return baseWidth + activeParts * 85f;
        };

        // Retrieve parent Slot RectTransforms for Slot 1 and Slot 2
        RectTransform slot1Rect = (attack1Name != null) ? attack1Name.transform.parent as RectTransform : null;
        RectTransform slot2Rect = (attack2Name != null) ? attack2Name.transform.parent as RectTransform : null;

        // Determine if we only have 1 attack (no ability)
        bool hasOnlyOneAttack = !cardData.ability.hasAbility;

        // Adjust vertical slot positioning and active states based on action count
        if (slot1Rect != null && slot2Rect != null)
        {
            if (hasOnlyOneAttack)
            {
                // Only 1 attack: Center Slot 1 vertically in the attacks panel and deactivate Slot 2
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
                // Multi-action (Ability+Attack): Restore default split layout
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

        // Disable auto-sizing and use a fixed font size to ensure "Attack" and "Ability" titles are bold and prominent without shrinking
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

        // Attack & Ability Layout
        if (cardData.ability.hasAbility)
        {
            // Slot 1: Display Ability
            float abilityLabelWidth = 250f + (string.IsNullOrEmpty(cardData.ability.abilityName) ? 0f : cardData.ability.abilityName.Length * 14f);
            if (attack1Cost != null)
            {
                attack1Cost.text = "<color=#1E222B><b>Ability</b></color> <color=#C1121F><b>[" + cardData.ability.abilityName + "]</b></color>";
                attack1Cost.rectTransform.anchoredPosition = new Vector2(25, 32);
                attack1Cost.rectTransform.sizeDelta = new Vector2(abilityLabelWidth, 36);
            }
            if (attack1Name != null)
            {
                attack1Name.text = ""; // Removed ability name completely from the separate field since it's now in the title
            }
            if (attack1Damage != null) attack1Damage.text = "";
            if (attack1Description != null) attack1Description.text = cardData.ability.abilityDescription;

            // Slot 2: Display Attack 1
            float costWidth2 = getCostWidth(cardData.attack1CostText);
            if (attack2Cost != null)
            {
                attack2Cost.text = "<color=#1E222B><b>Attack</b></color> " + FormatCostText(cardData.attack1CostText);
                attack2Cost.rectTransform.anchoredPosition = new Vector2(25, 32);
                attack2Cost.rectTransform.sizeDelta = new Vector2(costWidth2, 36);
            }
            if (attack2Name != null)
            {
                attack2Name.text = ""; // Removed attack name completely
            }
            if (attack2Damage != null) attack2Damage.text = cardData.attack1Damage > 0 ? $"<color=#1E222B><b>{cardData.attack1Damage}</b></color>" : "";
            if (attack2Description != null) attack2Description.text = cardData.attack1Description;
        }
        else
        {
            // Standard: Slot 1 is Attack 1, Slot 2 is Cleared
            float costWidth1 = getCostWidth(cardData.attack1CostText);
            if (attack1Cost != null)
            {
                attack1Cost.text = "<color=#1E222B><b>Attack</b></color> " + FormatCostText(cardData.attack1CostText);
                attack1Cost.rectTransform.anchoredPosition = new Vector2(25, 32);
                attack1Cost.rectTransform.sizeDelta = new Vector2(costWidth1, 36);
            }
            if (attack1Name != null)
            {
                attack1Name.text = ""; // Removed attack name completely
            }
            if (attack1Damage != null) attack1Damage.text = cardData.attack1Damage > 0 ? $"<color=#1E222B><b>{cardData.attack1Damage}</b></color>" : "";
            if (attack1Description != null) attack1Description.text = cardData.attack1Description;

            if (attack2Name != null) attack2Name.text = "";
            if (attack2Cost != null) attack2Cost.text = "";
            if (attack2Damage != null) attack2Damage.text = "";
            if (attack2Description != null) attack2Description.text = "";
        }

        // Weakness / Resistance
        if (weaknessText != null) weaknessText.text = $"{cardData.weaknessValue} {cardData.weakness.ToString()}";
        if (resistanceText != null)
        {
            resistanceText.text = cardData.hasResistance ? $"{cardData.resistanceValue} {cardData.resistance.ToString()}" : "None";
        }

        // Retreat & Rarity texts via normal numbers/labels
        if (retreatText != null)
        {
            int cost = Mathf.Clamp(cardData.retreatCost, 0, 5);
            retreatText.text = cost.ToString();
        }
        if (rarityText != null)
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
            rarityText.text = rarityName;
        }

        // Populate the new modern rounded stats badges with clear labels
        if (badgeStageTmp != null)
        {
            badgeStageTmp.text = $"<color=#1E222B><b>Stage: {cardData.stage}</b></color>";
        }

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

        if (badgeWeakTmp != null)
        {
            badgeWeakTmp.text = $"<color=#1E222B><b>Weak: {cardData.weakness.ToString()} {cardData.weaknessValue}</b></color>";
        }

        if (badgeResistTmp != null)
        {
            if (cardData.hasResistance)
            {
                badgeResistTmp.text = $"<color=#1E222B><b>Resist: {cardData.resistance.ToString()} {cardData.resistanceValue}</b></color>";
            }
            else
            {
                badgeResistTmp.text = $"<color=#1E222B><b>Resist: None</b></color>";
            }
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

        // 3. Align the World Canvas to Card Front
        AlignCanvas();
    }

    private string FormatCostText(string rawCost)
    {
        if (string.IsNullOrEmpty(rawCost)) return "";
        
        List<string> formattedParts = new List<string>();
        string[] tokens = rawCost.Split(' ');
        foreach (var token in tokens)
        {
            string t = token.Trim().ToUpper();
            if (t.StartsWith("[") && t.EndsWith("]"))
            {
                t = t.Substring(1, t.Length - 2);
            }
            switch (t)
            {
                case "C":
                case "COLORLESS":
                    break; // Omit colorless entirely
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
                    if (!string.IsNullOrEmpty(token))
                    {
                        formattedParts.Add($"<b>[{token}]</b>");
                    }
                    break;
            }
        }
        return string.Join(" ", formattedParts);
    }

    private void ApplyMaterials()
    {
        if (meshRenderer == null) return;

        // Find the correct texture for the card type (override with custom background or default front texture if assigned)
        Texture2D frontTex = null;
        if (cardData.customBackgroundSprite != null)
        {
            frontTex = cardData.customBackgroundSprite.texture;
        }
        else if (defaultFrontTexture != null)
        {
            frontTex = defaultFrontTexture;
        }
        else
        {
            foreach (var mapping in typeBackgrounds)
            {
                if (mapping.type == cardData.cardType)
                {
                    frontTex = mapping.backgroundTexture;
                    break;
                }
            }
        }

        // Setup instanced materials to prevent overriding scene asset materials
        if (instancedFrontMaterial == null && baseFrontMaterial != null)
        {
            instancedFrontMaterial = new Material(baseFrontMaterial);
        }
        if (instancedBackMaterial == null && baseBackMaterial != null)
        {
            instancedBackMaterial = new Material(baseBackMaterial);
        }
        if (instancedEdgeMaterial == null && baseEdgeMaterial != null)
        {
            instancedEdgeMaterial = new Material(baseEdgeMaterial);
        }

        // Apply Textures to instances
        if (instancedFrontMaterial != null && frontTex != null)
        {
            instancedFrontMaterial.mainTexture = frontTex;
        }
        if (instancedBackMaterial != null && cardBackTexture != null)
        {
            instancedBackMaterial.mainTexture = cardBackTexture;
        }

        // Put them on the MeshRenderer
        List<Material> mats = new List<Material>();
        mats.Add(instancedFrontMaterial != null ? instancedFrontMaterial : baseFrontMaterial); // Index 0: Front face (+Z)
        mats.Add(instancedBackMaterial != null ? instancedBackMaterial : baseBackMaterial);   // Index 1: Back face (-Z)
        mats.Add(instancedEdgeMaterial != null ? instancedEdgeMaterial : baseEdgeMaterial);   // Index 2: Edge

        meshRenderer.sharedMaterials = mats.ToArray();
    }

    private void AlignCanvas()
    {
        if (worldSpaceCanvas == null || canvasRect == null) return;

        // Determine direction and orientation based on canvasSide selection
        float sign = (canvasSide == CardCanvasSide.FrontSide) ? -1.0f : 1.0f;
        float zOffset = (cardThickness * 0.5f * sign) + (0.0015f * sign);
        canvasRect.localPosition = new Vector3(0f, 0f, zOffset);
        
        float yRotation = (canvasSide == CardCanvasSide.FrontSide) ? 0f : 180f;
        canvasRect.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        // Adjust Canvas scale according to card width/height and RectTransform pixel width/height (assumes 700x980 pixel resolution)
        float pixelsW = canvasRect.rect.width;
        float pixelsH = canvasRect.rect.height;

        if (pixelsW > 0 && pixelsH > 0)
        {
            float scaleX = cardWidth / pixelsW;
            float scaleY = cardHeight / pixelsH;
            canvasRect.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }

    public void SetCardData(PokemonCardData data)
    {
        cardData = data;
        UpdateCardUI();
    }

    private void OnDestroy()
    {
        if (instancedFrontMaterial != null) DestroyImmediate(instancedFrontMaterial);
        if (instancedBackMaterial != null) DestroyImmediate(instancedBackMaterial);
        if (instancedEdgeMaterial != null) DestroyImmediate(instancedEdgeMaterial);
    }
}
