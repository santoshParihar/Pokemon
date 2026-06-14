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

        if (pokemonImage != null)
        {
            pokemonImage.sprite = cardData.pokemonSprite;
            pokemonImage.enabled = cardData.pokemonSprite != null;
        }

        // Attack 1
        if (attack1Name != null) attack1Name.text = cardData.attack1Name;
        if (attack1Cost != null) attack1Cost.text = cardData.attack1CostText;
        if (attack1Damage != null) attack1Damage.text = cardData.attack1Damage > 0 ? cardData.attack1Damage.ToString() : "";
        if (attack1Description != null) attack1Description.text = cardData.attack1Description;

        // Attack 2
        if (attack2Name != null) attack2Name.text = cardData.attack2Name;
        if (attack2Cost != null) attack2Cost.text = cardData.attack2CostText;
        if (attack2Damage != null) attack2Damage.text = cardData.attack2Damage > 0 ? cardData.attack2Damage.ToString() : "";
        if (attack2Description != null) attack2Description.text = cardData.attack2Description;

        // Weakness / Resistance
        if (weaknessText != null) weaknessText.text = $"x2 {cardData.weakness.ToString()}";
        if (resistanceText != null) resistanceText.text = $"-30 {cardData.resistance.ToString()}";

        // Retreat & Rarity Stars via ASCII star symbol '*'
        if (retreatText != null)
        {
            int cost = Mathf.Clamp(cardData.retreatCost, 0, 5);
            retreatText.text = new string('*', cost);
        }
        if (rarityText != null)
        {
            int stars = Mathf.Clamp(cardData.rarityStars, 1, 5);
            rarityText.text = new string('*', stars);
        }

        // 3. Align the World Canvas to Card Front
        AlignCanvas();
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
