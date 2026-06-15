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

    [Header("Detail Inspect Overlay")]
    [SerializeField] private GameObject inspectOverlay;
    [SerializeField] private Card2DUIController inspectOverlayCardController;
    [SerializeField] private Button inspectOverlayCloseButton;

    [Header("State")]
    [SerializeField] private Tab activeTab = Tab.Collection;

    private List<GameObject> spawnedCards = new List<GameObject>();

    private void Awake()
    {
        SetupUI();
        SwitchToTab(activeTab);
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
                if (data != null)
                {
                    cardsData.Add(data);
                }
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

        foreach (var data in cardsData)
        {
            if (data == null) continue;

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
        if (Application.isPlaying)
        {
            foreach (var card in spawnedCards)
            {
                if (card != null) Destroy(card);
            }
            spawnedCards.Clear();
        }
        else
        {
            if (gridContentContainer != null)
            {
                List<GameObject> children = new List<GameObject>();
                for (int i = 0; i < gridContentContainer.childCount; i++)
                {
                    children.Add(gridContentContainer.GetChild(i).gameObject);
                }
                foreach (var child in children)
                {
                    DestroyImmediate(child);
                }
            }
            spawnedCards.Clear();
        }
    }

    public void ShowInspectOverlay(PokemonCardData data)
    {
        if (inspectOverlay != null)
        {
            inspectOverlay.SetActive(true);
        }
        if (inspectOverlayCardController != null)
        {
            inspectOverlayCardController.SetCardData(data);
        }
    }

    public void HideInspectOverlay()
    {
        if (inspectOverlay != null)
        {
            inspectOverlay.SetActive(false);
        }
    }

    private void SetupUI()
    {
        if (mainCanvas == null) mainCanvas = FindObjectOfType<Canvas>();
    }
}
