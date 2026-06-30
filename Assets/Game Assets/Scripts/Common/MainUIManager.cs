using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MainUIManager : MonoBehaviour
{
    // Public so PackOpeningController can reference Tab.Collection
    public enum Tab
    {
        Collection,
        Store
    }

    [Header("UI References")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private TextMeshProUGUI appTitleText;
    [SerializeField] private Button collectionTabButton;
    [SerializeField] private Button storeTabButton;
    [SerializeField] private GameObject collectionTabUnderline;
    [SerializeField] private GameObject storeTabUnderline;
    
    [Header("Content Panels")]
    [SerializeField] private GameObject storePanel;
    [SerializeField] private GameObject collectionPanel; // Grid ScrollView Panel

    [Header("2D Grid Setup")]
    [SerializeField] private GameObject card2DPrefab;
    [SerializeField] private Transform gridContentContainer;
    [SerializeField] private List<PokemonCardData> cardsData = new List<PokemonCardData>();
    [SerializeField] private TMP_InputField searchInputField;

    [Header("Empty Collection State")]
    [Tooltip("Optional label shown when the player owns no cards yet.")]
    [SerializeField] private GameObject emptyCollectionHint; // e.g. 'Open a pack to get your first cards!'

    [Header("Detail Inspect Overlay")]
    [SerializeField] private GameObject inspectOverlay;
    [SerializeField] private Button inspectOverlayCloseButton;
    [SerializeField] private Transform inspect3DAnchor;
    [SerializeField] private float inspectCardScale = 1f;
    [SerializeField] private List<GameObject> card3DPrefabs = new List<GameObject>();

    [Header("State")]
    [SerializeField] private Tab activeTab = Tab.Collection;

    private CollectionGridManager collectionGrid;
    private InspectOverlayController inspectOverlayController;

    private void Awake()
    {
        EnsureSubComponents();
        SetupUI();
    }

    private GameObject loadingOverlayInstance;
    private RectTransform loadingProgressBarFill;
    private TextMeshProUGUI loadingProgressText;
    private TextMeshProUGUI loadingStatusText;

    private void Start()
    {
        EnsureSubComponents();
        if (Application.isPlaying)
        {
            SetupButtonListeners();
            if (searchInputField != null)
            {
                searchInputField.onValueChanged.RemoveAllListeners();
                searchInputField.onValueChanged.AddListener((val) => {
                    EnsureSubComponents();
                    collectionGrid.Spawn2DCardGrid();
                });
            }
            StartPreloadingSequence();
        }
        else
        {
            SwitchToTab(activeTab);
        }
    }

    private void StartPreloadingSequence()
    {
        if (collectionTabButton != null) collectionTabButton.interactable = false;
        if (storeTabButton != null) storeTabButton.interactable = false;

        CreateLoadingOverlay();

        ImageCacheManager.Instance.PreloadImages(cardsData, 
            onProgress: (progress) =>
            {
                if (loadingProgressBarFill != null)
                {
                    loadingProgressBarFill.anchorMax = new Vector2(progress, 1f);
                }
                if (loadingProgressText != null)
                {
                    loadingProgressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
                }
                if (loadingStatusText != null && cardsData != null && cardsData.Count > 0)
                {
                    int index = Mathf.Clamp(Mathf.FloorToInt(progress * cardsData.Count), 0, cardsData.Count - 1);
                    if (cardsData[index] != null)
                    {
                        loadingStatusText.text = $"Syncing card data: {cardsData[index].pokemonName}...";
                    }
                }
            },
            onComplete: () =>
            {
                if (loadingOverlayInstance != null)
                {
                    CanvasGroup cg = loadingOverlayInstance.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        #if PRIME_TWEEN_INSTALLED
                        PrimeTween.Tween.Alpha(cg, 0f, 0.5f, PrimeTween.Ease.OutCubic).OnComplete(() =>
                        {
                            if (loadingOverlayInstance != null) Destroy(loadingOverlayInstance);
                            if (collectionTabButton != null) collectionTabButton.interactable = true;
                            if (storeTabButton != null) storeTabButton.interactable = true;
                            SwitchToTab(activeTab);
                        });
                        #else
                        Destroy(loadingOverlayInstance);
                        if (collectionTabButton != null) collectionTabButton.interactable = true;
                        if (storeTabButton != null) storeTabButton.interactable = true;
                        SwitchToTab(activeTab);
                        #endif
                    }
                    else
                    {
                        Destroy(loadingOverlayInstance);
                        if (collectionTabButton != null) collectionTabButton.interactable = true;
                        if (storeTabButton != null) storeTabButton.interactable = true;
                        SwitchToTab(activeTab);
                    }
                }
                else
                {
                    if (collectionTabButton != null) collectionTabButton.interactable = true;
                    if (storeTabButton != null) storeTabButton.interactable = true;
                    SwitchToTab(activeTab);
                }
            }
        );
    }

    private void CreateLoadingOverlay()
    {
        if (mainCanvas == null) mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null) return;

        // 1. Create Loading Overlay Panel
        loadingOverlayInstance = new GameObject("LoadingOverlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        loadingOverlayInstance.transform.SetParent(mainCanvas.transform, false);
        
        RectTransform overlayRt = loadingOverlayInstance.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero;
        overlayRt.offsetMax = Vector2.zero;
        
        Image bgImage = loadingOverlayInstance.GetComponent<Image>();
        bgImage.color = new Color(0.06f, 0.08f, 0.11f, 1f); // Deep graphite (#0F141C)

        // 2. Create Centered Container for items
        GameObject container = new GameObject("Container", typeof(RectTransform));
        container.transform.SetParent(loadingOverlayInstance.transform, false);
        RectTransform containerRt = container.GetComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, 0.5f);
        containerRt.anchorMax = new Vector2(0.5f, 0.5f);
        containerRt.pivot = new Vector2(0.5f, 0.5f);
        containerRt.anchoredPosition = new Vector2(0f, -50f);
        containerRt.sizeDelta = new Vector2(700, 450);

        // 3. Brand Logo Title
        GameObject titleObj = new GameObject("BrandTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(container.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 0.72f);
        titleRt.anchorMax = new Vector2(1f, 0.95f);
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;
        
        TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
        titleTMP.text = "CARD OUTPOST";
        titleTMP.fontSize = 62f;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        titleTMP.fontStyle = FontStyles.Bold;

        // 4. Subtitle
        GameObject subtitleObj = new GameObject("BrandSubtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        subtitleObj.transform.SetParent(container.transform, false);
        RectTransform subtitleRt = subtitleObj.GetComponent<RectTransform>();
        subtitleRt.anchorMin = new Vector2(0f, 0.60f);
        subtitleRt.anchorMax = new Vector2(1f, 0.72f);
        subtitleRt.offsetMin = Vector2.zero;
        subtitleRt.offsetMax = Vector2.zero;
        
        TextMeshProUGUI subtitleTMP = subtitleObj.GetComponent<TextMeshProUGUI>();
        subtitleTMP.text = "TCG COLLECTION COMPANION";
        subtitleTMP.fontSize = 22f;
        subtitleTMP.alignment = TextAlignmentOptions.Center;
        subtitleTMP.color = new Color(0.9f, 0.22f, 0.27f, 1f); // Theme crimson
        subtitleTMP.fontStyle = FontStyles.Bold;

        // 5. Progress Percentage Text
        GameObject textObj = new GameObject("ProgressText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(container.transform, false);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0f, 0.42f);
        textRt.anchorMax = new Vector2(1f, 0.55f);
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        
        loadingProgressText = textObj.GetComponent<TextMeshProUGUI>();
        loadingProgressText.text = "0%";
        loadingProgressText.fontSize = 32f;
        loadingProgressText.alignment = TextAlignmentOptions.Center;
        loadingProgressText.color = new Color(0.7f, 0.75f, 0.8f, 0.95f);
        loadingProgressText.fontStyle = FontStyles.Bold;

        // Create a custom runtime rounded slice sprite
        Sprite barSprite = CreateRuntimeRoundedRectSprite(128, 32, 12f);

        // 6. Progress Bar Background
        GameObject barBg = new GameObject("ProgressBarBg", typeof(RectTransform), typeof(Image));
        barBg.transform.SetParent(container.transform, false);
        RectTransform barBgRt = barBg.GetComponent<RectTransform>();
        barBgRt.anchorMin = new Vector2(0.08f, 0.28f);
        barBgRt.anchorMax = new Vector2(0.92f, 0.35f);
        barBgRt.offsetMin = Vector2.zero;
        barBgRt.offsetMax = Vector2.zero;
        
        Image barBgImg = barBg.GetComponent<Image>();
        barBgImg.sprite = barSprite;
        barBgImg.type = Image.Type.Sliced;
        barBgImg.color = new Color(0.12f, 0.15f, 0.20f, 1f);

        // 7. Progress Bar Fill
        GameObject barFill = new GameObject("ProgressBarFill", typeof(RectTransform), typeof(Image));
        barFill.transform.SetParent(barBg.transform, false);
        loadingProgressBarFill = barFill.GetComponent<RectTransform>();
        loadingProgressBarFill.anchorMin = Vector2.zero;
        loadingProgressBarFill.anchorMax = new Vector2(0f, 1f);
        loadingProgressBarFill.offsetMin = Vector2.zero;
        loadingProgressBarFill.offsetMax = Vector2.zero;
        
        Image barFillImg = barFill.GetComponent<Image>();
        barFillImg.sprite = barSprite;
        barFillImg.type = Image.Type.Sliced;
        barFillImg.color = new Color(0.9f, 0.22f, 0.27f, 1f); // Vibrant red fill

        // 8. Dynamic Status Text
        GameObject statusObj = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
        statusObj.transform.SetParent(container.transform, false);
        RectTransform statusRt = statusObj.GetComponent<RectTransform>();
        statusRt.anchorMin = new Vector2(0f, 0.05f);
        statusRt.anchorMax = new Vector2(1f, 0.20f);
        statusRt.offsetMin = Vector2.zero;
        statusRt.offsetMax = Vector2.zero;

        loadingStatusText = statusObj.GetComponent<TextMeshProUGUI>();
        loadingStatusText.text = "Syncing card database...";
        loadingStatusText.fontSize = 20f;
        loadingStatusText.alignment = TextAlignmentOptions.Center;
        loadingStatusText.color = new Color(0.55f, 0.6f, 0.65f, 0.8f);
        loadingStatusText.fontStyle = FontStyles.Italic;
    }

    private Sprite CreateRuntimeRoundedRectSprite(int width, int height, float radius)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color transparent = new Color(0f, 0f, 0f, 0f);
        Color white = Color.white;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float cx = x < radius ? radius : (x > width - radius ? width - radius : x);
                float cy = y < radius ? radius : (y > height - radius ? height - radius : y);
                
                float dx = x - cx;
                float dy = y - cy;
                
                if (dx * dx + dy * dy > radius * radius)
                {
                    tex.SetPixel(x, y, transparent);
                }
                else
                {
                    tex.SetPixel(x, y, white);
                }
            }
        }
        tex.Apply();
        
        float border = radius;
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.Tight, new Vector4(border, border, border, border));
    }

    private void EnsureSubComponents()
    {
        if (collectionGrid == null)
        {
            collectionGrid = GetComponent<CollectionGridManager>();
            if (collectionGrid == null) collectionGrid = gameObject.AddComponent<CollectionGridManager>();
        }
        if (inspectOverlayController == null)
        {
            inspectOverlayController = GetComponent<InspectOverlayController>();
            if (inspectOverlayController == null) inspectOverlayController = gameObject.AddComponent<InspectOverlayController>();
        }

        collectionGrid.Initialize(
            card2DPrefab,
            gridContentContainer,
            cardsData,
            searchInputField,
            emptyCollectionHint,
            ShowInspectOverlay
        );

        inspectOverlayController.Initialize(
            inspectOverlay,
            inspectOverlayCloseButton,
            inspect3DAnchor,
            inspectCardScale,
            card3DPrefabs
        );
    }

    private void OnValidate()
    {
        #if UNITY_EDITOR
        // Always sync all available Card Data assets in the editor to avoid getting out of sync
        string[] guids = AssetDatabase.FindAssets("t:PokemonCardData", new string[] { "Assets/Game Assets/Data" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PokemonCardData data = AssetDatabase.LoadAssetAtPath<PokemonCardData>(path);
            if (data != null && !cardsData.Contains(data))
            {
                cardsData.Add(data);
            }
        }

        // Clean up nulls and enforce uniqueness
        List<PokemonCardData> uniqueList = new List<PokemonCardData>();
        foreach (var data in cardsData)
        {
            if (data != null && !uniqueList.Contains(data))
            {
                uniqueList.Add(data);
            }
        }
        if (uniqueList.Count != cardsData.Count)
        {
            cardsData = uniqueList;
        }

        // Sync 3D card prefabs automatically
        string[] prefabs = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Game Assets/Prefabs" });
        List<GameObject> prefabsList = new List<GameObject>();
        foreach (var guid in prefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go != null && go.GetComponent<CardUIController>() != null)
            {
                prefabsList.Add(go);
            }
        }
        card3DPrefabs = prefabsList;

        // Defer tab switching to the next editor update frame to avoid DestroyImmediate issues inside OnValidate
        EditorApplication.delayCall -= DeferSwitchTab;
        EditorApplication.delayCall += DeferSwitchTab;
        #endif
    }

    #if UNITY_EDITOR
    private void DeferSwitchTab()
    {
        if (this != null)
        {
            SwitchToTab(activeTab);
        }
    }
    #endif

    private void SetupButtonListeners()
    {
        if (collectionTabButton != null)
        {
            collectionTabButton.onClick.RemoveAllListeners();
            collectionTabButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSound();
                SwitchToTab(Tab.Collection);
            });
        }

        if (storeTabButton != null)
        {
            storeTabButton.onClick.RemoveAllListeners();
            storeTabButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSound();
                SwitchToTab(Tab.Store);
            });
        }
    }

    public void SwitchToTab(Tab tab)
    {
        EnsureSubComponents();
        activeTab = tab;

        // Hide inspect overlay when changing tabs
        HideInspectOverlay();

        // Toggle UI Panels
        if (storePanel != null)
        {
            storePanel.SetActive(activeTab == Tab.Store);
        }

        if (collectionPanel != null)
        {
            collectionPanel.SetActive(activeTab == Tab.Collection);
        }

        // Update Tab Button Colors/Style
        UpdateTabStyles();

        if (activeTab == Tab.Collection)
        {
            collectionGrid.Spawn2DCardGrid();
        }
        else
        {
            collectionGrid.ClearSpawnedCards();
        }
    }

    private void UpdateTabStyles()
    {
        Color activeColor = Color.black;
        Color inactiveColor = new Color(0.4f, 0.45f, 0.5f, 1f);

        if (collectionTabButton != null)
        {
            var txt = collectionTabButton.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.color = activeTab == Tab.Collection ? activeColor : inactiveColor;
                txt.fontStyle = activeTab == Tab.Collection ? FontStyles.Bold : FontStyles.Normal;
                txt.fontSize = 42;
            }
        }

        if (storeTabButton != null)
        {
            var txt = storeTabButton.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.color = activeTab == Tab.Store ? activeColor : inactiveColor;
                txt.fontStyle = activeTab == Tab.Store ? FontStyles.Bold : FontStyles.Normal;
                txt.fontSize = 42;
            }
        }

        // Toggle Underline active states
        if (collectionTabUnderline != null)
        {
            collectionTabUnderline.SetActive(activeTab == Tab.Collection);
        }
        if (storeTabUnderline != null)
        {
            storeTabUnderline.SetActive(activeTab == Tab.Store);
        }
    }

    public void ShowInspectOverlay(PokemonCardData data)
    {
        EnsureSubComponents();
        inspectOverlayController.ShowInspectOverlay(data);
    }

    public void HideInspectOverlay()
    {
        EnsureSubComponents();
        inspectOverlayController.HideInspectOverlay();
    }

    private void SetupUI()
    {
        if (mainCanvas == null) mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas != null && mainCanvas.renderMode == RenderMode.ScreenSpaceCamera && mainCanvas.worldCamera == null)
        {
            mainCanvas.worldCamera = Camera.main;
        }
    }
}

