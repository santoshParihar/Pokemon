#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SceneSetupHelper
{
    [MenuItem("Pokemon TCG/Clear PlayerPrefs (Reset Collection)")]
    public static void ClearPlayerPrefs()
    {
        PlayerCollection.ClearAll();
    }

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
        }
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 5.0f;
        
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

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
        headerImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        headerImg.sprite = GetOrConvertSprite("Assets/Game Assets/Textures/pok.png");
        headerImg.color = Color.white;

        // App title
        GameObject titleObj = new GameObject("AppTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(headerObj.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.45f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = new Vector2(0f, 40f);
        titleRect.offsetMax = new Vector2(0f, 40f);

        TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
        titleTMP.text = "POKEMON CARDS";
        titleTMP.fontSize = 52;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.black;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.outlineColor = Color.white;
        titleTMP.outlineWidth = 0.2f;

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
        colBtnRect.sizeDelta = new Vector2(420, 110);
        Image colBtnImg = colBtnObj.GetComponent<Image>();
        colBtnImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        colBtnImg.color = Color.clear;

        // Dedicated underline childed directly to Collection tab button
        GameObject colUnderlineObj = new GameObject("Underline", typeof(RectTransform), typeof(Image));
        colUnderlineObj.transform.SetParent(colBtnObj.transform, false);
        RectTransform colUnderlineRect = colUnderlineObj.GetComponent<RectTransform>();
        colUnderlineRect.anchorMin = new Vector2(0f, 0f);
        colUnderlineRect.anchorMax = new Vector2(1f, 0f);
        colUnderlineRect.pivot = new Vector2(0.5f, 0f);
        colUnderlineRect.anchoredPosition = new Vector2(0, 0f); // Exactly at bottom edge of button
        colUnderlineRect.sizeDelta = new Vector2(0, 6f); // Span full button width, 6 height
        Image colUnderlineImg = colUnderlineObj.GetComponent<Image>();
        colUnderlineImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        colUnderlineImg.color = new Color(0.95f, 0.8f, 0.1f, 1f); // Vibrant Gold

        GameObject colTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        colTxtObj.transform.SetParent(colBtnObj.transform, false);
        RectTransform colTxtRect = colTxtObj.GetComponent<RectTransform>();
        colTxtRect.anchorMin = Vector2.zero;
        colTxtRect.anchorMax = Vector2.one;
        colTxtRect.offsetMin = Vector2.zero;
        colTxtRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI colTxtTMP = colTxtObj.GetComponent<TextMeshProUGUI>();
        colTxtTMP.text = "Collection";
        colTxtTMP.fontSize = 42;
        colTxtTMP.alignment = TextAlignmentOptions.Center;
        colTxtTMP.color = Color.black;
        colTxtTMP.fontStyle = FontStyles.Bold;

        Button colBtn = colBtnObj.GetComponent<Button>();

        // Store Tab
        GameObject storeBtnObj = new GameObject("StoreTabButton", typeof(RectTransform), typeof(Image), typeof(Button));
        storeBtnObj.transform.SetParent(tabRowObj.transform, false);
        RectTransform storeBtnRect = storeBtnObj.GetComponent<RectTransform>();
        storeBtnRect.sizeDelta = new Vector2(420, 110);
        Image storeBtnImg = storeBtnObj.GetComponent<Image>();
        storeBtnImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        storeBtnImg.color = Color.clear;

        // Dedicated underline childed directly to Store tab button
        GameObject storeUnderlineObj = new GameObject("Underline", typeof(RectTransform), typeof(Image));
        storeUnderlineObj.transform.SetParent(storeBtnObj.transform, false);
        RectTransform storeUnderlineRect = storeUnderlineObj.GetComponent<RectTransform>();
        storeUnderlineRect.anchorMin = new Vector2(0f, 0f);
        storeUnderlineRect.anchorMax = new Vector2(1f, 0f);
        storeUnderlineRect.pivot = new Vector2(0.5f, 0f);
        storeUnderlineRect.anchoredPosition = new Vector2(0, 0f);
        storeUnderlineRect.sizeDelta = new Vector2(0, 6f);
        Image storeUnderlineImg = storeUnderlineObj.GetComponent<Image>();
        storeUnderlineImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        storeUnderlineImg.color = new Color(0.95f, 0.8f, 0.1f, 1f); // Vibrant Gold

        GameObject storeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        storeTxtObj.transform.SetParent(storeBtnObj.transform, false);
        RectTransform storeTxtRect = storeTxtObj.GetComponent<RectTransform>();
        storeTxtRect.anchorMin = Vector2.zero;
        storeTxtRect.anchorMax = Vector2.one;
        storeTxtRect.offsetMin = Vector2.zero;
        storeTxtRect.offsetMax = Vector2.zero;

        TextMeshProUGUI storeTxtTMP = storeTxtObj.GetComponent<TextMeshProUGUI>();
        storeTxtTMP.text = "Store";
        storeTxtTMP.fontSize = 42;
        storeTxtTMP.alignment = TextAlignmentOptions.Center;
        storeTxtTMP.color = new Color(0.4f, 0.45f, 0.5f, 1f);
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
        storePanelImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        storePanelImg.sprite = GetOrConvertSprite("Assets/Game Assets/Textures/Background/Property 1=Normal.png");
        storePanelImg.color = new Color(1f, 1f, 1f, 0.5f);

        GameObject storeMsgObj = new GameObject("StoreMessage", typeof(RectTransform), typeof(TextMeshProUGUI));
        storeMsgObj.transform.SetParent(storePanelObj.transform, false);
        RectTransform storeMsgRect = storeMsgObj.GetComponent<RectTransform>();
        storeMsgRect.anchorMin = new Vector2(0, 0.4f);
        storeMsgRect.anchorMax = new Vector2(1, 0.6f);
        storeMsgRect.offsetMin = Vector2.zero;
        storeMsgRect.offsetMax = Vector2.zero;

        TextMeshProUGUI storeMsgTMP = storeMsgObj.GetComponent<TextMeshProUGUI>();
        storeMsgTMP.text = "<size=64><b>POKEMON TCG STORE</b></size>\n\n<size=40><color=#1A1D24>Booster Packs & Shop items arriving soon!</color></size>";
        storeMsgTMP.alignment = TextAlignmentOptions.Center;
        storeMsgTMP.color = new Color(0.08f, 0.10f, 0.14f, 1f);

        // 5. Collection Panel (2D Grid View ScrollRect)
        GameObject colPanelObj = new GameObject("CollectionPanel", typeof(RectTransform), typeof(Image));
        colPanelObj.transform.SetParent(canvas.transform, false);
        RectTransform colPanelRect = colPanelObj.GetComponent<RectTransform>();
        colPanelRect.anchorMin = new Vector2(0, 0);
        colPanelRect.anchorMax = new Vector2(1, 0.84f);
        colPanelRect.offsetMin = Vector2.zero;
        colPanelRect.offsetMax = Vector2.zero;

        Image colPanelImg = colPanelObj.GetComponent<Image>();
        colPanelImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        colPanelImg.sprite = null;
        colPanelImg.color = new Color(1f, 1f, 1f, 0.5f);

        // Create Search Input Field (Left 60% of width)
        GameObject searchBarObj = new GameObject("SearchInputField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        searchBarObj.transform.SetParent(colPanelObj.transform, false);
        RectTransform searchBarRect = searchBarObj.GetComponent<RectTransform>();
        searchBarRect.anchorMin = new Vector2(0f, 1f);
        searchBarRect.anchorMax = new Vector2(0.56f, 1f); // Reduced to 56% to leave a clear gap
        searchBarRect.pivot = new Vector2(0f, 1f);
        searchBarRect.anchoredPosition = new Vector2(20f, -10f); // 20px padding from left, 10px from top
        searchBarRect.sizeDelta = new Vector2(0f, 85f);

        Image searchBarImg = searchBarObj.GetComponent<Image>();
        searchBarImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        searchBarImg.type = Image.Type.Sliced;
        searchBarImg.color = new Color(0.12f, 0.15f, 0.22f, 1f);

        // Create Sort Dropdown (Right 36% of width, leaving a 4% gap for margin)
        GameObject sortDropdownObj = null;

        // Track existing children so we can identify the new dropdown
        List<GameObject> existingChildren = new List<GameObject>();
        for (int i = 0; i < colPanelObj.transform.childCount; i++)
        {
            existingChildren.Add(colPanelObj.transform.GetChild(i).gameObject);
        }

        // Find the TMPro CreateObjectMenu type using reflection across loaded Editor assemblies
        System.Type createMenuType = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName.Contains("Editor") && (assembly.FullName.Contains("TextMeshPro") || assembly.FullName.Contains("UGUI")))
            {
                createMenuType = assembly.GetType("TMPro.EditorUtilities.TMPro_CreateObjectMenu");
                if (createMenuType != null) break;
            }
        }

        if (createMenuType != null)
        {
            var addDropdownMethod = createMenuType.GetMethod("AddDropdown", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (addDropdownMethod != null)
            {
                // Temporarily select the parent panel so Unity nests the created dropdown under it
                GameObject oldSelection = Selection.activeGameObject;
                Selection.activeGameObject = colPanelObj;

                // Natively invoke the standard TMPro dropdown menu creation logic
                addDropdownMethod.Invoke(null, new object[] { new MenuCommand(colPanelObj, 0) });

                Selection.activeGameObject = oldSelection;
            }
        }

        // Identify the newly created dropdown object
        GameObject newChild = null;
        for (int i = 0; i < colPanelObj.transform.childCount; i++)
        {
            GameObject child = colPanelObj.transform.GetChild(i).gameObject;
            if (!existingChildren.Contains(child))
            {
                newChild = child;
                break;
            }
        }

        if (newChild == null)
        {
            Debug.LogError("[SceneSetupHelper] Failed to create TMP Dropdown natively via TMPro_CreateObjectMenu.");
            return;
        }

        sortDropdownObj = newChild;
        sortDropdownObj.name = "SortDropdown";

        TMP_Dropdown dropdownComponent = sortDropdownObj.GetComponent<TMP_Dropdown>();
        RectTransform templateRt = dropdownComponent.template;
        
        // Find TMP text references inside the natively created Dropdown hierarchy
        Transform labelTrans = sortDropdownObj.transform.Find("Label");
        Transform itemLabelTrans = sortDropdownObj.transform.Find("Template/Viewport/Content/Item/Item Label");
        TextMeshProUGUI captionTextTmp = labelTrans != null ? labelTrans.GetComponent<TextMeshProUGUI>() : null;
        TextMeshProUGUI itemTextTmp = itemLabelTrans != null ? itemLabelTrans.GetComponent<TextMeshProUGUI>() : null;

        // FIX 1: Canvas override sorting — forces dropdown list to render on top of all other UI
        if (templateRt != null)
        {
            Canvas templateCanvas = templateRt.gameObject.GetComponent<Canvas>();
            if (templateCanvas == null)
                templateCanvas = templateRt.gameObject.AddComponent<Canvas>();
            templateCanvas.overrideSorting = true;
            templateCanvas.sortingOrder = 100;

            if (templateRt.gameObject.GetComponent<GraphicRaycaster>() == null)
                templateRt.gameObject.AddComponent<GraphicRaycaster>();

            // FIX 2: Position Template to open BELOW the dropdown button
            templateRt.anchorMin = new Vector2(0f, 0f);
            templateRt.anchorMax = new Vector2(1f, 0f);
            templateRt.pivot = new Vector2(0.5f, 1f);
            templateRt.anchoredPosition = new Vector2(0f, -15f);
            templateRt.sizeDelta = new Vector2(0f, 380f); // 4 items × 95px each

            templateRt.gameObject.SetActive(false);

            // FIX 3: Replace Mask with RectMask2D on the Viewport
            Transform viewportTrans = templateRt.Find("Viewport");
            if (viewportTrans != null)
            {
                Mask oldMask = viewportTrans.GetComponent<Mask>();
                if (oldMask != null) Object.DestroyImmediate(oldMask);

                Image maskImage = viewportTrans.GetComponent<Image>();
                if (maskImage != null) Object.DestroyImmediate(maskImage);

                if (viewportTrans.GetComponent<RectMask2D>() == null)
                    viewportTrans.gameObject.AddComponent<RectMask2D>();
            }
        }

        // Configure options
        if (dropdownComponent != null)
        {
            dropdownComponent.options.Clear();
            dropdownComponent.options.Add(new TMP_Dropdown.OptionData("Name: A-Z"));
            dropdownComponent.options.Add(new TMP_Dropdown.OptionData("Name: Z-A"));
            dropdownComponent.options.Add(new TMP_Dropdown.OptionData("Price: Low to High"));
            dropdownComponent.options.Add(new TMP_Dropdown.OptionData("Price: High to Low"));
        }

        // Adjust the height of the item template row inside the template so it doesn't overlap
        Transform itemTrans = sortDropdownObj.transform.Find("Template/Viewport/Content/Item");
        if (itemTrans != null)
        {
            RectTransform rectTransComp = itemTrans.GetComponent<RectTransform>();
            if (rectTransComp != null)
                rectTransComp.sizeDelta = new Vector2(rectTransComp.sizeDelta.x, 95f);
        }

        if (sortDropdownObj != null)
        {
            RectTransform sortDropdownRect = sortDropdownObj.GetComponent<RectTransform>();
            sortDropdownRect.anchorMin = new Vector2(0.60f, 1f); // Starts at 60% for a wider dropdown button
            sortDropdownRect.anchorMax = new Vector2(1f, 1f);
            sortDropdownRect.pivot = new Vector2(1f, 1f);
            sortDropdownRect.anchoredPosition = new Vector2(-20f, -10f); // 20px padding from right, 10px from top
            sortDropdownRect.sizeDelta = new Vector2(0f, 85f); // Match the search bar height (85f)

            // Style the dropdown image background
            Image dropdownImg = sortDropdownObj.GetComponent<Image>();
            if (dropdownImg != null)
            {
                dropdownImg.color = new Color(0.12f, 0.15f, 0.22f, 1f); // Same dark slate background
            }

            // Find a valid font to apply
            TMP_FontAsset defaultFont = titleTMP != null ? titleTMP.font : null;
            if (defaultFont == null)
            {
                string[] fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset");
                if (fontGuids.Length > 0)
                {
                    defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(fontGuids[0]));
                }
            }

            if (captionTextTmp != null)
            {
                if (defaultFont != null) captionTextTmp.font = defaultFont;
                captionTextTmp.color = Color.white;
                captionTextTmp.fontSize = 32; // Increased size
                captionTextTmp.alignment = TextAlignmentOptions.MidlineLeft;
            }
            if (itemTextTmp != null)
            {
                if (defaultFont != null) itemTextTmp.font = defaultFont;
                itemTextTmp.color = Color.white; // Make option text white for readability on dark background
                itemTextTmp.fontSize = 34; // Large — easy to read and tap
                itemTextTmp.alignment = TextAlignmentOptions.MidlineLeft;
            }

            // Style the opened list (Template) background to match the dark theme
            if (templateRt != null)
            {
                Image templateBg = templateRt.GetComponent<Image>();
                if (templateBg != null)
                {
                    templateBg.color = new Color(0.12f, 0.15f, 0.22f, 1f); // Premium dark slate background
                }

                // Style the individual option item background hover color
                Transform itemBgTrans = templateRt.Find("Viewport/Content/Item/Item Background");
                if (itemBgTrans != null)
                {
                    Image itemBgImg = itemBgTrans.GetComponent<Image>();
                    if (itemBgImg != null)
                    {
                        itemBgImg.color = new Color(0.18f, 0.22f, 0.30f, 1f); // Slightly lighter slate for hover states
                    }
                }
            }
        }

        TMP_InputField inputComponent = searchBarObj.GetComponent<TMP_InputField>();

        // Create TextArea
        GameObject textAreaObj = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
        textAreaObj.transform.SetParent(searchBarObj.transform, false);
        RectTransform textAreaRect = textAreaObj.GetComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(20, 10);
        textAreaRect.offsetMax = new Vector2(-20, -10);

        // Create Placeholder Text
        GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        placeholderObj.transform.SetParent(textAreaObj.transform, false);
        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;

        TextMeshProUGUI placeholderTMP = placeholderObj.GetComponent<TextMeshProUGUI>();
        placeholderTMP.text = "Search by Pokemon name...";
        placeholderTMP.fontSize = 28;
        placeholderTMP.fontStyle = FontStyles.Italic;
        placeholderTMP.color = new Color(0.5f, 0.55f, 0.65f, 1f);
        placeholderTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Create Input Text
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(textAreaObj.transform, false);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI textTMP = textObj.GetComponent<TextMeshProUGUI>();
        textTMP.fontSize = 28;
        textTMP.color = Color.white;
        textTMP.alignment = TextAlignmentOptions.MidlineLeft;

        inputComponent.textViewport = textAreaRect;
        inputComponent.textComponent = textTMP;
        inputComponent.placeholder = placeholderTMP;

        // ScrollView Object (shifted down to make space for search bar)
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(colPanelObj.transform, false);
        RectTransform scrollRectTrans = scrollObj.GetComponent<RectTransform>();
        scrollRectTrans.anchorMin = Vector2.zero;
        scrollRectTrans.anchorMax = Vector2.one;
        scrollRectTrans.offsetMin = new Vector2(20, 20);
        scrollRectTrans.offsetMax = new Vector2(-20, -105);

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

        // Move the SearchInputField and SortDropdown to render on top of the ScrollView so scrolled cards pass beneath them
        searchBarObj.transform.SetAsLastSibling();
        if (sortDropdownObj != null)
        {
            sortDropdownObj.transform.SetAsLastSibling();
        }

        // 6. Generate/Save the 2D Card Prefab
        GameObject card2DPrefabAsset = GetOrCreate2DCardPrefab();

        // Move Header Panel to render on top of Collection and Store Panels so scrolled cards go beneath it
        headerObj.transform.SetAsLastSibling();

        // 7. Setup Detailed Inspect Overlay
        GameObject overlayObj = new GameObject("InspectOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
        overlayObj.transform.SetParent(canvas.transform, false);
        RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImg = overlayObj.GetComponent<Image>();
        overlayImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        overlayImg.color = new Color(0.04f, 0.05f, 0.07f, 0.92f); // High-premium extra-dark glass background

        Button overlayBgBtn = overlayObj.GetComponent<Button>();
 
        // Create 3D inspect anchor right in front of camera
        GameObject inspectAnchorObj = GameObject.Find("Inspect3DAnchor");
        if (inspectAnchorObj == null)
        {
            inspectAnchorObj = new GameObject("Inspect3DAnchor");
        }
        inspectAnchorObj.transform.position = new Vector3(0f, 0.4f, -2.0f); // 2.5 units in front of Camera
        inspectAnchorObj.transform.rotation = Quaternion.identity;

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(overlayObj.transform, false);
        RectTransform closeBtnRect = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(0.5f, 0f);
        closeBtnRect.anchorMax = new Vector2(0.5f, 0f);
        closeBtnRect.anchoredPosition = new Vector2(0, 100);
        closeBtnRect.sizeDelta = new Vector2(250, 70);

        Image closeImg = closeBtnObj.GetComponent<Image>();
        closeImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
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
        var inspect3DAnchorField = typeof(MainUIManager).GetField("inspect3DAnchor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var card3DPrefabsField = typeof(MainUIManager).GetField("card3DPrefabs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var inspectOverlayCloseButtonField = typeof(MainUIManager).GetField("inspectOverlayCloseButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var searchInputFieldField = typeof(MainUIManager).GetField("searchInputField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var sortDropdownField = typeof(MainUIManager).GetField("sortDropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
 
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
        inspect3DAnchorField?.SetValue(uiManager, inspectAnchorObj.transform);
        inspectOverlayCloseButtonField?.SetValue(uiManager, closeBtn);
        searchInputFieldField?.SetValue(uiManager, inputComponent);
        sortDropdownField?.SetValue(uiManager, dropdownComponent);

        // Attach and wire up CollectionSearchField script dynamically
        CollectionSearchField searchField = searchBarObj.GetComponent<CollectionSearchField>();
        if (searchField == null) searchField = searchBarObj.AddComponent<CollectionSearchField>();
        
        var sfInputField = typeof(CollectionSearchField).GetField("inputField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var sfGridContainer = typeof(CollectionSearchField).GetField("gridContentContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        sfInputField?.SetValue(searchField, inputComponent);
        sfGridContainer?.SetValue(searchField, contentRect);

        // Populate card3DPrefabs list from Assets automatically
        if (card3DPrefabsField != null)
        {
            List<GameObject> prefabsList = new List<GameObject>();
            string[] prefabs = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Game Assets/Prefabs" });
            foreach (var guid in prefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null && go.GetComponent<CardUIController>() != null)
                {
                    prefabsList.Add(go);
                }
            }
            card3DPrefabsField.SetValue(uiManager, prefabsList);
        }

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
            if (img.gameObject.name != "PackArtImage" && img.gameObject.name != "PackShimmerOverlay")
            {
                img.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            }
        }
        #endif

        uiManager.SwitchToTab(MainUIManager.Tab.Collection);
        EditorUtility.SetDirty(uiManager);
        Selection.activeGameObject = uiManagerObj;

        // Automatically setup the pack opening system to wire up the store and avoid empty screens
        SetupPackOpeningUI();

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
        bgImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
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
        typeBadgeImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
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

        // Price Text under HP
        GameObject priceTextObj = new GameObject("PriceText", typeof(RectTransform), typeof(TextMeshProUGUI));
        priceTextObj.transform.SetParent(headerObj.transform, false);
        RectTransform priceTextRect = priceTextObj.GetComponent<RectTransform>();
        priceTextRect.anchorMin = new Vector2(0.7f, 0f);
        priceTextRect.anchorMax = new Vector2(1f, 0.35f);
        priceTextRect.offsetMin = Vector2.zero;
        priceTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI priceTMP = priceTextObj.GetComponent<TextMeshProUGUI>();
        priceTMP.text = "$1.99";
        priceTMP.fontSize = 38; // Size 38 to match the 3D card layout perfectly!
        priceTMP.alignment = TextAlignmentOptions.MidlineRight;
        priceTMP.color = new Color(0.9f, 0.22f, 0.27f, 1f); // Accent crimson
        priceTMP.fontStyle = FontStyles.Bold;

        // Artwork Panel
        GameObject artObj = new GameObject("ArtworkPanel", typeof(RectTransform), typeof(Image));
        artObj.transform.SetParent(root.transform, false);
        RectTransform artRect = artObj.GetComponent<RectTransform>();
        artRect.anchorMin = new Vector2(0.5f, 0.5f);
        artRect.anchorMax = new Vector2(0.5f, 0.5f);
        artRect.anchoredPosition = new Vector3(0, 170, 0);
        artRect.sizeDelta = new Vector2(610, 340);
        
        Image artImg = artObj.GetComponent<Image>();
        artImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
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
            img.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
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
                iconImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
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
            img.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
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
        var priceTextF = typeof(Card2DUIController).GetField("priceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pokemonImageF = typeof(Card2DUIController).GetField("pokemonImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        nameTextF?.SetValue(controller, nameTMP);
        hpTextF?.SetValue(controller, hpTMP);
        typeTextF?.SetValue(controller, typeTMP);
        pokedexClassTextF?.SetValue(controller, pokedexClassTMP);
        priceTextF?.SetValue(controller, priceTMP);
        pokemonImageF?.SetValue(controller, artImg);

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
            img.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
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

    // ═════════════════════════════════════════════════════════════════════════
    //  Pack Opening UI Setup
    // ═════════════════════════════════════════════════════════════════════════

    public static void SetupPackOpeningUI()
    {
        // ── Find / require Canvas ────────────────────────────────────────────
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[PackOpeningSetup] No Canvas found. Run 'Setup Main Scene UI' first.");
            return;
        }

        // ── Destroy any existing pack overlay so we start clean ──────────────
        Transform oldOverlay = canvas.transform.Find("PackOverlayPanel");
        if (oldOverlay != null) Object.DestroyImmediate(oldOverlay.gameObject);

        // Replace the placeholder StorePanel content ─────────────────────────
        Transform storePanelTf = canvas.transform.Find("StorePanel");
        if (storePanelTf == null)
        {
            Debug.LogError("[PackOpeningSetup] StorePanel not found. Run 'Setup Main Scene UI' first.");
            return;
        }
        GameObject storePanelObj = storePanelTf.gameObject;

        // Clear previous store content
        List<Transform> storeChildren = new List<Transform>();
        for (int i = 0; i < storePanelObj.transform.childCount; i++)
            storeChildren.Add(storePanelObj.transform.GetChild(i));
        foreach (var child in storeChildren) Object.DestroyImmediate(child.gameObject);

        // ────────────────────────────────────────────────────────────────────
        //  STORE PANEL CONTENT
        // ────────────────────────────────────────────────────────────────────

        // Pack art image (centred, takes up most of the panel height)
        Sprite packArtSprite = GetOrCreatePackArtSprite();

        GameObject packArtObj = new GameObject("PackArtImage", typeof(RectTransform), typeof(Image), typeof(Mask));
        packArtObj.transform.SetParent(storePanelObj.transform, false);
        RectTransform packArtRt = packArtObj.GetComponent<RectTransform>();
        packArtRt.anchorMin = new Vector2(0.5f, 0.5f);
        packArtRt.anchorMax = new Vector2(0.5f, 0.5f);
        packArtRt.pivot     = new Vector2(0.5f, 0.5f);
        packArtRt.anchoredPosition = new Vector2(0f, 120f);
        packArtRt.sizeDelta        = new Vector2(647.3f, 1024f);
        Image packArtImg = packArtObj.GetComponent<Image>();
        packArtImg.material = null; // Use default UI material to support Stencil Masking
        packArtImg.sprite = packArtSprite;
        packArtImg.preserveAspect = true;

        Mask packMask = packArtObj.GetComponent<Mask>();
        packMask.showMaskGraphic = true;

        // Shine beam overlay (child of pack art)
        GameObject shimmerObj = new GameObject("PackShimmerOverlay", typeof(RectTransform), typeof(Image));
        shimmerObj.transform.SetParent(packArtObj.transform, false);
        RectTransform shimmerRt = shimmerObj.GetComponent<RectTransform>();
        shimmerRt.anchorMin = new Vector2(0.5f, 0.5f);
        shimmerRt.anchorMax = new Vector2(0.5f, 0.5f);
        shimmerRt.pivot     = new Vector2(0.5f, 0.5f);
        shimmerRt.anchoredPosition = new Vector2(-800f, 0f);
        shimmerRt.sizeDelta        = new Vector2(140f, 1600f);
        shimmerRt.localRotation    = Quaternion.Euler(0f, 0f, -35f);
        Image shimmerImg = shimmerObj.GetComponent<Image>();
        shimmerImg.material = null; // Use default UI material to support Stencil Masking
        shimmerImg.sprite = GetOrCreateShineBeamSprite();
        shimmerImg.color = new Color(1f, 1f, 1f, 0.75f);

        // Pack name label
        GameObject packNameObj = new GameObject("PackNameLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        packNameObj.transform.SetParent(storePanelObj.transform, false);
        RectTransform packNameRt = packNameObj.GetComponent<RectTransform>();
        packNameRt.anchorMin = new Vector2(0.1f, 0.88f);
        packNameRt.anchorMax = new Vector2(0.9f, 0.98f);
        packNameRt.offsetMin = packNameRt.offsetMax = Vector2.zero;
        TextMeshProUGUI packNameTMP = packNameObj.GetComponent<TextMeshProUGUI>();
        packNameTMP.text      = "<color=black><b>KANTO STARTER PACK</b></color>";
        packNameTMP.fontSize  = 54;
        packNameTMP.fontStyle = FontStyles.Bold;
        packNameTMP.alignment = TextAlignmentOptions.Center;
        packNameTMP.color     = Color.black;

        // Open pack button
        Sprite badgeSprite = GetOrCreateBadgeSprite();
        GameObject openBtnObj = new GameObject("OpenPackButton", typeof(RectTransform), typeof(Image), typeof(Button));
        openBtnObj.transform.SetParent(storePanelObj.transform, false);
        RectTransform openBtnRt = openBtnObj.GetComponent<RectTransform>();
        openBtnRt.anchorMin = new Vector2(0.15f, 0.13f);
        openBtnRt.anchorMax = new Vector2(0.85f, 0.21f);
        openBtnRt.offsetMin = openBtnRt.offsetMax = Vector2.zero;
        Image openBtnImg = openBtnObj.GetComponent<Image>();
        openBtnImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        openBtnImg.sprite = badgeSprite;
        openBtnImg.type   = Image.Type.Sliced;
        openBtnImg.color  = new Color(0.95f, 0.75f, 0.1f, 1f); // Gold

        GameObject openBtnTxtObj = new GameObject("ButtonText", typeof(RectTransform), typeof(TextMeshProUGUI));
        openBtnTxtObj.transform.SetParent(openBtnObj.transform, false);
        RectTransform openBtnTxtRt = openBtnTxtObj.GetComponent<RectTransform>();
        openBtnTxtRt.anchorMin = Vector2.zero;
        openBtnTxtRt.anchorMax = Vector2.one;
        openBtnTxtRt.offsetMin = openBtnTxtRt.offsetMax = Vector2.zero;
        TextMeshProUGUI openBtnTMP = openBtnTxtObj.GetComponent<TextMeshProUGUI>();
        openBtnTMP.text      = "✨  Open Free Pack";
        openBtnTMP.fontSize  = 36;
        openBtnTMP.fontStyle = FontStyles.Bold;
        openBtnTMP.alignment = TextAlignmentOptions.Center;
        openBtnTMP.color     = new Color(0.1f, 0.08f, 0.02f, 1f);
        Navigation noneNav = new Navigation { mode = Navigation.Mode.None };
        openBtnObj.GetComponent<Button>().navigation = noneNav;

        // Cooldown timer label
        GameObject cooldownObj = new GameObject("CooldownTimerLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        cooldownObj.transform.SetParent(storePanelObj.transform, false);
        RectTransform cooldownRt = cooldownObj.GetComponent<RectTransform>();
        cooldownRt.anchorMin = new Vector2(0.1f, 0.07f);
        cooldownRt.anchorMax = new Vector2(0.9f, 0.13f);
        cooldownRt.offsetMin = cooldownRt.offsetMax = Vector2.zero;
        TextMeshProUGUI cooldownTMP = cooldownObj.GetComponent<TextMeshProUGUI>();
        cooldownTMP.text      = "<color=black><b>Pack ready!</b></color>";
        cooldownTMP.fontSize  = 48;
        cooldownTMP.alignment = TextAlignmentOptions.Center;
        cooldownTMP.color     = Color.black;
        cooldownTMP.fontStyle = FontStyles.Bold;

        // Empty collection hint (shown when player has no cards yet)
        GameObject emptyHintObj = new GameObject("EmptyCollectionHint", typeof(RectTransform), typeof(TextMeshProUGUI));
        // This belongs on CollectionPanel, not StorePanel
        Transform colPanelTf = canvas.transform.Find("CollectionPanel");
        if (colPanelTf != null)
        {
            emptyHintObj.transform.SetParent(colPanelTf, false);
        }
        else
        {
            emptyHintObj.transform.SetParent(storePanelObj.transform, false);
        }
        RectTransform emptyHintRt = emptyHintObj.GetComponent<RectTransform>();
        emptyHintRt.anchorMin = new Vector2(0.05f, 0.4f);
        emptyHintRt.anchorMax = new Vector2(0.95f, 0.6f);
        emptyHintRt.offsetMin = emptyHintRt.offsetMax = Vector2.zero;
        TextMeshProUGUI emptyHintTMP = emptyHintObj.GetComponent<TextMeshProUGUI>();
        emptyHintTMP.text      = "Open a pack in the <b>Store</b> tab to get your first cards!";
        emptyHintTMP.fontSize  = 44;
        emptyHintTMP.alignment = TextAlignmentOptions.Center;
        emptyHintTMP.color     = new Color(0.08f, 0.10f, 0.14f, 1f);
        emptyHintTMP.enableWordWrapping = true;
        emptyHintObj.SetActive(false); // hidden until runtime logic enables it

        // ────────────────────────────────────────────────────────────────────
        //  PACK OVERLAY PANEL  (sort order above everything else)
        // ────────────────────────────────────────────────────────────────────

        GameObject overlayPanelObj = new GameObject("PackOverlayPanel",
            typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasGroup));
        overlayPanelObj.transform.SetParent(canvas.transform, false);

        // Own Canvas so we can set sort order high
        Canvas overlayCanvas = overlayPanelObj.GetComponent<Canvas>();
        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder    = 50;

        CanvasGroup overlayCG = overlayPanelObj.GetComponent<CanvasGroup>();
        overlayCG.alpha            = 0f;
        overlayCG.interactable     = false;
        overlayCG.blocksRaycasts   = false;

        RectTransform overlayRt = overlayPanelObj.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = overlayRt.offsetMax = Vector2.zero;

        // Dark background
        GameObject darkBgObj = new GameObject("DarkBackground", typeof(RectTransform), typeof(Image));
        darkBgObj.transform.SetParent(overlayPanelObj.transform, false);
        RectTransform darkBgRt = darkBgObj.GetComponent<RectTransform>();
        darkBgRt.anchorMin = Vector2.zero;
        darkBgRt.anchorMax = Vector2.one;
        darkBgRt.offsetMin = darkBgRt.offsetMax = Vector2.zero;
        Image darkBgImg = darkBgObj.GetComponent<Image>();
        darkBgImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        darkBgImg.color = new Color(0.04f, 0.04f, 0.06f, 0.95f);

        // Pack rip image (the booster art shown in overlay centre)
        GameObject packRipObj = new GameObject("PackRipImage", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        packRipObj.transform.SetParent(overlayPanelObj.transform, false);
        RectTransform packRipRt = packRipObj.GetComponent<RectTransform>();
        packRipRt.anchorMin = new Vector2(0.5f, 0.5f);
        packRipRt.anchorMax = new Vector2(0.5f, 0.5f);
        packRipRt.pivot     = new Vector2(0.5f, 0.5f);
        packRipRt.anchoredPosition = new Vector2(0f, -33.6f);
        packRipRt.sizeDelta        = new Vector2(647.3f, 1024f);
        Image packRipImgComp = packRipObj.GetComponent<Image>();
        packRipImgComp.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        packRipImgComp.sprite         = packArtSprite;
        packRipImgComp.preserveAspect = true;
        CanvasGroup packRipCGComp = packRipObj.GetComponent<CanvasGroup>();
        packRipCGComp.alpha = 1f;
        packRipObj.SetActive(false); // Hidden until ceremony starts

        // Card reveal container (centred, above mid screen)
        GameObject cardRevealObj = new GameObject("CardRevealContainer", typeof(RectTransform));
        cardRevealObj.transform.SetParent(overlayPanelObj.transform, false);
        RectTransform cardRevealRt = cardRevealObj.GetComponent<RectTransform>();
        cardRevealRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRevealRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRevealRt.pivot     = new Vector2(0.5f, 0.5f);
        cardRevealRt.anchoredPosition = new Vector2(0f, 60f);
        cardRevealRt.sizeDelta        = new Vector2(1000f, 700f);

        // Glow burst (fullscreen flash, normally hidden)
        GameObject glowObj = new GameObject("GlowBurstImage", typeof(RectTransform), typeof(Image));
        glowObj.transform.SetParent(overlayPanelObj.transform, false);
        RectTransform glowRt = glowObj.GetComponent<RectTransform>();
        glowRt.anchorMin = Vector2.zero;
        glowRt.anchorMax = Vector2.one;
        glowRt.offsetMin = glowRt.offsetMax = Vector2.zero;
        Image glowImg = glowObj.GetComponent<Image>();
        glowImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        glowImg.color = new Color(1f, 1f, 1f, 0f);
        glowObj.SetActive(false);

        // Summary panel
        GameObject summaryObj = new GameObject("SummaryPanel", typeof(RectTransform), typeof(Image));
        summaryObj.transform.SetParent(overlayPanelObj.transform, false);
        RectTransform summaryRt = summaryObj.GetComponent<RectTransform>();
        summaryRt.anchorMin = new Vector2(0.15f, 0.22f);
        summaryRt.anchorMax = new Vector2(0.85f, 0.74f);
        summaryRt.offsetMin = summaryRt.offsetMax = Vector2.zero;
        Image summaryBg = summaryObj.GetComponent<Image>();
        summaryBg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        summaryBg.sprite = badgeSprite;
        summaryBg.type   = Image.Type.Sliced;
        summaryBg.color  = new Color(0.06f, 0.08f, 0.12f, 0.98f); // Deep dark slate background

        // Gold border outline
        GameObject borderObj = new GameObject("Border", typeof(RectTransform), typeof(Image));
        borderObj.transform.SetParent(summaryObj.transform, false);
        RectTransform borderRt = borderObj.GetComponent<RectTransform>();
        borderRt.anchorMin = Vector2.zero;
        borderRt.anchorMax = Vector2.one;
        borderRt.offsetMin = new Vector2(-4, -4);
        borderRt.offsetMax = new Vector2(4, 4);
        Image borderImg = borderObj.GetComponent<Image>();
        borderImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        borderImg.sprite = badgeSprite;
        borderImg.type = Image.Type.Sliced;
        borderImg.color = new Color(0.72f, 0.58f, 0.36f, 0.8f); // Sleek gold outline
        borderObj.transform.SetAsFirstSibling();

        // Header Title
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(summaryObj.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 0.82f);
        titleRt.anchorMax = new Vector2(1f, 0.96f);
        titleRt.offsetMin = titleRt.offsetMax = Vector2.zero;
        TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
        titleTMP.text = "PACK RESULTS";
        titleTMP.fontSize = 42;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(0.95f, 0.75f, 0.25f, 1f); // Rich gold color

        // Dark List Inset Area
        GameObject listBoxObj = new GameObject("ListBox", typeof(RectTransform), typeof(Image));
        listBoxObj.transform.SetParent(summaryObj.transform, false);
        RectTransform listBoxRt = listBoxObj.GetComponent<RectTransform>();
        listBoxRt.anchorMin = new Vector2(0.08f, 0.28f);
        listBoxRt.anchorMax = new Vector2(0.92f, 0.78f);
        listBoxRt.offsetMin = listBoxRt.offsetMax = Vector2.zero;
        Image listBoxImg = listBoxObj.GetComponent<Image>();
        listBoxImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        listBoxImg.sprite = badgeSprite;
        listBoxImg.type = Image.Type.Sliced;
        listBoxImg.color = new Color(0.03f, 0.04f, 0.06f, 0.65f); // Inset background

        // Card Rows Container (VerticalLayoutGroup) inside ListBox — rows are spawned at runtime
        GameObject cardContainerObj = new GameObject("CardContainer", typeof(RectTransform));
        cardContainerObj.transform.SetParent(listBoxObj.transform, false);
        RectTransform cardContainerRt = cardContainerObj.GetComponent<RectTransform>();
        cardContainerRt.anchorMin = Vector2.zero;
        cardContainerRt.anchorMax = Vector2.one;
        cardContainerRt.offsetMin = new Vector2(8f, 6f);
        cardContainerRt.offsetMax = new Vector2(-8f, -6f);
        VerticalLayoutGroup vlg = cardContainerObj.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment         = TextAnchor.UpperLeft;
        vlg.spacing                = 4f;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;  // MUST be false — heights come from LayoutElement
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = false;  // MUST be false — LayoutElement.preferredHeight is used

        // Keep a hidden SummaryLabel for backward-compat (text cleared at runtime)
        GameObject summaryLabelObj = new GameObject("SummaryLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        summaryLabelObj.transform.SetParent(listBoxObj.transform, false);
        summaryLabelObj.SetActive(false);
        TextMeshProUGUI summaryTMP = summaryLabelObj.GetComponent<TextMeshProUGUI>();
        summaryTMP.text = "";

        // Add to Collection button
        GameObject addBtnObj = new GameObject("AddToCollectionButton", typeof(RectTransform), typeof(Image), typeof(Button));
        addBtnObj.transform.SetParent(summaryObj.transform, false);
        RectTransform addBtnRt = addBtnObj.GetComponent<RectTransform>();
        addBtnRt.anchorMin = new Vector2(0.18f, 0.06f);
        addBtnRt.anchorMax = new Vector2(0.82f, 0.20f);
        addBtnRt.offsetMin = addBtnRt.offsetMax = Vector2.zero;
        Image addBtnImg = addBtnObj.GetComponent<Image>();
        addBtnImg.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        addBtnImg.sprite = badgeSprite;
        addBtnImg.type   = Image.Type.Sliced;
        addBtnImg.color  = new Color(0.1f, 0.75f, 0.4f, 1f); // Premium emerald green
        addBtnObj.GetComponent<Button>().navigation = noneNav;

        GameObject addBtnTxtObj = new GameObject("ButtonText", typeof(RectTransform), typeof(TextMeshProUGUI));
        addBtnTxtObj.transform.SetParent(addBtnObj.transform, false);
        RectTransform addBtnTxtRt = addBtnTxtObj.GetComponent<RectTransform>();
        addBtnTxtRt.anchorMin = Vector2.zero;
        addBtnTxtRt.anchorMax = Vector2.one;
        addBtnTxtRt.offsetMin = addBtnTxtRt.offsetMax = Vector2.zero;
        TextMeshProUGUI addBtnTMP = addBtnTxtObj.GetComponent<TextMeshProUGUI>();
        addBtnTMP.text      = "Add to Collection →";
        addBtnTMP.fontSize  = 34;
        addBtnTMP.fontStyle = FontStyles.Bold;
        addBtnTMP.alignment = TextAlignmentOptions.Center;
        addBtnTMP.color     = Color.white;

        summaryObj.SetActive(false); // Hidden until reveal is done

        // Fix materials
        foreach (var img in overlayPanelObj.GetComponentsInChildren<Image>(true))
            img.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        foreach (var img in storePanelObj.GetComponentsInChildren<Image>(true))
        {
            if (img.gameObject.name != "PackArtImage" && img.gameObject.name != "PackShimmerOverlay")
            {
                img.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            }
        }

        // ────────────────────────────────────────────────────────────────────
        //  WIRE UP PackOpeningController
        // ────────────────────────────────────────────────────────────────────

        GameObject uiManagerObj = GameObject.Find("MainUIManager");
        if (uiManagerObj == null) uiManagerObj = new GameObject("MainUIManager");

        PackOpeningController poc = uiManagerObj.GetComponent<PackOpeningController>();
        if (poc == null) poc = uiManagerObj.AddComponent<PackOpeningController>();

        System.Type t = typeof(PackOpeningController);
        var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        void Set(string fieldName, object value)
        {
            var f = t.GetField(fieldName, bf);
            if (f != null) f.SetValue(poc, value);
            else Debug.LogWarning($"[PackOpeningSetup] Field not found: {fieldName}");
        }

        Set("storeRootPanel",      storePanelObj);
        Set("packArtImage",        packArtImg);
        Set("packNameLabel",       packNameTMP);
        Set("openPackButton",      openBtnObj.GetComponent<Button>());
        Set("openPackButtonText",  openBtnTMP);
        Set("cooldownTimerLabel",  cooldownTMP);
        Set("packShimmerOverlay",  shimmerImg);
        Set("packOverlayPanel",    overlayPanelObj);
        Set("packOverlayCG",       overlayCG);
        Set("packRipRect",         packRipRt);
        Set("packRipCG",           packRipCGComp);
        Set("cardRevealContainer", cardRevealRt);
        Set("glowBurstImage",      glowObj.GetComponent<Image>());
        Set("summaryPanel",        summaryObj);
        Set("addToCollectionButton", addBtnObj.GetComponent<Button>());
        Set("summaryLabel",        summaryTMP);
        Set("summaryCardContainer", cardContainerObj.transform);

        // Populate masterCardPool from Assets
        var poolField = t.GetField("masterCardPool", bf);
        if (poolField != null)
        {
            List<PokemonCardData> pool = new List<PokemonCardData>();
            string[] guids = AssetDatabase.FindAssets("t:PokemonCardData", new[] { "Assets/Game Assets/Data" });
            foreach (var guid in guids)
            {
                PokemonCardData d = AssetDatabase.LoadAssetAtPath<PokemonCardData>(AssetDatabase.GUIDToAssetPath(guid));
                if (d != null) pool.Add(d);
            }
            poolField.SetValue(poc, pool);
        }

        // Link MainUIManager back reference
        MainUIManager uiMgr = uiManagerObj.GetComponent<MainUIManager>();
        Set("mainUIManager", uiMgr);

        // Wire CharizardCard as the 3D reveal prefab
        string charizardPath = "Assets/Game Assets/Prefabs/CharizardCard.prefab";
        GameObject charizardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(charizardPath);
        Set("card3DRevealPrefab", charizardPrefab);

        // Wire EmptyCollectionHint to MainUIManager
        if (uiMgr != null && emptyHintObj != null)
        {
            var emptyField = typeof(MainUIManager).GetField("emptyCollectionHint",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            emptyField?.SetValue(uiMgr, emptyHintObj);
        }

        EditorUtility.SetDirty(poc);
        if (uiMgr != null) EditorUtility.SetDirty(uiMgr);
        Selection.activeGameObject = overlayPanelObj;

        Debug.Log("[PackOpeningSetup] Pack Opening UI built and wired successfully! ✅");
    }

    private static Sprite GetOrCreateShineBeamSprite()
    {
        string path = "Assets/Game Assets/Textures/shine_beam.png";
        
        int w = 256;
        int h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float normalizedX = (float)x / w;
                
                // Double-peak metallic gloss highlight:
                float dist1 = Mathf.Abs(normalizedX - 0.5f) * 6.5f;
                float peak1 = Mathf.Exp(-dist1 * dist1); // Main bright peak
                
                float dist2 = Mathf.Abs(normalizedX - 0.64f) * 14.0f;
                float peak2 = Mathf.Exp(-dist2 * dist2) * 0.45f; // Sharp secondary highlight
                
                float alpha = Mathf.Clamp01(peak1 + peak2) * 0.95f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        
        System.IO.Directory.CreateDirectory("Assets/Game Assets/Textures");
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.Refresh();
        
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ── Booster pack art texture generator ───────────────────────────────────

    private static Sprite GetOrCreatePackArtSprite()
    {
        string path = "Assets/Game Assets/Textures/pack.png";
        Sprite packSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (packSprite != null) return packSprite;

        path = "Assets/Game Assets/Textures/booster_pack_art.png";
        Texture2D exist = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (exist != null) return AssetDatabase.LoadAssetAtPath<Sprite>(path);

        // Build a stylised booster pack texture programmatically
        int w = 256, h = 360;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

        // Background gradient: deep navy → midnight blue
        Color topColor    = new Color(0.08f, 0.12f, 0.35f, 1f);
        Color bottomColor = new Color(0.04f, 0.06f, 0.16f, 1f);

        for (int y = 0; y < h; y++)
        {
            float t = (float)y / h;
            Color rowColor = Color.Lerp(bottomColor, topColor, t);
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, rowColor);
        }

        // Gold horizontal accent stripe near top third
        int stripeY = (int)(h * 0.72f);
        for (int sy = stripeY; sy < stripeY + 6; sy++)
            for (int sx = 0; sx < w; sx++)
                tex.SetPixel(sx, sy, new Color(0.95f, 0.78f, 0.12f, 1f));

        // Pokeball-like circle in the centre
        Vector2 centre = new Vector2(w * 0.5f, h * 0.42f);
        float outerR = 52f, innerR = 14f, lineH = 5f;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = x - centre.x, dy = y - centre.y;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > outerR) continue;

                Color circleFill;
                if (dist < innerR)
                    circleFill = new Color(0.9f, 0.9f, 0.92f, 1f); // Centre button
                else if (Mathf.Abs(dy) < lineH)
                    circleFill = new Color(0.15f, 0.15f, 0.2f, 1f);  // Horizontal divider
                else if (dy > 0)
                    circleFill = new Color(0.88f, 0.15f, 0.12f, 1f); // Top half: red
                else
                    circleFill = new Color(0.88f, 0.88f, 0.92f, 1f); // Bottom half: white

                // Soft edge anti-alias
                float edge = outerR - dist;
                if (edge < 2f) circleFill.a = edge * 0.5f;

                Color current = tex.GetPixel(x, y);
                tex.SetPixel(x, y, Color.Lerp(current, circleFill, circleFill.a));
            }
        }

        // "POKEMON" text replaced by a simple star cluster
        Color starColor = new Color(0.95f, 0.82f, 0.18f, 1f);
        int[] starXs = { w / 2 - 60, w / 2 - 20, w / 2 + 20, w / 2 + 60, w / 2 };
        int[] starYs = { (int)(h * 0.88f), (int)(h * 0.91f), (int)(h * 0.89f), (int)(h * 0.92f), (int)(h * 0.86f) };
        for (int s = 0; s < starXs.Length; s++)
        {
            int sx = starXs[s], sy = starYs[s];
            for (int dy2 = -4; dy2 <= 4; dy2++)
                for (int dx2 = -4; dx2 <= 4; dx2++)
                {
                    float d = Mathf.Sqrt(dx2 * dx2 + dy2 * dy2);
                    if (d < 4f) tex.SetPixel(sx + dx2, sy + dy2, starColor);
                }
        }

        tex.Apply();
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteBorder = new Vector4(0, 0, 0, 0);
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
#endif
