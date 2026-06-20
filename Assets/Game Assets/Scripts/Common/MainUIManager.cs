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
        }
        SwitchToTab(activeTab);
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
            collectionTabButton.onClick.AddListener(() => SwitchToTab(Tab.Collection));
        }

        if (storeTabButton != null)
        {
            storeTabButton.onClick.RemoveAllListeners();
            storeTabButton.onClick.AddListener(() => SwitchToTab(Tab.Store));
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

