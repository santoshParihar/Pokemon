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
            charmanderData.stage = "Basic";
            charmanderData.pokedexNo = "#004";
            charmanderData.pokedexClass = "Lizard Pokémon";
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
        else
        {
            charmanderData.stage = "Basic";
            charmanderData.pokedexNo = "#004";
            charmanderData.pokedexClass = "Lizard Pokémon";
            EditorUtility.SetDirty(charmanderData);
        }

        PokemonCardData bulbasaurData = AssetDatabase.LoadAssetAtPath<PokemonCardData>($"{dataFolder}/BulbasaurData.asset");
        if (bulbasaurData == null)
        {
            bulbasaurData = ScriptableObject.CreateInstance<PokemonCardData>();
            bulbasaurData.pokemonName = "Bulbasaur";
            bulbasaurData.hp = 70;
            bulbasaurData.stage = "Basic";
            bulbasaurData.pokedexNo = "#001";
            bulbasaurData.pokedexClass = "Seed Pokémon";
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
        else
        {
            bulbasaurData.stage = "Basic";
            bulbasaurData.pokedexNo = "#001";
            bulbasaurData.pokedexClass = "Seed Pokémon";
            EditorUtility.SetDirty(bulbasaurData);
        }

        PokemonCardData squirtleData = AssetDatabase.LoadAssetAtPath<PokemonCardData>($"{dataFolder}/SquirtleData.asset");
        if (squirtleData == null)
        {
            squirtleData = ScriptableObject.CreateInstance<PokemonCardData>();
            squirtleData.pokemonName = "Squirtle";
            squirtleData.hp = 60;
            squirtleData.stage = "Basic";
            squirtleData.pokedexNo = "#007";
            squirtleData.pokedexClass = "Tiny Turtle Pokémon";
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
        else
        {
            squirtleData.stage = "Basic";
            squirtleData.pokedexNo = "#007";
            squirtleData.pokedexClass = "Tiny Turtle Pokémon";
            EditorUtility.SetDirty(squirtleData);
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
        headerRect.anchorMin = new Vector2(0f, 0.88f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.offsetMin = new Vector2(40, 0);
        headerRect.offsetMax = new Vector2(-40, -20);

        // Container for Name + Type Badge on the left
        GameObject nameContainerObj = new GameObject("NameAndTypeContainer", typeof(RectTransform));
        nameContainerObj.transform.SetParent(headerObj.transform, false);
        RectTransform nameContainerRect = nameContainerObj.GetComponent<RectTransform>();
        nameContainerRect.anchorMin = new Vector2(0f, 0.35f); // Updated from 0.4f to align with HP text Y anchors
        nameContainerRect.anchorMax = new Vector2(0.7f, 1f);
        nameContainerRect.offsetMin = Vector2.zero;
        nameContainerRect.offsetMax = Vector2.zero;

        // Add HorizontalLayoutGroup to NameAndTypeContainer
        HorizontalLayoutGroup hlGroup = nameContainerObj.AddComponent<HorizontalLayoutGroup>();
        hlGroup.spacing = 15;
        hlGroup.childAlignment = TextAnchor.MiddleLeft;
        hlGroup.childControlWidth = true;
        hlGroup.childControlHeight = false;
        hlGroup.childForceExpandWidth = false;
        hlGroup.childForceExpandHeight = false;

        // Name text inside the layout group
        GameObject nameTextObj = new GameObject("NameText", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameTextObj.transform.SetParent(nameContainerObj.transform, false);
        TextMeshProUGUI nameTMP = nameTextObj.GetComponent<TextMeshProUGUI>();
        nameTMP.text = "Pokemon Name";
        nameTMP.fontSize = 42; // Increased from 38
        nameTMP.alignment = TextAlignmentOptions.MidlineLeft; // Centered vertical alignment to match HP text
        nameTMP.color = new Color(0.12f, 0.14f, 0.18f, 1f); // #1E222B - premium dark slate
        nameTMP.fontStyle = FontStyles.Bold;
        // Add ContentSizeFitter so the Horizontal Layout Group can wrap it properly
        ContentSizeFitter sizeFitter = nameTextObj.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Type badge inside the layout group
        GameObject typeBadgeObj = new GameObject("TypeBadge", typeof(RectTransform), typeof(Image));
        typeBadgeObj.transform.SetParent(nameContainerObj.transform, false);
        RectTransform typeBadgeRect = typeBadgeObj.GetComponent<RectTransform>();

        Sprite badgeBgSprite = GetOrCreateBadgeSprite();
        Image typeBadgeImg = typeBadgeObj.GetComponent<Image>();
        typeBadgeImg.sprite = badgeBgSprite; // Use the rounded rect badge background!
        typeBadgeImg.type = Image.Type.Sliced;
        typeBadgeImg.color = new Color(0.18f, 0.22f, 0.29f, 1f); // #2D323E - dark slate pill background

        // Add HorizontalLayoutGroup to TypeBadge for internal padding
        HorizontalLayoutGroup badgeHL = typeBadgeObj.AddComponent<HorizontalLayoutGroup>();
        badgeHL.padding = new RectOffset(24, 24, 8, 8); // Increased horizontal padding from 20 to 24
        badgeHL.childAlignment = TextAnchor.MiddleCenter;
        badgeHL.childControlWidth = true;
        badgeHL.childControlHeight = true;
        badgeHL.childForceExpandWidth = false;
        badgeHL.childForceExpandHeight = false;

        // Add ContentSizeFitter to TypeBadge so it sizes dynamically based on text length
        ContentSizeFitter badgeFitter = typeBadgeObj.AddComponent<ContentSizeFitter>();
        badgeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        badgeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Type text inside the badge
        GameObject typeTextObj = new GameObject("TypeText", typeof(RectTransform), typeof(TextMeshProUGUI));
        typeTextObj.transform.SetParent(typeBadgeObj.transform, false);

        TextMeshProUGUI typeTMP = typeTextObj.GetComponent<TextMeshProUGUI>();
        typeTMP.text = "Water";
        typeTMP.fontSize = 24; // Increased from 20 for better readability
        typeTMP.alignment = TextAlignmentOptions.Center;
        typeTMP.color = Color.white; // White text on dark pill background
        typeTMP.fontStyle = FontStyles.Bold;

        // Pokedex classification text under the name container
        GameObject pokedexClassObj = new GameObject("PokedexClassText", typeof(RectTransform), typeof(TextMeshProUGUI));
        pokedexClassObj.transform.SetParent(headerObj.transform, false);
        RectTransform pokedexClassRect = pokedexClassObj.GetComponent<RectTransform>();
        pokedexClassRect.anchorMin = new Vector2(0f, 0f);
        pokedexClassRect.anchorMax = new Vector2(0.7f, 0.35f); // Updated from 0.4f to match upper row adjustment
        pokedexClassRect.offsetMin = new Vector2(0, 0);
        pokedexClassRect.offsetMax = new Vector2(0, 0);

        TextMeshProUGUI pokedexClassTMP = pokedexClassObj.GetComponent<TextMeshProUGUI>();
        pokedexClassTMP.text = "Lizard Pokémon";
        pokedexClassTMP.fontSize = 26; // Increased from 22 for better legibility
        pokedexClassTMP.alignment = TextAlignmentOptions.MidlineLeft; // Midline aligned
        pokedexClassTMP.color = new Color(0.12f, 0.14f, 0.18f, 1f); // #1E222B - premium dark slate for high visibility
        pokedexClassTMP.fontStyle = FontStyles.Italic; // Changed to Italic style for authentic Pokedex classification look

        // HPText on the top right
        GameObject hpTextObj = new GameObject("HPText", typeof(RectTransform), typeof(TextMeshProUGUI));
        hpTextObj.transform.SetParent(headerObj.transform, false);
        RectTransform hpTextRect = hpTextObj.GetComponent<RectTransform>();
        hpTextRect.anchorMin = new Vector2(0.7f, 0.35f); // Changed from 0f to 0.35f to match name Y anchors
        hpTextRect.anchorMax = new Vector2(1f, 1f);
        hpTextRect.offsetMin = Vector2.zero;
        hpTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI hpTMP = hpTextObj.GetComponent<TextMeshProUGUI>();
        hpTMP.text = "120 HP";
        hpTMP.fontSize = 42; // Increased from 38 to match name text size
        hpTMP.alignment = TextAlignmentOptions.MidlineRight; // Midline aligned right to match name midline vertical level
        hpTMP.color = new Color(0.12f, 0.14f, 0.18f, 1f); // #1E222B - premium dark slate
        hpTMP.fontStyle = FontStyles.Bold;

        // Sprite Image Panel
        GameObject artObj = new GameObject("ArtworkPanel", typeof(RectTransform), typeof(Image));
        artObj.transform.SetParent(canvasObj.transform, false);
        RectTransform artRect = artObj.GetComponent<RectTransform>();
        artRect.anchorMin = new Vector2(0.5f, 0.5f);
        artRect.anchorMax = new Vector2(0.5f, 0.5f);
        artRect.anchoredPosition = new Vector3(0, 170, 0); // Shifted down from 200 to 140
        artRect.sizeDelta = new Vector2(610, 340); // Increased size from 580x320 to 610x340
        
        Image artImg = artObj.GetComponent<Image>();
        artImg.color = new Color(0.9f, 0.9f, 0.9f, 0.6f);
        Sprite pokemonSprite = GetOrConvertSprite("Assets/Game Assets/Textures/pokemon.png");
        if (pokemonSprite != null)
        {
            artImg.sprite = pokemonSprite;
            artImg.color = Color.white;
        }

        // Add Outline component by default to match the desired neumorphic/drop-shadow border style
        Outline artOutline = artObj.AddComponent<Outline>();
        artOutline.effectColor = Color.black;
        artOutline.effectDistance = new Vector2(0f, 10f);
        artOutline.useGraphicAlpha = true;

        // Stats Panel (modern rounded badges)
        GameObject statsPanelObj = new GameObject("StatsPanel", typeof(RectTransform));
        statsPanelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform statsRect = statsPanelObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.5f, 0.5f);
        statsRect.anchorMax = new Vector2(0.5f, 0.5f);
        statsRect.anchoredPosition = new Vector3(0, -115, 0); // Shifted down from -60 to -115
        statsRect.sizeDelta = new Vector2(610, 110); // Increased from 580x90 to fit larger badges

        Sprite weakSprite = GetOrConvertSprite("Assets/Game Assets/Textures/weak.png");
        Sprite shieldSprite = GetOrConvertSprite("Assets/Game Assets/Textures/shield.png");

        Sprite chevronSprite = GetOrCreateShapeSprite("chevron", 
            new Vector2[] { new Vector2(0.5f, 0.85f), new Vector2(0.8f, 0.60f), new Vector2(0.7f, 0.50f), new Vector2(0.5f, 0.68f), new Vector2(0.3f, 0.50f), new Vector2(0.2f, 0.60f) },
            new Vector2[] { new Vector2(0.5f, 0.55f), new Vector2(0.8f, 0.30f), new Vector2(0.7f, 0.20f), new Vector2(0.5f, 0.38f), new Vector2(0.3f, 0.20f), new Vector2(0.2f, 0.30f) }
        );
        Sprite lightningSprite = GetOrCreateShapeSprite("lightning", 
            new Vector2[] { new Vector2(0.55f, 0.90f), new Vector2(0.80f, 0.52f), new Vector2(0.52f, 0.52f), new Vector2(0.65f, 0.10f), new Vector2(0.25f, 0.48f), new Vector2(0.48f, 0.48f) }
        );
        Sprite retreatSprite = GetOrCreateShapeSprite("retreat", 
            new Vector2[] { new Vector2(0.20f, 0.20f), new Vector2(0.65f, 0.20f), new Vector2(0.48f, 0.37f), new Vector2(0.85f, 0.74f), new Vector2(0.74f, 0.85f), new Vector2(0.37f, 0.48f), new Vector2(0.20f, 0.65f) }
        );
        
        // Define Star shape
        Vector2[] starPoly = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float angle = i * Mathf.PI / 5.0f - Mathf.PI / 2.0f;
            float r = (i % 2 == 0) ? 0.42f : 0.18f;
            starPoly[i] = new Vector2(0.5f + r * Mathf.Cos(angle), 0.5f + r * Mathf.Sin(angle));
        }
        Sprite starIconSprite = GetOrCreateShapeSprite("star_icon", starPoly);

        // Helper to create a badge
        System.Func<string, Vector2, Vector2, Sprite, TextMeshProUGUI> createBadge = (badgeName, anchoredPos, sizeDelta, iconSprite) =>
        {
            GameObject badgeObj = new GameObject(badgeName, typeof(RectTransform), typeof(Image));
            badgeObj.transform.SetParent(statsPanelObj.transform, false);
            
            RectTransform bRect = badgeObj.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0.5f);
            bRect.anchorMax = new Vector2(0.5f, 0.5f);
            bRect.anchoredPosition = anchoredPos;
            bRect.sizeDelta = sizeDelta;

            Image img = badgeObj.GetComponent<Image>();
            img.sprite = badgeBgSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            // Create text child
            GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(badgeObj.transform, false);

            RectTransform tRect = txtObj.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;

            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 24; // Increased from 20 for better visibility
            tmp.color = new Color(0.36f, 0.39f, 0.44f, 1f); // #5C6370 - beautiful gray for label
            tmp.fontStyle = FontStyles.Bold;
            tmp.text = badgeName;

            if (iconSprite != null)
            {
                // Create icon child
                GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(badgeObj.transform, false);

                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.anchoredPosition = new Vector2(24, 0); // Position slightly in from left edge
                iconRect.sizeDelta = new Vector2(24, 24); // Increased from 20x20 for scale

                Image iconImg = iconObj.GetComponent<Image>();
                iconImg.sprite = iconSprite;
                iconImg.color = new Color(0.18f, 0.22f, 0.29f, 1f); // Cohesive slate/dark grey text color

                tRect.offsetMin = new Vector2(56, 0); // Padding left to make space for the icon (increased from 52)
                tRect.offsetMax = new Vector2(-10, 0); // Padding right
                tmp.alignment = TextAlignmentOptions.Left; // Left alignment works better when there is an icon
            }
            else
            {
                tRect.offsetMin = new Vector2(10, 0); // Padding left
                tRect.offsetMax = new Vector2(-10, 0); // Padding right
                tmp.alignment = TextAlignmentOptions.Center; // Center alignment is best for text-only
            }

            return tmp;
        };

        Vector2 badgeSize = new Vector2(195, 48); // Increased size from 180x38 to look bolder and larger
        TextMeshProUGUI badgeStageTmp = createBadge("Badge_Stage", new Vector2(-205, 28), badgeSize, chevronSprite);
        TextMeshProUGUI badgeCPTmp = createBadge("Badge_CP", new Vector2(0, 28), badgeSize, lightningSprite);
        TextMeshProUGUI badgeRetreatTmp = createBadge("Badge_Retreat", new Vector2(205, 28), badgeSize, retreatSprite);

        TextMeshProUGUI badgeWeakTmp = createBadge("Badge_Weakness", new Vector2(-205, -28), badgeSize, weakSprite);
        TextMeshProUGUI badgeResistTmp = createBadge("Badge_Resistance", new Vector2(0, -28), badgeSize, shieldSprite);
        TextMeshProUGUI badgeRarityTmp = createBadge("Badge_Rarity", new Vector2(205, -28), badgeSize, starIconSprite);

        // Attacks Container
        GameObject attacksPanelObj = new GameObject("AttacksPanel", typeof(RectTransform));
        attacksPanelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform attacksRect = attacksPanelObj.GetComponent<RectTransform>();
        attacksRect.anchorMin = new Vector2(0f, 0.03f); // Shifted down from 0.06 to 0.03
        attacksRect.anchorMax = new Vector2(1f, 0.26f); // Shifted down from 0.34 to 0.26
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

        TextMeshProUGUI atk1CostTMP = CreateTextElement(atk1Obj, "Atk1Cost", "G C", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(50, 22), new Vector2(110, 45), 28, TextAlignmentOptions.Left);
        TextMeshProUGUI atk1NameTMP = CreateTextElement(atk1Obj, "Atk1Name", "Attack One", new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(165, 22), new Vector2(280, 45), 32, TextAlignmentOptions.Left);
        TextMeshProUGUI atk1DmgTMP = CreateTextElement(atk1Obj, "Atk1Damage", "30", new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-50, 22), new Vector2(110, 45), 32, TextAlignmentOptions.Right);
        TextMeshProUGUI atk1DescTMP = CreateTextElement(atk1Obj, "Atk1Description", "Deals 30 damage.", new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(50, -10), new Vector2(-100, 32), 20, TextAlignmentOptions.Left);

        // Attack 2 elements
        GameObject atk2Obj = new GameObject("Attack2", typeof(RectTransform));
        atk2Obj.transform.SetParent(attacksPanelObj.transform, false);
        RectTransform atk2Rect = atk2Obj.GetComponent<RectTransform>();
        atk2Rect.anchorMin = new Vector2(0, 0);
        atk2Rect.anchorMax = new Vector2(1, 0.5f);
        atk2Rect.offsetMin = Vector2.zero;
        atk2Rect.offsetMax = Vector2.zero;

        TextMeshProUGUI atk2CostTMP = CreateTextElement(atk2Obj, "Atk2Cost", "G G C", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(50, 22), new Vector2(110, 45), 28, TextAlignmentOptions.Left);
        TextMeshProUGUI atk2NameTMP = CreateTextElement(atk2Obj, "Atk2Name", "Attack Two", new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(165, 22), new Vector2(280, 45), 32, TextAlignmentOptions.Left);
        TextMeshProUGUI atk2DmgTMP = CreateTextElement(atk2Obj, "Atk2Damage", "70", new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-50, 22), new Vector2(110, 45), 32, TextAlignmentOptions.Right);
        TextMeshProUGUI atk2DescTMP = CreateTextElement(atk2Obj, "Atk2Description", "Discard an energy card.", new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(50, -10), new Vector2(-100, 32), 20, TextAlignmentOptions.Left);

        // 8. Bind UI components to UI Controller
        var nameTextField = typeof(CardUIController).GetField("nameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hpTextField = typeof(CardUIController).GetField("hpText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var typeTextField = typeof(CardUIController).GetField("typeText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pokedexClassTextField = typeof(CardUIController).GetField("pokedexClassText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pokemonImageField = typeof(CardUIController).GetField("pokemonImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var attack1NameField = typeof(CardUIController).GetField("attack1Name", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack1CostField = typeof(CardUIController).GetField("attack1Cost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack1DamageField = typeof(CardUIController).GetField("attack1Damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack1DescField = typeof(CardUIController).GetField("attack1Description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var attack2NameField = typeof(CardUIController).GetField("attack2Name", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack2CostField = typeof(CardUIController).GetField("attack2Cost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack2DamageField = typeof(CardUIController).GetField("attack2Damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack2DescField = typeof(CardUIController).GetField("attack2Description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var badgeStageField = typeof(CardUIController).GetField("badgeStageTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var badgeCPField = typeof(CardUIController).GetField("badgeCPTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var badgeRetreatField = typeof(CardUIController).GetField("badgeRetreatTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var badgeWeakField = typeof(CardUIController).GetField("badgeWeakTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var badgeResistField = typeof(CardUIController).GetField("badgeResistTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var badgeRarityField = typeof(CardUIController).GetField("badgeRarityTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        nameTextField?.SetValue(uiController, nameTMP);
        hpTextField?.SetValue(uiController, hpTMP);
        typeTextField?.SetValue(uiController, typeTMP);
        pokedexClassTextField?.SetValue(uiController, pokedexClassTMP);
        pokemonImageField?.SetValue(uiController, artImg);

        attack1NameField?.SetValue(uiController, atk1NameTMP);
        attack1CostField?.SetValue(uiController, atk1CostTMP);
        attack1DamageField?.SetValue(uiController, atk1DmgTMP);
        attack1DescField?.SetValue(uiController, atk1DescTMP);

        attack2NameField?.SetValue(uiController, atk2NameTMP);
        attack2CostField?.SetValue(uiController, atk2CostTMP);
        attack2DamageField?.SetValue(uiController, atk2DmgTMP);
        attack2DescField?.SetValue(uiController, atk2DescTMP);

        badgeStageField?.SetValue(uiController, badgeStageTmp);
        badgeCPField?.SetValue(uiController, badgeCPTmp);
        badgeRetreatField?.SetValue(uiController, badgeRetreatTmp);
        badgeWeakField?.SetValue(uiController, badgeWeakTmp);
        badgeResistField?.SetValue(uiController, badgeResistTmp);
        badgeRarityField?.SetValue(uiController, badgeRarityTmp);
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

    private static Sprite GetOrCreateBadgeSprite()
    {
        string dir = "Assets/Game Assets/Textures";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        string path = $"{dir}/BadgeBackground.png";
        
        bool exists = File.Exists(path);
        if (!exists)
        {
            // Create a 64x64 rounded rectangle texture
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color bgColor = new Color(0.95f, 0.96f, 0.97f, 1f); // #f2f4f7
            float radius = 16f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = x < radius ? radius : (x >= size - radius ? size - radius - 1 : x);
                    float cy = y < radius ? radius : (y >= size - radius ? size - radius - 1 : y);
                    float dx = x - cx;
                    float dy = y - cy;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        tex.SetPixel(x, y, bgColor);
                    }
                    else if (x >= radius && x < size - radius || y >= radius && y < size - radius)
                    {
                        tex.SetPixel(x, y, bgColor);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        // Configure sprite with 9-slicing (doing this regardless to ensure correctness)
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            bool needsReimport = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                needsReimport = true;
            }
            Vector4 targetBorder = new Vector4(16f, 16f, 16f, 16f);
            if (importer.spriteBorder != targetBorder)
            {
                importer.spriteBorder = targetBorder;
                needsReimport = true;
            }
            if (needsReimport)
            {
                importer.SaveAndReimport();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static bool IsPointInPolygon(Vector2 p, Vector2[] poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    private static Sprite GetOrCreateShapeSprite(string filename, params Vector2[][] polygons)
    {
        string dir = "Assets/Game Assets/Textures";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        string path = $"{dir}/{filename}.png";
        
        bool exists = File.Exists(path);
        if (!exists)
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2((float)x / size, (float)y / size);
                    bool inAny = false;
                    foreach (var poly in polygons)
                    {
                        if (IsPointInPolygon(p, poly))
                        {
                            inAny = true;
                            break;
                        }
                    }
                    tex.SetPixel(x, y, inAny ? Color.white : Color.clear);
                }
            }
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            bool needsReimport = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                needsReimport = true;
            }
            if (needsReimport)
            {
                importer.SaveAndReimport();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
