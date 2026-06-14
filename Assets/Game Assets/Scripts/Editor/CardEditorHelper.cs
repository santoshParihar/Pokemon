#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using TMPro;

public enum CardDefaultFacing
{
    BackSide,
    FrontSide
}

public class CardEditorHelper : EditorWindow
{
    // Default parameters
    private float cardWidth = 1.0f;
    private float cardHeight = 1.4f;
    private float cardThickness = 0.02f;
    private float cornerRadius = 0.08f;
    private int cornerSegments = 8;
    private CardDefaultFacing defaultFacing = CardDefaultFacing.BackSide;
    private CardCanvasSide canvasSide = CardCanvasSide.FrontSide;
    private Texture2D customBackTexture = null;

    [MenuItem("Pokemon TCG/Card Creator Window")]
    public static void ShowWindow()
    {
        GetWindow<CardEditorHelper>("Card Creator");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("3D Card Customizer", EditorStyles.boldLabel);
        
        GUILayout.BeginVertical("box");
        cardWidth = EditorGUILayout.FloatField("Width", cardWidth);
        cardHeight = EditorGUILayout.FloatField("Height", cardHeight);
        cardThickness = EditorGUILayout.FloatField("Thickness", cardThickness);
        GUILayout.EndVertical();

        GUILayout.Space(5);
        GUILayout.Label("Rounded Corner Settings", EditorStyles.boldLabel);
        
        GUILayout.BeginVertical("box");
        // Limit the corner radius to at most half of the card's narrowest dimension
        float maxRadius = Mathf.Min(cardWidth, cardHeight) * 0.5f;
        cornerRadius = EditorGUILayout.Slider("Corner Radius", cornerRadius, 0.0f, maxRadius);
        cornerSegments = EditorGUILayout.IntSlider("Corner Segments", cornerSegments, 2, 32);
        GUILayout.EndVertical();

        GUILayout.Space(5);
        GUILayout.Label("Default Rotation Settings", EditorStyles.boldLabel);
        GUILayout.BeginVertical("box");
        defaultFacing = (CardDefaultFacing)EditorGUILayout.EnumPopup("Spawn Facing", defaultFacing);
        GUILayout.EndVertical();

        GUILayout.Space(5);
        GUILayout.Label("Canvas Placement Settings", EditorStyles.boldLabel);
        GUILayout.BeginVertical("box");
        canvasSide = (CardCanvasSide)EditorGUILayout.EnumPopup("Canvas Side", canvasSide);
        GUILayout.EndVertical();

        GUILayout.Space(5);
        GUILayout.Label("Custom Material Settings", EditorStyles.boldLabel);
        GUILayout.BeginVertical("box");
        customBackTexture = (Texture2D)EditorGUILayout.ObjectField("Back Texture (Override)", customBackTexture, typeof(Texture2D), false);
        GUILayout.EndVertical();

        GUILayout.Space(15);
        
        GUI.backgroundColor = new Color(0.35f, 0.75f, 0.35f);
        if (GUILayout.Button("Bake Card & Generate Prefab", GUILayout.Height(40)))
        {
            CreatePokemonCardPrefab(cardWidth, cardHeight, cardThickness, cornerRadius, cornerSegments, defaultFacing, canvasSide, customBackTexture);
        }
        GUI.backgroundColor = Color.white;
    }

    public static void CreatePokemonCardPrefab(float width, float height, float thickness, float cornerRadius, int cornerSegments, CardDefaultFacing defaultFacing, CardCanvasSide canvasSide, Texture2D customBackTexture)
    {
        // 1. Setup/Find Shaders (Unlit for card faces, Lit for card edges)
        Shader faceShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (faceShader == null) faceShader = Shader.Find("Unlit/Texture");
        if (faceShader == null) faceShader = Shader.Find("Standard");

        Shader edgeShader = Shader.Find("Universal Render Pipeline/Lit");
        if (edgeShader == null) edgeShader = Shader.Find("Standard");

        // Create folders if they do not exist
        string matFolder = "Assets/Game Assets/Materials";
        if (!AssetDatabase.IsValidFolder(matFolder))
        {
            Directory.CreateDirectory(matFolder);
            AssetDatabase.Refresh();
        }

        string dataFolder = "Assets/Game Assets/Data";
        if (!AssetDatabase.IsValidFolder(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
            AssetDatabase.Refresh();
        }

        string prefabsFolder = "Assets/Game Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabsFolder))
        {
            Directory.CreateDirectory(prefabsFolder);
            AssetDatabase.Refresh();
        }

        string meshFolder = "Assets/Game Assets/Meshes";
        if (!AssetDatabase.IsValidFolder(meshFolder))
        {
            Directory.CreateDirectory(meshFolder);
            AssetDatabase.Refresh();
        }

        // Setup Materials
        Material frontMat = AssetDatabase.LoadAssetAtPath<Material>($"{matFolder}/CardFront.mat");
        if (frontMat == null)
        {
            frontMat = new Material(faceShader);
            AssetDatabase.CreateAsset(frontMat, $"{matFolder}/CardFront.mat");
        }
        else
        {
            frontMat.shader = faceShader;
        }

        Material backMat = AssetDatabase.LoadAssetAtPath<Material>($"{matFolder}/CardBack.mat");
        if (backMat == null)
        {
            backMat = new Material(faceShader);
            Texture2D cardBackTex = customBackTexture != null ? customBackTexture : AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game Assets/Textures/card-back.png");
            if (cardBackTex != null)
            {
                backMat.mainTexture = cardBackTex;
            }
            AssetDatabase.CreateAsset(backMat, $"{matFolder}/CardBack.mat");
        }
        else
        {
            backMat.shader = faceShader;
            if (customBackTexture != null)
            {
                backMat.mainTexture = customBackTexture;
            }
        }

        Material edgeMat = AssetDatabase.LoadAssetAtPath<Material>($"{matFolder}/CardEdge.mat");
        if (edgeMat == null)
        {
            edgeMat = new Material(edgeShader);
            edgeMat.color = new Color(0.9f, 0.85f, 0.75f); // Soft paper/gold-ish edge
            AssetDatabase.CreateAsset(edgeMat, $"{matFolder}/CardEdge.mat");
        }
        else
        {
            edgeMat.shader = edgeShader;
        }

        // 2. Create Sample Pokemon Card Data assets
        PokemonCardData charmanderData = AssetDatabase.LoadAssetAtPath<PokemonCardData>($"{dataFolder}/CharmanderData.asset");
        if (charmanderData == null)
        {
            charmanderData = ScriptableObject.CreateInstance<PokemonCardData>();
            charmanderData.pokemonName = "Charmander";
            charmanderData.hp = 60;
            charmanderData.cardType = PokemonType.Fire;
            charmanderData.pokemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game Assets/Textures/pokemon.png");
            
            charmanderData.attack1Name = "Scratch";
            charmanderData.attack1CostText = "C";
            charmanderData.attack1Damage = 10;
            charmanderData.attack1Description = "Hard fingernails scratch the opponent.";
            
            charmanderData.attack2Name = "Ember";
            charmanderData.attack2CostText = "F C";
            charmanderData.attack2Damage = 30;
            charmanderData.attack2Description = "Discard 1 Fire Energy attached to this Pokemon.";
            
            charmanderData.weakness = PokemonType.Water;
            charmanderData.resistance = PokemonType.Normal;
            charmanderData.retreatCost = 1;
            charmanderData.rarityStars = 1;
            
            AssetDatabase.CreateAsset(charmanderData, $"{dataFolder}/CharmanderData.asset");
        }

        PokemonCardData bulbasaurData = AssetDatabase.LoadAssetAtPath<PokemonCardData>($"{dataFolder}/BulbasaurData.asset");
        if (bulbasaurData == null)
        {
            bulbasaurData = ScriptableObject.CreateInstance<PokemonCardData>();
            bulbasaurData.pokemonName = "Bulbasaur";
            bulbasaurData.hp = 70;
            bulbasaurData.cardType = PokemonType.Grass;
            bulbasaurData.pokemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game Assets/Textures/pokemon.png");
            
            bulbasaurData.attack1Name = "Tackle";
            bulbasaurData.attack1CostText = "C";
            bulbasaurData.attack1Damage = 10;
            bulbasaurData.attack1Description = "A simple tackle attack.";
            
            bulbasaurData.attack2Name = "Vine Whip";
            bulbasaurData.attack2CostText = "G C";
            bulbasaurData.attack2Damage = 30;
            bulbasaurData.attack2Description = "Whips the opponent with thin vines.";
            
            bulbasaurData.weakness = PokemonType.Fire;
            bulbasaurData.resistance = PokemonType.Water;
            bulbasaurData.retreatCost = 1;
            bulbasaurData.rarityStars = 2;
            
            AssetDatabase.CreateAsset(bulbasaurData, $"{dataFolder}/BulbasaurData.asset");
        }

        PokemonCardData squirtleData = AssetDatabase.LoadAssetAtPath<PokemonCardData>($"{dataFolder}/SquirtleData.asset");
        if (squirtleData == null)
        {
            squirtleData = ScriptableObject.CreateInstance<PokemonCardData>();
            squirtleData.pokemonName = "Squirtle";
            squirtleData.hp = 60;
            squirtleData.cardType = PokemonType.Water;
            squirtleData.pokemonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game Assets/Textures/pokemon.png");
            
            squirtleData.attack1Name = "Bubble";
            squirtleData.attack1CostText = "W";
            squirtleData.attack1Damage = 10;
            squirtleData.attack1Description = "Flip a coin. If heads, the Defending Pokemon is now Paralyzed.";
            
            squirtleData.attack2Name = "Water Gun";
            squirtleData.attack2CostText = "W C";
            squirtleData.attack2Damage = 30;
            squirtleData.attack2Description = "Shoots a burst of high pressure water.";
            
            squirtleData.weakness = PokemonType.Lightning;
            squirtleData.resistance = PokemonType.Normal;
            squirtleData.retreatCost = 1;
            squirtleData.rarityStars = 1;
            
            AssetDatabase.CreateAsset(squirtleData, $"{dataFolder}/SquirtleData.asset");
        }

        AssetDatabase.SaveAssets();

        // 3. Generate Card Mesh
        Mesh tempMesh = GenerateMesh(width, height, thickness, cornerRadius, cornerSegments);

        // 4. Bake and Save the Mesh Asset Locally
        string meshAssetPath = $"{meshFolder}/CardMesh.asset";
        Mesh savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);
        
        if (savedMesh == null)
        {
            savedMesh = Instantiate(tempMesh);
            savedMesh.name = "CardMesh";
            savedMesh.hideFlags = HideFlags.None;
            AssetDatabase.CreateAsset(savedMesh, meshAssetPath);
        }
        else
        {
            // Overwrite geometry in existing mesh to preserve scene and prefab links
            savedMesh.Clear();
            savedMesh.vertices = tempMesh.vertices;
            savedMesh.normals = tempMesh.normals;
            savedMesh.uv = tempMesh.uv;
            savedMesh.subMeshCount = tempMesh.subMeshCount;
            for (int i = 0; i < tempMesh.subMeshCount; i++)
            {
                savedMesh.SetTriangles(tempMesh.GetTriangles(i), i);
            }
            savedMesh.RecalculateBounds();
            savedMesh.RecalculateTangents();
            EditorUtility.SetDirty(savedMesh);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Destroy temp mesh
        DestroyImmediate(tempMesh);

        // 5. Create the Main Card GameObject
        GameObject cardObj = new GameObject("3D Pokemon Card");
        
        // Attach Components
        MeshFilter meshFilter = cardObj.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);

        MeshRenderer renderer = cardObj.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = new Material[] { frontMat, backMat, edgeMat };

        CardUIController uiController = cardObj.AddComponent<CardUIController>();
        cardObj.AddComponent<CardRotator>();

        // Set default rotation dynamically based on the chosen dropdown option
        if (defaultFacing == CardDefaultFacing.BackSide)
        {
            cardObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            cardObj.transform.rotation = Quaternion.identity;
        }

        // Set dimensions on UI Controller so it can align the Canvas
        var cardWidthField = typeof(CardUIController).GetField("cardWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cardHeightField = typeof(CardUIController).GetField("cardHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cardThicknessField = typeof(CardUIController).GetField("cardThickness", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        cardWidthField?.SetValue(uiController, width);
        cardHeightField?.SetValue(uiController, height);
        cardThicknessField?.SetValue(uiController, thickness);

        var canvasSideField = typeof(CardUIController).GetField("canvasSide", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (canvasSideField != null) canvasSideField.SetValue(uiController, canvasSide);

        // Set Materials on UI Controller
        var baseFrontField = typeof(CardUIController).GetField("baseFrontMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var baseBackField = typeof(CardUIController).GetField("baseBackMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var baseEdgeField = typeof(CardUIController).GetField("baseEdgeMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (baseFrontField != null) baseFrontField.SetValue(uiController, frontMat);
        if (baseBackField != null) baseBackField.SetValue(uiController, backMat);
        if (baseEdgeField != null) baseEdgeField.SetValue(uiController, edgeMat);

        // Map textures automatically
        var typeBgsField = typeof(CardUIController).GetField("typeBackgrounds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var backTexField = typeof(CardUIController).GetField("cardBackTexture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (backTexField != null)
        {
            Texture2D cardBackTex = null;
            if (customBackTexture != null)
            {
                cardBackTex = customBackTexture;
            }
            else
            {
                OptimizeTextureSettings("Assets/Game Assets/Textures/card-back.png");
                cardBackTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game Assets/Textures/card-back.png");
            }
            backTexField.SetValue(uiController, cardBackTex);
        }

        if (typeBgsField != null)
        {
            var mappingList = new List<CardUIController.TypeTextureMapping>();
            string[] textureNames = new string[] { "Grass", "Fire", "Water", "Lightning", "Psychic", "Fighting", "Darkness", "Metal", "Dragon", "Normal" };
            PokemonType[] types = new PokemonType[]
            {
                PokemonType.Grass, PokemonType.Fire, PokemonType.Water, PokemonType.Lightning, PokemonType.Psychic,
                PokemonType.Fighting, PokemonType.Darkness, PokemonType.Metal, PokemonType.Dragon, PokemonType.Normal
            };

            for (int i = 0; i < types.Length; i++)
            {
                string texPath = $"Assets/Game Assets/Textures/Property 1={textureNames[i]}.png";
                OptimizeTextureSettings(texPath);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (tex != null)
                {
                    mappingList.Add(new CardUIController.TypeTextureMapping { type = types[i], backgroundTexture = tex });
                }
            }
            typeBgsField.SetValue(uiController, mappingList);
        }

        // 6. Create World Space Canvas Hierarchy
        GameObject canvasObj = new GameObject("CardCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObj.transform.SetParent(cardObj.transform, false);

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(700, 980);
        
        float sign = (canvasSide == CardCanvasSide.FrontSide) ? -1.0f : 1.0f;
        float zOffset = (thickness * 0.5f * sign) + (0.0015f * sign);
        canvasRect.localPosition = new Vector3(0, 0, zOffset);
        
        float yRotation = (canvasSide == CardCanvasSide.FrontSide) ? 0f : 180f;
        canvasRect.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        canvasRect.localScale = new Vector3(width / 700.0f, height / 980.0f, 1f);

        // Assign Canvas Fields on UI Controller
        var canvasField = typeof(CardUIController).GetField("worldSpaceCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var canvasRectField = typeof(CardUIController).GetField("canvasRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (canvasField != null) canvasField.SetValue(uiController, canvas);
        if (canvasRectField != null) canvasRectField.SetValue(uiController, canvasRect);

        // 7. Create Canvas UI Elements
        // Header: Name and HP
        GameObject headerObj = new GameObject("HeaderPanel", typeof(RectTransform));
        headerObj.transform.SetParent(canvasObj.transform, false);
        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 0.9f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.offsetMin = new Vector2(40, 0);
        headerRect.offsetMax = new Vector2(-40, -30);

        TextMeshProUGUI nameTMP = CreateTextElement(headerObj, "NameText", "Pokemon Name", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(125f, 0f), new Vector2(250, 60), 38, TextAlignmentOptions.Left);
        TextMeshProUGUI hpTMP = CreateTextElement(headerObj, "HPText", "120 HP", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-125f, 0f), new Vector2(250, 60), 38, TextAlignmentOptions.Right);

        // Type info (under name or in background)
        TextMeshProUGUI typeTMP = CreateTextElement(headerObj, "TypeText", "Grass", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, -20), new Vector2(100, 30), 18, TextAlignmentOptions.Center);
        typeTMP.color = Color.gray;

        // Sprite Image Panel
        GameObject artObj = new GameObject("ArtworkPanel", typeof(RectTransform), typeof(Image));
        artObj.transform.SetParent(canvasObj.transform, false);
        RectTransform artRect = artObj.GetComponent<RectTransform>();
        artRect.anchorMin = new Vector2(0.5f, 0.5f);
        artRect.anchorMax = new Vector2(0.5f, 0.5f);
        artRect.anchoredPosition = new Vector3(0, 150, 0);
        artRect.sizeDelta = new Vector2(580, 380);
        
        Image artImg = artObj.GetComponent<Image>();
        artImg.color = new Color(0.9f, 0.9f, 0.9f, 0.6f);
        Sprite pokemonSprite = GetOrConvertSprite("Assets/Game Assets/Textures/pokemon.png");
        if (pokemonSprite != null)
        {
            artImg.sprite = pokemonSprite;
            artImg.color = Color.white;
        }

        // Attacks Container
        GameObject attacksPanelObj = new GameObject("AttacksPanel", typeof(RectTransform));
        attacksPanelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform attacksRect = attacksPanelObj.GetComponent<RectTransform>();
        attacksRect.anchorMin = new Vector2(0f, 0.15f);
        attacksRect.anchorMax = new Vector2(1f, 0.45f);
        attacksRect.offsetMin = new Vector2(40, 0);
        attacksRect.offsetMax = new Vector2(-40, 0);

        // Attack 1 elements
        GameObject atk1Obj = new GameObject("Attack1", typeof(RectTransform));
        atk1Obj.transform.SetParent(attacksPanelObj.transform, false);
        RectTransform atk1Rect = atk1Obj.GetComponent<RectTransform>();
        atk1Rect.anchorMin = new Vector2(0, 0.5f);
        atk1Rect.anchorMax = new Vector2(1, 1);
        atk1Rect.offsetMin = Vector2.zero;
        atk1Rect.offsetMax = Vector2.zero;

        TextMeshProUGUI atk1CostTMP = CreateTextElement(atk1Obj, "Atk1Cost", "G C", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(50, 20), new Vector2(100, 40), 24, TextAlignmentOptions.Left);
        TextMeshProUGUI atk1NameTMP = CreateTextElement(atk1Obj, "Atk1Name", "Attack One", new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(150, 20), new Vector2(250, 40), 28, TextAlignmentOptions.Left);
        TextMeshProUGUI atk1DmgTMP = CreateTextElement(atk1Obj, "Atk1Damage", "30", new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-50, 20), new Vector2(100, 40), 28, TextAlignmentOptions.Right);
        TextMeshProUGUI atk1DescTMP = CreateTextElement(atk1Obj, "Atk1Description", "Deals 30 damage.", new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(50, -10), new Vector2(-100, 30), 18, TextAlignmentOptions.Left);

        // Attack 2 elements
        GameObject atk2Obj = new GameObject("Attack2", typeof(RectTransform));
        atk2Obj.transform.SetParent(attacksPanelObj.transform, false);
        RectTransform atk2Rect = atk2Obj.GetComponent<RectTransform>();
        atk2Rect.anchorMin = new Vector2(0, 0);
        atk2Rect.anchorMax = new Vector2(1, 0.5f);
        atk2Rect.offsetMin = Vector2.zero;
        atk2Rect.offsetMax = Vector2.zero;

        TextMeshProUGUI atk2CostTMP = CreateTextElement(atk2Obj, "Atk2Cost", "G G C", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(50, 20), new Vector2(100, 40), 24, TextAlignmentOptions.Left);
        TextMeshProUGUI atk2NameTMP = CreateTextElement(atk2Obj, "Atk2Name", "Attack Two", new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(150, 20), new Vector2(250, 40), 28, TextAlignmentOptions.Left);
        TextMeshProUGUI atk2DmgTMP = CreateTextElement(atk2Obj, "Atk2Damage", "70", new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-50, 20), new Vector2(100, 40), 28, TextAlignmentOptions.Right);
        TextMeshProUGUI atk2DescTMP = CreateTextElement(atk2Obj, "Atk2Description", "Discard an energy card.", new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(50, -10), new Vector2(-100, 30), 18, TextAlignmentOptions.Left);

        // Footer: Weakness, Resistance, Retreat, Rarity
        GameObject footerObj = new GameObject("FooterPanel", typeof(RectTransform));
        footerObj.transform.SetParent(canvasObj.transform, false);
        RectTransform footerRect = footerObj.GetComponent<RectTransform>();
        footerRect.anchorMin = new Vector2(0f, 0f);
        footerRect.anchorMax = new Vector2(1f, 0.15f);
        footerRect.offsetMin = new Vector2(40, 20);
        footerRect.offsetMax = new Vector2(-40, 0);

        // Weakness Section
        CreateTextElement(footerObj, "WeaknessLabel", "Weakness", new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(50, 30), new Vector2(100, 25), 16, TextAlignmentOptions.Left);
        TextMeshProUGUI weakValTMP = CreateTextElement(footerObj, "WeaknessText", "x2 Fire", new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(50, 5), new Vector2(100, 25), 18, TextAlignmentOptions.Left);

        // Resistance Section
        CreateTextElement(footerObj, "ResistanceLabel", "Resistance", new Vector2(0.33f, 0.5f), new Vector2(0.33f, 0.5f), new Vector2(50, 30), new Vector2(100, 25), 16, TextAlignmentOptions.Center);
        TextMeshProUGUI resistValTMP = CreateTextElement(footerObj, "ResistanceText", "-30 Water", new Vector2(0.33f, 0.5f), new Vector2(0.33f, 0.5f), new Vector2(50, 5), new Vector2(100, 25), 18, TextAlignmentOptions.Center);

        // Retreat Cost Section (ASCII * based layout)
        CreateTextElement(footerObj, "RetreatLabel", "Retreat", new Vector2(0.66f, 0.5f), new Vector2(0.66f, 0.5f), new Vector2(50, 30), new Vector2(100, 25), 16, TextAlignmentOptions.Center);
        TextMeshProUGUI retreatValTMP = CreateTextElement(footerObj, "RetreatText", "*", new Vector2(0.66f, 0.5f), new Vector2(0.66f, 0.5f), new Vector2(50, 5), new Vector2(100, 25), 18, TextAlignmentOptions.Center);

        // Rarity Stars Section (ASCII * based layout)
        TextMeshProUGUI rarityValTMP = CreateTextElement(footerObj, "RarityText", "*", new Vector2(1.0f, 0.5f), new Vector2(1.0f, 0.5f), new Vector2(-40, 5), new Vector2(100, 25), 18, TextAlignmentOptions.Right);

        // 8. Bind UI components to UI Controller
        var nameTextField = typeof(CardUIController).GetField("nameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hpTextField = typeof(CardUIController).GetField("hpText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var typeTextField = typeof(CardUIController).GetField("typeText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pokemonImageField = typeof(CardUIController).GetField("pokemonImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var attack1NameField = typeof(CardUIController).GetField("attack1Name", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack1CostField = typeof(CardUIController).GetField("attack1Cost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack1DamageField = typeof(CardUIController).GetField("attack1Damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack1DescField = typeof(CardUIController).GetField("attack1Description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var attack2NameField = typeof(CardUIController).GetField("attack2Name", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack2CostField = typeof(CardUIController).GetField("attack2Cost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack2DamageField = typeof(CardUIController).GetField("attack2Damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack2DescField = typeof(CardUIController).GetField("attack2Description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var weaknessTextField = typeof(CardUIController).GetField("weaknessText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var resistanceTextField = typeof(CardUIController).GetField("resistanceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var retreatTextField = typeof(CardUIController).GetField("retreatText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rarityTextField = typeof(CardUIController).GetField("rarityText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        nameTextField?.SetValue(uiController, nameTMP);
        hpTextField?.SetValue(uiController, hpTMP);
        typeTextField?.SetValue(uiController, typeTMP);
        pokemonImageField?.SetValue(uiController, artImg);

        attack1NameField?.SetValue(uiController, atk1NameTMP);
        attack1CostField?.SetValue(uiController, atk1CostTMP);
        attack1DamageField?.SetValue(uiController, atk1DmgTMP);
        attack1DescField?.SetValue(uiController, atk1DescTMP);

        attack2NameField?.SetValue(uiController, atk2NameTMP);
        attack2CostField?.SetValue(uiController, atk2CostTMP);
        attack2DamageField?.SetValue(uiController, atk2DmgTMP);
        attack2DescField?.SetValue(uiController, atk2DescTMP);

        weaknessTextField?.SetValue(uiController, weakValTMP);
        resistanceTextField?.SetValue(uiController, resistValTMP);

        retreatTextField?.SetValue(uiController, retreatValTMP);
        rarityTextField?.SetValue(uiController, rarityValTMP);

        // Bind default Card Data (Charmander)
        uiController.SetCardData(charmanderData);

        // 9. Save Card Prefab
        string cardPrefabPath = $"{prefabsFolder}/PokemonCard.prefab";
        GameObject cardPrefab = PrefabUtility.SaveAsPrefabAsset(cardObj, cardPrefabPath);
        DestroyImmediate(cardObj);

        // Destroy existing instances in the active scene to avoid duplicates/stale objects
        GameObject[] existingCards = GameObject.FindObjectsOfType<GameObject>();
        foreach (var card in existingCards)
        {
            if (card.name == "3D Pokemon Card" && card.scene.IsValid())
            {
                Undo.DestroyObjectImmediate(card);
            }
        }

        // Instantiate in active scene for user
        GameObject sceneInstance = PrefabUtility.InstantiatePrefab(cardPrefab) as GameObject;
        Selection.activeGameObject = sceneInstance;

        Debug.Log($"Successfully created Card materials, Charmander/Bulbasaur/Squirtle Data assets, local Baked Mesh asset, and complete 3D Pokemon Card Prefab at: {cardPrefabPath}. Spawned instance in scene!");
    }

    private static Mesh GenerateMesh(float width, float height, float thickness, float cornerRadius, int cornerSegments)
    {
        Mesh mesh = new Mesh();
        mesh.name = "CardMesh";

        float halfW = width * 0.5f;
        float halfH = height * 0.5f;
        float clampedRadius = Mathf.Clamp(cornerRadius, 0f, Mathf.Min(halfW, halfH));

        List<Vector2> points2D = new List<Vector2>();

        if (clampedRadius <= 0f)
        {
            points2D.Add(new Vector2(halfW, halfH));
            points2D.Add(new Vector2(-halfW, halfH));
            points2D.Add(new Vector2(-halfW, -halfH));
            points2D.Add(new Vector2(halfW, -halfH));
        }
        else
        {
            Vector2[] centers = new Vector2[]
            {
                new Vector2(halfW - clampedRadius, halfH - clampedRadius),
                new Vector2(-halfW + clampedRadius, halfH - clampedRadius),
                new Vector2(-halfW + clampedRadius, -halfH + clampedRadius),
                new Vector2(halfW - clampedRadius, -halfH + clampedRadius)
            };

            float[] startAngles = new float[]
            {
                0f,
                Mathf.PI * 0.5f,
                Mathf.PI,
                Mathf.PI * 1.5f
            };

            for (int c = 0; c < 4; c++)
            {
                for (int i = 0; i <= cornerSegments; i++)
                {
                    float angle = startAngles[c] + (i / (float)cornerSegments) * (Mathf.PI * 0.5f);
                    float x = centers[c].x + clampedRadius * Mathf.Cos(angle);
                    float y = centers[c].y + clampedRadius * Mathf.Sin(angle);
                    points2D.Add(new Vector2(x, y));
                }
            }
        }

        int pCount = points2D.Count;
        int vCount = (pCount + 1) + (pCount + 1) + (2 * pCount);
        Vector3[] vertices = new Vector3[vCount];
        Vector3[] normals = new Vector3[vCount];
        Vector2[] uvs = new Vector2[vCount];

        float halfThickness = thickness * 0.5f;

        // Front Face
        int frontCenterIdx = 0;
        vertices[frontCenterIdx] = new Vector3(0f, 0f, halfThickness);
        normals[frontCenterIdx] = Vector3.forward;
        uvs[frontCenterIdx] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < pCount; i++)
        {
            int idx = 1 + i;
            float x = points2D[i].x;
            float y = points2D[i].y;
            vertices[idx] = new Vector3(x, y, halfThickness);
            normals[idx] = Vector3.forward;
            uvs[idx] = new Vector2(x / width + 0.5f, y / height + 0.5f);
        }

        // Back Face
        int backCenterIdx = pCount + 1;
        vertices[backCenterIdx] = new Vector3(0f, 0f, -halfThickness);
        normals[backCenterIdx] = Vector3.back;
        uvs[backCenterIdx] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < pCount; i++)
        {
            int idx = pCount + 2 + i;
            float x = points2D[i].x;
            float y = points2D[i].y;
            vertices[idx] = new Vector3(x, y, -halfThickness);
            normals[idx] = Vector3.back;
            uvs[idx] = new Vector2(0.5f - x / width, y / height + 0.5f);
        }

        // Edge Face
        int edgeStartIdx = 2 * pCount + 2;
        for (int i = 0; i < pCount; i++)
        {
            int nextI = (i + 1) % pCount;
            Vector2 pCurrent = points2D[i];
            Vector2 pNext = points2D[nextI];
            Vector2 pPrev = points2D[(i - 1 + pCount) % pCount];

            Vector2 tangent = (pNext - pPrev).normalized;
            Vector2 normal2D = new Vector2(tangent.y, -tangent.x);
            Vector3 outwardNormal = new Vector3(normal2D.x, normal2D.y, 0f).normalized;

            int fIdx = edgeStartIdx + 2 * i;
            vertices[fIdx] = new Vector3(pCurrent.x, pCurrent.y, halfThickness);
            normals[fIdx] = outwardNormal;
            uvs[fIdx] = new Vector2((float)i / pCount, 1.0f);

            int bIdx = fIdx + 1;
            vertices[bIdx] = new Vector3(pCurrent.x, pCurrent.y, -halfThickness);
            normals[bIdx] = outwardNormal;
            uvs[bIdx] = new Vector2((float)i / pCount, 0.0f);
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;

        int[] frontTriangles = new int[pCount * 3];
        for (int i = 0; i < pCount; i++)
        {
            frontTriangles[3 * i] = frontCenterIdx;
            frontTriangles[3 * i + 1] = 1 + (i + 1) % pCount;
            frontTriangles[3 * i + 2] = 1 + i;
        }

        int[] backTriangles = new int[pCount * 3];
        for (int i = 0; i < pCount; i++)
        {
            backTriangles[3 * i] = backCenterIdx;
            backTriangles[3 * i + 1] = pCount + 2 + i;
            backTriangles[3 * i + 2] = pCount + 2 + (i + 1) % pCount;
        }

        int[] edgeTriangles = new int[pCount * 6];
        for (int i = 0; i < pCount; i++)
        {
            int currentFront = edgeStartIdx + 2 * i;
            int currentBack = currentFront + 1;
            int nextFront = edgeStartIdx + 2 * ((i + 1) % pCount);
            int nextBack = nextFront + 1;

            edgeTriangles[6 * i] = currentBack;
            edgeTriangles[6 * i + 1] = nextBack;
            edgeTriangles[6 * i + 2] = nextFront;

            edgeTriangles[6 * i + 3] = currentBack;
            edgeTriangles[6 * i + 4] = nextFront;
            edgeTriangles[6 * i + 5] = currentFront;
        }

        mesh.subMeshCount = 3;
        mesh.SetTriangles(frontTriangles, 0);
        mesh.SetTriangles(backTriangles, 1);
        mesh.SetTriangles(edgeTriangles, 2);

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }

    private static Sprite GetOrConvertSprite(string path)
    {
        OptimizeTextureSettings(path);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }
        return sprite;
    }

    private static void OptimizeTextureSettings(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            bool modified = false;
            if (importer.filterMode != FilterMode.Trilinear)
            {
                importer.filterMode = FilterMode.Trilinear;
                modified = true;
            }
            if (importer.anisoLevel < 8)
            {
                importer.anisoLevel = 8;
                modified = true;
            }
            if (modified)
            {
                importer.SaveAndReimport();
            }
        }
    }

    private static TextMeshProUGUI CreateTextElement(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(parent.transform, false);
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.black;
        tmp.fontStyle = FontStyles.Bold;

        return tmp;
    }
}
#endif
