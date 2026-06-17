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

    [Header("Empty Collection State")]
    [Tooltip("Optional label shown when the player owns no cards yet.")]
    [SerializeField] private GameObject emptyCollectionHint; // e.g. 'Open a pack to get your first cards!'

    [Header("Detail Inspect Overlay")]
    [SerializeField] private GameObject inspectOverlay;
    [SerializeField] private Button inspectOverlayCloseButton;
    [SerializeField] private Transform inspect3DAnchor;
    [SerializeField] private float inspectCardScale = 1f;
    [SerializeField] private List<GameObject> card3DPrefabs = new List<GameObject>();

    private GameObject spawned3DInspectCard;

    [Header("State")]
    [SerializeField] private Tab activeTab = Tab.Collection;

    private List<GameObject> spawnedCards = new List<GameObject>();

    private void Awake()
    {
        SetupUI();
        SetupInspectOverlayClose();
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            SetupButtonListeners();
            SetupInspectOverlayClose();
        }
        SwitchToTab(activeTab);
    }

    private void OnValidate()
    {
        #if UNITY_EDITOR
        // Auto-find all Card Data assets if empty
        if (cardsData.Count == 0)
        {
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
        }
        else
        {
            // Enforce uniqueness to clean up duplicates already serialized in the Inspector/Scene
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
        }

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

    private void SetupInspectOverlayClose()
    {
        if (inspectOverlayCloseButton != null)
        {
            inspectOverlayCloseButton.onClick.RemoveAllListeners();
            inspectOverlayCloseButton.onClick.AddListener(HideInspectOverlay);
        }

        if (inspectOverlay != null)
        {
            Button overlayBtn = inspectOverlay.GetComponent<Button>();
            if (overlayBtn != null)
            {
                overlayBtn.onClick.RemoveAllListeners();
                overlayBtn.onClick.AddListener(HideInspectOverlay);
            }
        }
    }

    public void SwitchToTab(Tab tab)
    {
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
            Spawn2DCardGrid();
        }
        else
        {
            ClearSpawnedCards();
        }
    }

    private void UpdateTabStyles()
    {
        Color activeColor = Color.white;
        Color inactiveColor = new Color(0.5f, 0.55f, 0.65f, 1f); // Muted slate blue/gray

        if (collectionTabButton != null)
        {
            var txt = collectionTabButton.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.color = activeTab == Tab.Collection ? activeColor : inactiveColor;
                txt.fontStyle = activeTab == Tab.Collection ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        if (storeTabButton != null)
        {
            var txt = storeTabButton.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.color = activeTab == Tab.Store ? activeColor : inactiveColor;
                txt.fontStyle = activeTab == Tab.Store ? FontStyles.Bold : FontStyles.Normal;
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

    private void Spawn2DCardGrid()
    {
        ClearSpawnedCards();

        if (gridContentContainer == null || card2DPrefab == null || cardsData.Count == 0) return;

        // ── At runtime: only show cards the player actually owns.
        // ── In the Editor (not playing): show the full pool for design purposes.
        List<PokemonCardData> displayCards;
        if (Application.isPlaying)
        {
            displayCards = PlayerCollection.GetOwnedCards(cardsData);
        }
        else
        {
            displayCards = cardsData;
        }

        // Show / hide the empty-state hint
        if (emptyCollectionHint != null)
            emptyCollectionHint.SetActive(Application.isPlaying && displayCards.Count == 0);

        if (displayCards.Count == 0) return;

        HashSet<PokemonCardData> uniqueCards = new HashSet<PokemonCardData>();
        foreach (var data in displayCards)
        {
            if (data == null) continue;
            if (uniqueCards.Contains(data)) continue;
            uniqueCards.Add(data);

            // Create cell container to prevent GridLayoutGroup from overriding card prefab size and breaking absolute positions
            GameObject cellObj = new GameObject("CardCell", typeof(RectTransform));
            cellObj.transform.SetParent(gridContentContainer, false);
            RectTransform cellRt = cellObj.GetComponent<RectTransform>();
            cellRt.localScale = Vector3.one;
            cellRt.localPosition = Vector3.zero;
            cellRt.localRotation = Quaternion.identity;

            GameObject cardInst = null;
            if (Application.isPlaying)
            {
                cardInst = Instantiate(card2DPrefab, cellObj.transform, false);
            }
            else
            {
                #if UNITY_EDITOR
                cardInst = PrefabUtility.InstantiatePrefab(card2DPrefab, cellObj.transform) as GameObject;
                #else
                cardInst = Instantiate(card2DPrefab, cellObj.transform, false);
                #endif
            }

            if (cardInst != null)
            {
                RectTransform rt = cardInst.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = new Vector3(0.6857f, 0.6857f, 1f); // Scale card down to fit the 480x672 cell size perfectly
                }

                // Attach hover effect component for smooth premium interactivity
                if (cellObj.GetComponent<UICardHoverEffect>() == null)
                {
                    cellObj.AddComponent<UICardHoverEffect>();
                }

                Card2DUIController controller = cardInst.GetComponent<Card2DUIController>();
                if (controller != null)
                {
                    controller.SetCardData(data);
                    controller.SetupClick(ShowInspectOverlay);
                }
                spawnedCards.Add(cellObj);
            }
            else
            {
                if (Application.isPlaying) Destroy(cellObj);
                else DestroyImmediate(cellObj);
            }
        }
    }

    private void ClearSpawnedCards()
    {
        // 1. Clear tracked list references
        foreach (var card in spawnedCards)
        {
            if (card != null)
            {
                if (Application.isPlaying) Destroy(card);
                else DestroyImmediate(card);
            }
        }
        spawnedCards.Clear();

        // 2. Clear any children physically under the container to prevent [ExecuteAlways] duplicate leaks
        if (gridContentContainer != null)
        {
            List<GameObject> children = new List<GameObject>();
            for (int i = 0; i < gridContentContainer.childCount; i++)
            {
                Transform child = gridContentContainer.GetChild(i);
                if (child != null)
                {
                    children.Add(child.gameObject);
                }
            }
            foreach (var child in children)
            {
                if (child != null)
                {
                    if (Application.isPlaying) Destroy(child);
                    else DestroyImmediate(child);
                }
            }
        }
    }

    public void ShowInspectOverlay(PokemonCardData data)
    {
        // 1. Clean up any existing inspect card
        if (spawned3DInspectCard != null)
        {
            Destroy(spawned3DInspectCard);
        }

        // 2. Open dark overlay panel
        if (inspectOverlay != null)
        {
            inspectOverlay.SetActive(true);
        }

        // 3. Find and instantiate matching 3D card prefab
        if (inspect3DAnchor != null && card3DPrefabs.Count > 0)
        {
            GameObject matchingPrefab = null;
            foreach (var prefab in card3DPrefabs)
            {
                if (prefab == null) continue;
                CardUIController controller = prefab.GetComponent<CardUIController>();
                if (controller != null)
                {
                    // Match by name
                    var dataField = typeof(CardUIController).GetField("cardData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    PokemonCardData prefabData = dataField?.GetValue(controller) as PokemonCardData;
                    if (prefabData != null && prefabData.pokemonName == data.pokemonName)
                    {
                        matchingPrefab = prefab;
                        break;
                    }
                }
            }

            if (matchingPrefab != null)
            {
                if (Application.isPlaying)
                {
                    spawned3DInspectCard = Instantiate(matchingPrefab, inspect3DAnchor);
                }
                else
                {
                    #if UNITY_EDITOR
                    spawned3DInspectCard = PrefabUtility.InstantiatePrefab(matchingPrefab, inspect3DAnchor) as GameObject;
                    #else
                    spawned3DInspectCard = Instantiate(matchingPrefab, inspect3DAnchor);
                    #endif
                }

                if (spawned3DInspectCard != null)
                {
                    spawned3DInspectCard.transform.localPosition = Vector3.zero;
                    spawned3DInspectCard.transform.localRotation = Quaternion.identity;
                    // Scale up slightly for close-up inspection
                    spawned3DInspectCard.transform.localScale = new Vector3(inspectCardScale, inspectCardScale, inspectCardScale);

                    // Add/Configure CardRotator for slow auto-spinning
                    CardRotator rotator = spawned3DInspectCard.GetComponent<CardRotator>();
                    if (rotator == null)
                    {
                        rotator = spawned3DInspectCard.AddComponent<CardRotator>();
                    }
                    rotator.enabled = true;
                    rotator.autoSpin = true;
                    rotator.spinAxis = new Vector3(0f, 1f, 0f);
                    rotator.spinSpeed = 15f; // Slow, premium rotation speed
                }
            }
        }
    }

    public void HideInspectOverlay()
    {
        if (inspectOverlay != null)
        {
            inspectOverlay.SetActive(false);
        }

        if (spawned3DInspectCard != null)
        {
            if (Application.isPlaying) Destroy(spawned3DInspectCard);
            else DestroyImmediate(spawned3DInspectCard);
            spawned3DInspectCard = null;
        }
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
