#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SceneSetupHelper
{
    [MenuItem("Pokemon TCG/Setup Main Scene UI")]
    public static void SetupMainSceneUI()
    {
        // 1. Setup Main Camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera", typeof(Camera));
            cam = camObj.GetComponent<Camera>();
        }
        cam.transform.position = new Vector3(0, 0.4f, -4.5f);
        cam.transform.rotation = Quaternion.identity;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.10f, 0.14f, 1f); // Premium deep dark slate background

        // Disable 3D grid anchor if it exists so it doesn't distract/clash
        GameObject gridAnchorObj = GameObject.Find("CardGridAnchor");
        if (gridAnchorObj != null)
        {
            gridAnchorObj.SetActive(false);
        }

        // 2. Setup Main Canvas
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas == null || canvas.gameObject.name == "CardCanvas")
        {
            GameObject canvasObj = new GameObject("Main Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
        }

        // Clean existing Main UI panels
        Transform oldHeader = canvas.transform.Find("MainHeaderPanel");
        if (oldHeader != null) Object.DestroyImmediate(oldHeader.gameObject);

        Transform oldStore = canvas.transform.Find("StorePanel");
        if (oldStore != null) Object.DestroyImmediate(oldStore.gameObject);

        Transform oldCollection = canvas.transform.Find("CollectionPanel");
        if (oldCollection != null) Object.DestroyImmediate(oldCollection.gameObject);

        Transform oldOverlay = canvas.transform.Find("InspectOverlay");
        if (oldOverlay != null) Object.DestroyImmediate(oldOverlay.gameObject);

        // 3. Header Panel
        GameObject headerObj = new GameObject("MainHeaderPanel", typeof(RectTransform), typeof(Image));
        headerObj.transform.SetParent(canvas.transform, false);
        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 0.84f);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.offsetMin = Vector2.zero;
        headerRect.offsetMax = Vector2.zero;
        
        Image headerImg = headerObj.GetComponent<Image>();
        headerImg.color = new Color(0.12f, 0.15f, 0.22f, 0.9f); // Glassmorphism translucent dark slate

        // App title
        GameObject titleObj = new GameObject("AppTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(headerObj.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.45f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
        titleTMP.text = "POKEMON CARD CREATOR";
        titleTMP.fontSize = 44;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        titleTMP.fontStyle = FontStyles.Bold;

        // Tabs Row
        GameObject tabRowObj = new GameObject("TabRow", typeof(RectTransform));
        tabRowObj.transform.SetParent(headerObj.transform, false);
        RectTransform tabRowRect = tabRowObj.GetComponent<RectTransform>();
        tabRowRect.anchorMin = new Vector2(0.1f, 0f);
        tabRowRect.anchorMax = new Vector2(0.9f, 0.4f);
        tabRowRect.offsetMin = Vector2.zero;
        tabRowRect.offsetMax = Vector2.zero;

        HorizontalLayoutGroup tabHL = tabRowObj.AddComponent<HorizontalLayoutGroup>();
        tabHL.spacing = 100;
        tabHL.childAlignment = TextAnchor.MiddleCenter;
        tabHL.childControlWidth = false;
        tabHL.childControlHeight = false;

        // Collection Tab
        GameObject colBtnObj = new GameObject("CollectionTabButton", typeof(RectTransform), typeof(Image), typeof(Button));
        colBtnObj.transform.SetParent(tabRowObj.transform, false);
        RectTransform colBtnRect = colBtnObj.GetComponent<RectTransform>();
        colBtnRect.sizeDelta = new Vector2(320, 70);
        colBtnObj.GetComponent<Image>().color = Color.clear;

        // Dedicated underline childed directly to Collection tab button
        GameObject colUnderlineObj = new GameObject("Underline", typeof(RectTransform), typeof(Image));
        colUnderlineObj.transform.SetParent(colBtnObj.transform, false);
        RectTransform colUnderlineRect = colUnderlineObj.GetComponent<RectTransform>();
        colUnderlineRect.anchorMin = new Vector2(0f, 0f);
        colUnderlineRect.anchorMax = new Vector2(1f, 0f);
        colUnderlineRect.pivot = new Vector2(0.5f, 0f);
        colUnderlineRect.anchoredPosition = new Vector2(0, 0f); // Exactly at bottom edge of button
        colUnderlineRect.sizeDelta = new Vector2(0, 6f); // Span full button width, 6 height
        colUnderlineObj.GetComponent<Image>().color = new Color(0.95f, 0.8f, 0.1f, 1f); // Vibrant Gold

        GameObject colTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        colTxtObj.transform.SetParent(colBtnObj.transform, false);
        RectTransform colTxtRect = colTxtObj.GetComponent<RectTransform>();
        colTxtRect.anchorMin = Vector2.zero;
        colTxtRect.anchorMax = Vector2.one;
        colTxtRect.offsetMin = Vector2.zero;
        colTxtRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI colTxtTMP = colTxtObj.GetComponent<TextMeshProUGUI>();
        colTxtTMP.text = "Collection";
        colTxtTMP.fontSize = 34;
        colTxtTMP.alignment = TextAlignmentOptions.Center;
        colTxtTMP.color = Color.white;
        colTxtTMP.fontStyle = FontStyles.Bold;

        Button colBtn = colBtnObj.GetComponent<Button>();

        // Store Tab
        GameObject storeBtnObj = new GameObject("StoreTabButton", typeof(RectTransform), typeof(Image), typeof(Button));
        storeBtnObj.transform.SetParent(tabRowObj.transform, false);
        RectTransform storeBtnRect = storeBtnObj.GetComponent<RectTransform>();
        storeBtnRect.sizeDelta = new Vector2(320, 70);
        storeBtnObj.GetComponent<Image>().color = Color.clear;

        // Dedicated underline childed directly to Store tab button
        GameObject storeUnderlineObj = new GameObject("Underline", typeof(RectTransform), typeof(Image));
        storeUnderlineObj.transform.SetParent(storeBtnObj.transform, false);
        RectTransform storeUnderlineRect = storeUnderlineObj.GetComponent<RectTransform>();
        storeUnderlineRect.anchorMin = new Vector2(0f, 0f);
        storeUnderlineRect.anchorMax = new Vector2(1f, 0f);
        storeUnderlineRect.pivot = new Vector2(0.5f, 0f);
        storeUnderlineRect.anchoredPosition = new Vector2(0, 0f);
        storeUnderlineRect.sizeDelta = new Vector2(0, 6f);
        storeUnderlineObj.GetComponent<Image>().color = new Color(0.95f, 0.8f, 0.1f, 1f); // Vibrant Gold

        GameObject storeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        storeTxtObj.transform.SetParent(storeBtnObj.transform, false);
        RectTransform storeTxtRect = storeTxtObj.GetComponent<RectTransform>();
        storeTxtRect.anchorMin = Vector2.zero;
        storeTxtRect.anchorMax = Vector2.one;
        storeTxtRect.offsetMin = Vector2.zero;
        storeTxtRect.offsetMax = Vector2.zero;

        TextMeshProUGUI storeTxtTMP = storeTxtObj.GetComponent<TextMeshProUGUI>();
        storeTxtTMP.text = "Store";
        storeTxtTMP.fontSize = 34;
        storeTxtTMP.alignment = TextAlignmentOptions.Center;
        storeTxtTMP.color = new Color(0.5f, 0.55f, 0.65f, 1f);
        storeTxtTMP.fontStyle = FontStyles.Normal;

        Button storeBtn = storeBtnObj.GetComponent<Button>();

        // 4. Store Panel
        GameObject storePanelObj = new GameObject("StorePanel", typeof(RectTransform), typeof(Image));
        storePanelObj.transform.SetParent(canvas.transform, false);
        RectTransform storePanelRect = storePanelObj.GetComponent<RectTransform>();
        storePanelRect.anchorMin = new Vector2(0, 0);
        storePanelRect.anchorMax = new Vector2(1, 0.84f);
        storePanelRect.offsetMin = Vector2.zero;
        storePanelRect.offsetMax = Vector2.zero;

        Image storePanelImg = storePanelObj.GetComponent<Image>();
        storePanelImg.color = new Color(0.08f, 0.10f, 0.14f, 1f);

        GameObject storeMsgObj = new GameObject("StoreMessage", typeof(RectTransform), typeof(TextMeshProUGUI));
        storeMsgObj.transform.SetParent(storePanelObj.transform, false);
        RectTransform storeMsgRect = storeMsgObj.GetComponent<RectTransform>();
        storeMsgRect.anchorMin = new Vector2(0, 0.4f);
        storeMsgRect.anchorMax = new Vector2(1, 0.6f);
        storeMsgRect.offsetMin = Vector2.zero;
        storeMsgRect.offsetMax = Vector2.zero;

        TextMeshProUGUI storeMsgTMP = storeMsgObj.GetComponent<TextMeshProUGUI>();
        storeMsgTMP.text = "<size=48><b>POKEMON TCG STORE</b></size>\n\n<size=32><color=#5C6370>Booster Packs & Shop items arriving soon!</color></size>";
        storeMsgTMP.alignment = TextAlignmentOptions.Center;
        storeMsgTMP.color = new Color(0.12f, 0.14f, 0.18f, 1f);

        // 5. Collection Panel (2D Grid View ScrollRect)
        GameObject colPanelObj = new GameObject("CollectionPanel", typeof(RectTransform), typeof(Image));
        colPanelObj.transform.SetParent(canvas.transform, false);
        RectTransform colPanelRect = colPanelObj.GetComponent<RectTransform>();
        colPanelRect.anchorMin = new Vector2(0, 0);
        colPanelRect.anchorMax = new Vector2(1, 0.84f);
        colPanelRect.offsetMin = Vector2.zero;
        colPanelRect.offsetMax = Vector2.zero;

        Image colPanelImg = colPanelObj.GetComponent<Image>();
        colPanelImg.color = new Color(0.08f, 0.10f, 0.14f, 1f);

        // ScrollView Object
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(colPanelObj.transform, false);
        RectTransform scrollRectTrans = scrollObj.GetComponent<RectTransform>();
        scrollRectTrans.anchorMin = Vector2.zero;
        scrollRectTrans.anchorMax = Vector2.one;
        scrollRectTrans.offsetMin = new Vector2(20, 20);
        scrollRectTrans.offsetMax = new Vector2(-20, -20);

        ScrollRect scrollComponent = scrollObj.GetComponent<ScrollRect>();
        scrollComponent.horizontal = false;
        scrollComponent.vertical = true;
        scrollComponent.movementType = ScrollRect.MovementType.Elastic;

        // Viewport (Using RectMask2D for clean performance and zero-image clipping dependencies)
        GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewportObj.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
 
        scrollComponent.viewport = viewportRect;

        // Content
        GameObject contentObj = new GameObject("Content", typeof(RectTransform));
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        GridLayoutGroup gridLayout = contentObj.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(480, 672); // Bolder card size inside 2-column list
        gridLayout.spacing = new Vector2(40, 40);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 2;
        gridLayout.padding = new RectOffset(20, 20, 30, 30);

        ContentSizeFitter sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        scrollComponent.content = contentRect;

        // 6. Generate/Save the 2D Card Prefab
        GameObject card2DPrefabAsset = GetOrCreate2DCardPrefab();

        // 7. Setup Detailed Inspect Overlay
        GameObject overlayObj = new GameObject("InspectOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
        overlayObj.transform.SetParent(canvas.transform, false);
        RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImg = overlayObj.GetComponent<Image>();
        overlayImg.color = new Color(0.04f, 0.05f, 0.07f, 0.92f); // High-premium extra-dark glass background

        Button overlayBgBtn = overlayObj.GetComponent<Button>();
 
        // Large Card Container
        GameObject cardContainerObj = new GameObject("CardContainer", typeof(RectTransform));
        cardContainerObj.transform.SetParent(overlayObj.transform, false);
        RectTransform containerRect = cardContainerObj.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0, 30);
        containerRect.sizeDelta = new Vector2(700, 980);

        // Instantiate the 2D card inside the container to show high-res details
        GameObject overlayCardInst = PrefabUtility.InstantiatePrefab(card2DPrefabAsset, cardContainerObj.transform) as GameObject;
        overlayCardInst.name = "LargePokemonCard";
        RectTransform overlayCardRect = overlayCardInst.GetComponent<RectTransform>();
        overlayCardRect.anchorMin = Vector2.zero;
        overlayCardRect.anchorMax = Vector2.one;
        overlayCardRect.offsetMin = Vector2.zero;
        overlayCardRect.offsetMax = Vector2.zero;
        overlayCardRect.localScale = Vector3.one;

        // Disable standard click on the details overlay version of the card
        Button overlayCardBtn = overlayCardInst.GetComponent<Button>();
        if (overlayCardBtn != null) overlayCardBtn.enabled = false;

        Card2DUIController overlayCardController = overlayCardInst.GetComponent<Card2DUIController>();

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(overlayObj.transform, false);
        RectTransform closeBtnRect = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(0.5f, 0f);
        closeBtnRect.anchorMax = new Vector2(0.5f, 0f);
        closeBtnRect.anchoredPosition = new Vector2(0, 100);
        closeBtnRect.sizeDelta = new Vector2(250, 70);

        Image closeImg = closeBtnObj.GetComponent<Image>();
        closeImg.sprite = GetOrCreateCloseButtonSprite();
        closeImg.type = Image.Type.Sliced;
        closeImg.color = new Color(0.75f, 0.15f, 0.20f, 1f); // Premium dark crimson red

        GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTxtRect = closeTxtObj.GetComponent<RectTransform>();
        closeTxtRect.anchorMin = Vector2.zero;
        closeTxtRect.anchorMax = Vector2.one;
        closeTxtRect.offsetMin = Vector2.zero;
        closeTxtRect.offsetMax = Vector2.zero;

        TextMeshProUGUI closeTxt = closeTxtObj.GetComponent<TextMeshProUGUI>();
        closeTxt.text = "CLOSE";
        closeTxt.fontSize = 28;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color = Color.white;
        closeTxt.fontStyle = FontStyles.Bold;

        Button closeBtn = closeBtnObj.GetComponent<Button>();

        // Link background overlay click to close as well
        Navigation noneNav = new Navigation { mode = Navigation.Mode.None };
        overlayBgBtn.navigation = noneNav;
        closeBtn.navigation = noneNav;

        // 8. Bind references to MainUIManager
        GameObject uiManagerObj = GameObject.Find("MainUIManager");
        if (uiManagerObj == null)
        {
            uiManagerObj = new GameObject("MainUIManager");
        }
        MainUIManager uiManager = uiManagerObj.GetComponent<MainUIManager>();
        if (uiManager == null)
        {
            uiManager = uiManagerObj.AddComponent<MainUIManager>();
        }

        var mainCanvasField = typeof(MainUIManager).GetField("mainCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var appTitleField = typeof(MainUIManager).GetField("appTitleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var colTabField = typeof(MainUIManager).GetField("collectionTabButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var storeTabField = typeof(MainUIManager).GetField("storeTabButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var colUnderlineField = typeof(MainUIManager).GetField("collectionTabUnderline", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var storeUnderlineField = typeof(MainUIManager).GetField("storeTabUnderline", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var storePanelField = typeof(MainUIManager).GetField("storePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var colPanelField = typeof(MainUIManager).GetField("collectionPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var card2DPrefabField = typeof(MainUIManager).GetField("card2DPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gridContentContainerField = typeof(MainUIManager).GetField("gridContentContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var inspectOverlayField = typeof(MainUIManager).GetField("inspectOverlay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var inspectOverlayCardControllerField = typeof(MainUIManager).GetField("inspectOverlayCardController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var inspectOverlayCloseButtonField = typeof(MainUIManager).GetField("inspectOverlayCloseButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
 
        mainCanvasField?.SetValue(uiManager, canvas);
        appTitleField?.SetValue(uiManager, titleTMP);
        colTabField?.SetValue(uiManager, colBtn);
        storeTabField?.SetValue(uiManager, storeBtn);
        colUnderlineField?.SetValue(uiManager, colUnderlineObj);
        storeUnderlineField?.SetValue(uiManager, storeUnderlineObj);
        storePanelField?.SetValue(uiManager, storePanelObj);
        
        colPanelField?.SetValue(uiManager, colPanelObj);
        card2DPrefabField?.SetValue(uiManager, card2DPrefabAsset);
        gridContentContainerField?.SetValue(uiManager, contentRect);
        inspectOverlayField?.SetValue(uiManager, overlayObj);
        inspectOverlayCardControllerField?.SetValue(uiManager, overlayCardController);
        inspectOverlayCloseButtonField?.SetValue(uiManager, closeBtn);

        // Populate cardsData list
        var cardsDataField = typeof(MainUIManager).GetField("cardsData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (cardsDataField != null)
        {
            List<PokemonCardData> list = new List<PokemonCardData>();
            string[] guids = AssetDatabase.FindAssets("t:PokemonCardData", new string[] { "Assets/Game Assets/Data" });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PokemonCardData data = AssetDatabase.LoadAssetAtPath<PokemonCardData>(path);
                if (data != null) list.Add(data);
            }
            cardsDataField.SetValue(uiManager, list);
        }

        // Add background click listener in editor utility to also close it
        overlayBgBtn.onClick.RemoveAllListeners();
        #if UNITY_2019_1_OR_NEWER
        UnityEditor.Events.UnityEventTools.AddPersistentListener(overlayBgBtn.onClick, uiManager.HideInspectOverlay);
        // Force default UI material assignment on all Canvas Images to prevent missing material/pink bugs
        foreach (var img in canvas.GetComponentsInChildren<Image>(true))
        {
            img.material = Canvas.GetDefaultCanvasMaterial();
        }
        #endif

        uiManager.SwitchToTab(MainUIManager.Tab.Collection);
        EditorUtility.SetDirty(uiManager);
        Selection.activeGameObject = uiManagerObj;

        Debug.Log("Successfully transitioned Main Scene to 2D Canvas layout grid with detailed inspection overlay!");
    }

    private static GameObject GetOrCreate2DCardPrefab()
    {
        string path = "Assets/Game Assets/Prefabs/Pokemon2DCard.prefab";
        // Always recreate the prefab to ensure it updates with the latest sprite conversion assets and settings
        AssetDatabase.DeleteAsset(path);

        // Build the 2D Card layout from scratch
        GameObject root = new GameObject("Pokemon2DCard", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Card2DUIController));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(700, 980);

        Image bgImg = root.GetComponent<Image>();
        bgImg.type = Image.Type.Simple;

        Card2DUIController controller = root.GetComponent<Card2DUIController>();

        // Set references using reflection
        var bgImageField = typeof(Card2DUIController).GetField("bgImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        bgImageField?.SetValue(controller, bgImg);

        // Bind types images in Card2DUIController
        var typeBgsField = typeof(Card2DUIController).GetField("typeBackgrounds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (typeBgsField != null)
        {
            List<Card2DUIController.TypeSpriteMapping> list = new List<Card2DUIController.TypeSpriteMapping>();
            PokemonType[] types = (PokemonType[])System.Enum.GetValues(typeof(PokemonType));
            string[] textureNames = new string[] { "Grass", "Fire", "Water", "Lightning", "Psychic", "Fighting", "Darkness", "Metal", "Dragon", "Normal" };
            for (int i = 0; i < types.Length; i++)
            {
                string texPath = $"Assets/Game Assets/Textures/Property 1={textureNames[i]}.png";
                Sprite sprite = GetOrConvertSprite(texPath);
                if (sprite != null)
                {
                    list.Add(new Card2DUIController.TypeSpriteMapping { type = types[i], backgroundSprite = sprite });
                }
            }
            typeBgsField.SetValue(controller, list);
        }

        // Header Panel (Name & HP)
        GameObject headerObj = new GameObject("HeaderPanel", typeof(RectTransform));
        headerObj.transform.SetParent(root.transform, false);
        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 0.88f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.offsetMin = new Vector2(40, 0);
        headerRect.offsetMax = new Vector2(-40, -20);

        // Container for Name
        GameObject nameContainerObj = new GameObject("NameContainer", typeof(RectTransform));
        nameContainerObj.transform.SetParent(headerObj.transform, false);
        RectTransform nameContainerRect = nameContainerObj.GetComponent<RectTransform>();
        nameContainerRect.anchorMin = new Vector2(0f, 0.35f);
        nameContainerRect.anchorMax = new Vector2(0.7f, 1f);
        nameContainerRect.offsetMin = Vector2.zero;
        nameContainerRect.offsetMax = Vector2.zero;

        HorizontalLayoutGroup hlGroup = nameContainerObj.AddComponent<HorizontalLayoutGroup>();
        hlGroup.spacing = 15;
        hlGroup.childAlignment = TextAnchor.MiddleLeft;
        hlGroup.childControlWidth = true;
        hlGroup.childControlHeight = false;
        hlGroup.childForceExpandWidth = false;
        hlGroup.childForceExpandHeight = false;

        GameObject nameTextObj = new GameObject("NameText", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameTextObj.transform.SetParent(nameContainerObj.transform, false);
        TextMeshProUGUI nameTMP = nameTextObj.GetComponent<TextMeshProUGUI>();
        nameTMP.text = "Pokemon Name";
        nameTMP.fontSize = 42;
        nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
        nameTMP.color = new Color(0.12f, 0.14f, 0.18f, 1f);
        nameTMP.fontStyle = FontStyles.Bold;
        
        nameTextObj.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Type Badge
        GameObject typeBadgeObj = new GameObject("TypeBadge", typeof(RectTransform), typeof(Image));
        typeBadgeObj.transform.SetParent(nameContainerObj.transform, false);
        Image typeBadgeImg = typeBadgeObj.GetComponent<Image>();
        typeBadgeImg.sprite = GetOrCreateBadgeSprite();
        typeBadgeImg.type = Image.Type.Sliced;
        typeBadgeImg.color = new Color(0.18f, 0.22f, 0.29f, 1f);

        HorizontalLayoutGroup badgeHL = typeBadgeObj.AddComponent<HorizontalLayoutGroup>();
        badgeHL.padding = new RectOffset(24, 24, 8, 8);
        badgeHL.childAlignment = TextAnchor.MiddleCenter;
        badgeHL.childControlWidth = true;
        badgeHL.childControlHeight = true;
        badgeHL.childForceExpandWidth = false;
        badgeHL.childForceExpandHeight = false;

        typeBadgeObj.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        typeBadgeObj.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject typeTextObj = new GameObject("TypeText", typeof(RectTransform), typeof(TextMeshProUGUI));
        typeTextObj.transform.SetParent(typeBadgeObj.transform, false);
        TextMeshProUGUI typeTMP = typeTextObj.GetComponent<TextMeshProUGUI>();
        typeTMP.fontSize = 24;
        typeTMP.alignment = TextAlignmentOptions.Center;
        typeTMP.color = Color.white;
        typeTMP.fontStyle = FontStyles.Bold;

        // HP Text
        GameObject hpTextObj = new GameObject("HPText", typeof(RectTransform), typeof(TextMeshProUGUI));
        hpTextObj.transform.SetParent(headerObj.transform, false);
        RectTransform hpTextRect = hpTextObj.GetComponent<RectTransform>();
        hpTextRect.anchorMin = new Vector2(0.7f, 0.35f);
        hpTextRect.anchorMax = new Vector2(1f, 1f);
        hpTextRect.offsetMin = Vector2.zero;
        hpTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI hpTMP = hpTextObj.GetComponent<TextMeshProUGUI>();
        hpTMP.text = "100 HP";
        hpTMP.fontSize = 42;
        hpTMP.alignment = TextAlignmentOptions.MidlineRight;
        hpTMP.color = new Color(0.12f, 0.14f, 0.18f, 1f);
        hpTMP.fontStyle = FontStyles.Bold;

        // Pokedex Class Text
        GameObject pokedexClassObj = new GameObject("PokedexClassText", typeof(RectTransform), typeof(TextMeshProUGUI));
        pokedexClassObj.transform.SetParent(headerObj.transform, false);
        RectTransform pokedexClassRect = pokedexClassObj.GetComponent<RectTransform>();
        pokedexClassRect.anchorMin = new Vector2(0f, 0f);
        pokedexClassRect.anchorMax = new Vector2(0.7f, 0.35f);
        pokedexClassRect.offsetMin = Vector2.zero;
        pokedexClassRect.offsetMax = Vector2.zero;

        TextMeshProUGUI pokedexClassTMP = pokedexClassObj.GetComponent<TextMeshProUGUI>();
        pokedexClassTMP.text = "Class Pokemon";
        pokedexClassTMP.fontSize = 26;
        pokedexClassTMP.alignment = TextAlignmentOptions.MidlineLeft;
        pokedexClassTMP.color = new Color(0.12f, 0.14f, 0.18f, 1f);
        pokedexClassTMP.fontStyle = FontStyles.Italic;

        // Artwork Panel
        GameObject artObj = new GameObject("ArtworkPanel", typeof(RectTransform), typeof(Image));
        artObj.transform.SetParent(root.transform, false);
        RectTransform artRect = artObj.GetComponent<RectTransform>();
        artRect.anchorMin = new Vector2(0.5f, 0.5f);
        artRect.anchorMax = new Vector2(0.5f, 0.5f);
        artRect.anchoredPosition = new Vector3(0, 170, 0);
        artRect.sizeDelta = new Vector2(610, 340);
        
        Image artImg = artObj.GetComponent<Image>();
        artImg.color = Color.white;

        Outline artOutline = artObj.AddComponent<Outline>();
        artOutline.effectColor = Color.black;
        artOutline.effectDistance = new Vector2(0f, 10f);

        // Stats Panel (Badges)
        GameObject statsPanelObj = new GameObject("StatsPanel", typeof(RectTransform));
        statsPanelObj.transform.SetParent(root.transform, false);
        RectTransform statsRect = statsPanelObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.5f, 0.5f);
        statsRect.anchorMax = new Vector2(0.5f, 0.5f);
        statsRect.anchoredPosition = new Vector3(0, -115, 0);
        statsRect.sizeDelta = new Vector2(610, 110);

        Sprite weakSprite = GetOrConvertSprite("Assets/Game Assets/Textures/weak.png");
        Sprite shieldSprite = GetOrConvertSprite("Assets/Game Assets/Textures/shield.png");
        Sprite chevronSprite = GetOrConvertSprite("Assets/Game Assets/Textures/chevron.png");
        Sprite lightningSprite = GetOrConvertSprite("Assets/Game Assets/Textures/lightning.png");
        Sprite retreatSprite = GetOrConvertSprite("Assets/Game Assets/Textures/retreat.png");
        Sprite starIconSprite = GetOrConvertSprite("Assets/Game Assets/Textures/star_icon.png");

        Sprite badgeBgSprite = GetOrCreateBadgeSprite();

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

            GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(badgeObj.transform, false);

            RectTransform tRect = txtObj.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;

            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.fontSizeMin = 18;
            tmp.fontSizeMax = 26;
            tmp.enableAutoSizing = true;
            tmp.enableWordWrapping = false;
            tmp.color = new Color(0.36f, 0.39f, 0.44f, 1f);
            tmp.fontStyle = FontStyles.Bold;

            if (iconSprite != null)
            {
                GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(badgeObj.transform, false);

                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.anchoredPosition = new Vector2(16, 0);
                iconRect.sizeDelta = new Vector2(24, 24);

                Image iconImg = iconObj.GetComponent<Image>();
                iconImg.sprite = iconSprite;
                iconImg.color = new Color(0.18f, 0.22f, 0.29f, 1f);

                tRect.offsetMin = new Vector2(46, 0);
                tRect.offsetMax = new Vector2(-10, 0);
                tmp.alignment = TextAlignmentOptions.Left;
            }
            else
            {
                tRect.offsetMin = new Vector2(10, 0);
                tRect.offsetMax = new Vector2(-10, 0);
                tmp.alignment = TextAlignmentOptions.Center;
            }

            return tmp;
        };

        Vector2 badgeSize = new Vector2(195, 48);
        TextMeshProUGUI badgeStageTmp = createBadge("Badge_Stage", new Vector2(-205, 28), badgeSize, chevronSprite);
        TextMeshProUGUI badgeCPTmp = createBadge("Badge_CP", new Vector2(0, 28), badgeSize, lightningSprite);
        TextMeshProUGUI badgeRetreatTmp = createBadge("Badge_Retreat", new Vector2(205, 28), badgeSize, retreatSprite);

        TextMeshProUGUI badgeWeakTmp = createBadge("Badge_Weakness", new Vector2(-205, -28), badgeSize, weakSprite);
        TextMeshProUGUI badgeResistTmp = createBadge("Badge_Resistance", new Vector2(0, -28), badgeSize, shieldSprite);
        TextMeshProUGUI badgeRarityTmp = createBadge("Badge_Rarity", new Vector2(205, -28), badgeSize, starIconSprite);

        // Attacks Panel
        GameObject attacksPanelObj = new GameObject("AttacksPanel", typeof(RectTransform));
        attacksPanelObj.transform.SetParent(root.transform, false);
        RectTransform attacksRect = attacksPanelObj.GetComponent<RectTransform>();
        attacksRect.anchorMin = new Vector2(0f, 0.03f);
        attacksRect.anchorMax = new Vector2(1f, 0.30f);
        attacksRect.offsetMin = new Vector2(40, 0);
        attacksRect.offsetMax = new Vector2(-40, 0);

        System.Func<string, GameObject> createAttackSlot = (slotName) =>
        {
            GameObject slotObj = new GameObject(slotName, typeof(RectTransform), typeof(Image));
            slotObj.transform.SetParent(attacksPanelObj.transform, false);
            
            RectTransform sRect = slotObj.GetComponent<RectTransform>();
            sRect.anchorMin = Vector2.zero;
            sRect.anchorMax = Vector2.one;
            sRect.offsetMin = Vector2.zero;
            sRect.offsetMax = Vector2.zero;

            Image img = slotObj.GetComponent<Image>();
            img.sprite = badgeBgSprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(1f, 1f, 1f, 0.45f);

            GameObject nameObj = new GameObject("AttackName", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(slotObj.transform, false);
            nameObj.GetComponent<TextMeshProUGUI>().fontSize = 28;
            nameObj.GetComponent<TextMeshProUGUI>().color = new Color(0.12f, 0.14f, 0.17f, 0.95f);

            GameObject costObj = new GameObject("AttackCost", typeof(RectTransform), typeof(TextMeshProUGUI));
            costObj.transform.SetParent(slotObj.transform, false);

            GameObject damageObj = new GameObject("AttackDamage", typeof(RectTransform), typeof(TextMeshProUGUI));
            damageObj.transform.SetParent(slotObj.transform, false);
            RectTransform dmgRect = damageObj.GetComponent<RectTransform>();
            dmgRect.anchoredPosition = new Vector2(-25, 32);
            dmgRect.sizeDelta = new Vector2(100, 36);
            damageObj.GetComponent<TextMeshProUGUI>().fontSize = 32;
            damageObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineRight;

            GameObject descObj = new GameObject("AttackDescription", typeof(RectTransform), typeof(TextMeshProUGUI));
            descObj.transform.SetParent(slotObj.transform, false);

            return slotObj;
        };

        GameObject slot1 = createAttackSlot("Slot1");
        GameObject slot2 = createAttackSlot("Slot2");

        // Bind fields on Card2DUIController using reflection
        var nameTextF = typeof(Card2DUIController).GetField("nameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hpTextF = typeof(Card2DUIController).GetField("hpText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var typeTextF = typeof(Card2DUIController).GetField("typeText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pokedexClassTextF = typeof(Card2DUIController).GetField("pokedexClassText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pokemonImageF = typeof(Card2DUIController).GetField("pokemonImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var attack1NameF = typeof(Card2DUIController).GetField("attack1Name", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack1CostF = typeof(Card2DUIController).GetField("attack1Cost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack1DamageF = typeof(Card2DUIController).GetField("attack1Damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack1DescriptionF = typeof(Card2DUIController).GetField("attack1Description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var attack2NameF = typeof(Card2DUIController).GetField("attack2Name", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack2CostF = typeof(Card2DUIController).GetField("attack2Cost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack2DamageF = typeof(Card2DUIController).GetField("attack2Damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attack2DescriptionF = typeof(Card2DUIController).GetField("attack2Description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var badgeStageF = typeof(Card2DUIController).GetField("badgeStageTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var badgeCPF = typeof(Card2DUIController).GetField("badgeCPTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var badgeRetreatF = typeof(Card2DUIController).GetField("badgeRetreatTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var badgeWeakF = typeof(Card2DUIController).GetField("badgeWeakTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var badgeResistF = typeof(Card2DUIController).GetField("badgeResistTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var badgeRarityF = typeof(Card2DUIController).GetField("badgeRarityTmp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        nameTextF?.SetValue(controller, nameTMP);
        hpTextF?.SetValue(controller, hpTMP);
        typeTextF?.SetValue(controller, typeTMP);
        pokedexClassTextF?.SetValue(controller, pokedexClassTMP);
        pokemonImageF?.SetValue(controller, artImg);

        attack1NameF?.SetValue(controller, slot1.transform.Find("AttackName").GetComponent<TextMeshProUGUI>());
        attack1CostF?.SetValue(controller, slot1.transform.Find("AttackCost").GetComponent<TextMeshProUGUI>());
        attack1DamageF?.SetValue(controller, slot1.transform.Find("AttackDamage").GetComponent<TextMeshProUGUI>());
        attack1DescriptionF?.SetValue(controller, slot1.transform.Find("AttackDescription").GetComponent<TextMeshProUGUI>());

        attack2NameF?.SetValue(controller, slot2.transform.Find("AttackName").GetComponent<TextMeshProUGUI>());
        attack2CostF?.SetValue(controller, slot2.transform.Find("AttackCost").GetComponent<TextMeshProUGUI>());
        attack2DamageF?.SetValue(controller, slot2.transform.Find("AttackDamage").GetComponent<TextMeshProUGUI>());
        attack2DescriptionF?.SetValue(controller, slot2.transform.Find("AttackDescription").GetComponent<TextMeshProUGUI>());

        badgeStageF?.SetValue(controller, badgeStageTmp);
        badgeCPF?.SetValue(controller, badgeCPTmp);
        badgeRetreatF?.SetValue(controller, badgeRetreatTmp);
        badgeWeakF?.SetValue(controller, badgeWeakTmp);
        badgeResistF?.SetValue(controller, badgeResistTmp);
        badgeRarityF?.SetValue(controller, badgeRarityTmp);

        // Force default UI material assignment on all prefab Images to prevent missing material/pink bugs
        foreach (var img in root.GetComponentsInChildren<Image>(true))
        {
            img.material = Canvas.GetDefaultCanvasMaterial();
        }

        // Save prefab
        System.IO.Directory.CreateDirectory("Assets/Game Assets/Prefabs");
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefabAsset;
    }

    private static Sprite GetOrCreateBadgeSprite()
    {
        string path = "Assets/Game Assets/Textures/rounded_badge.png";
        Texture2D exist = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (exist == null)
        {
            // Build a simple 64x64 rounded badge texture dynamically
            Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color fill = Color.white;
            float radius = 28f;
            Vector2 center = new Vector2(32f, 32f);
            
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    // Rounded rectangle logic
                    float dx = Mathf.Max(0, Mathf.Abs(x - 32) - (32 - radius));
                    float dy = Mathf.Max(0, Mathf.Abs(y - 32) - (32 - radius));
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    if (dist < radius - 1.5f)
                    {
                        tex.SetPixel(x, y, fill);
                    }
                    else if (dist < radius)
                    {
                        // Antialiased border
                        float alpha = radius - dist;
                        tex.SetPixel(x, y, Color.Lerp(transparent, fill, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            
            // Set sliced border settings
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteBorder = new Vector4(28, 28, 28, 28);
                importer.SaveAndReimport();
            }
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite GetOrCreateCloseButtonSprite()
    {
        string path = "Assets/Game Assets/Textures/close_badge.png";
        Texture2D exist = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (exist == null)
        {
            Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color fill = Color.white;
            float radius = 16f;
            
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dx = Mathf.Max(0, Mathf.Abs(x - 32) - (32 - radius));
                    float dy = Mathf.Max(0, Mathf.Abs(y - 32) - (32 - radius));
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    if (dist < radius - 1.5f) tex.SetPixel(x, y, fill);
                    else if (dist < radius) tex.SetPixel(x, y, Color.Lerp(transparent, fill, radius - dist));
                    else tex.SetPixel(x, y, transparent);
                }
            }
            tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.Refresh();
            
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteBorder = new Vector4(16, 16, 16, 16);
                importer.SaveAndReimport();
            }
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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

    private static Sprite GetOrCreateRadialGlowSprite()
    {
        string path = "Assets/Game Assets/Textures/radial_glow.png";
        Texture2D exist = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (exist == null)
        {
            // Create a soft white radial gradient texture
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float halfSize = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - halfSize;
                    float dy = y - halfSize;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) / halfSize;
                    float alpha = Mathf.Clamp01(1f - dist);
                    // Square it to make the falloff smoother/softer
                    alpha = alpha * alpha;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.Refresh();
            
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
#endif
