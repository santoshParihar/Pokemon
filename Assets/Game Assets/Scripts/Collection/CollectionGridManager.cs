using UnityEngine;
using System.Collections.Generic;
using TMPro;

[ExecuteAlways]
public class CollectionGridManager : MonoBehaviour
{
    private GameObject card2DPrefab;
    private Transform gridContentContainer;
    private List<PokemonCardData> cardsData;
    private TMP_InputField searchInputField;
    private GameObject emptyCollectionHint;
    private System.Action<PokemonCardData> onCardClicked;

    private List<GameObject> spawnedCards = new List<GameObject>();

    public void Initialize(
        GameObject card2DPrefab,
        Transform gridContentContainer,
        List<PokemonCardData> cardsData,
        TMP_InputField searchInputField,
        GameObject emptyCollectionHint,
        System.Action<PokemonCardData> onCardClicked)
    {
        this.card2DPrefab = card2DPrefab;
        this.gridContentContainer = gridContentContainer;
        this.cardsData = cardsData;
        this.searchInputField = searchInputField;
        this.emptyCollectionHint = emptyCollectionHint;
        this.onCardClicked = onCardClicked;
    }

    public void Spawn2DCardGrid()
    {
        ClearSpawnedCards();

        if (gridContentContainer == null || card2DPrefab == null || cardsData == null || cardsData.Count == 0) return;

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

        // Filter displayCards by search input name query
        if (searchInputField != null && !string.IsNullOrEmpty(searchInputField.text))
        {
            string query = searchInputField.text.ToLower();
            displayCards = displayCards.FindAll(c => c.pokemonName.ToLower().Contains(query));
        }

        if (displayCards.Count == 0) return;

        foreach (var data in displayCards)
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
                cardInst = UnityEditor.PrefabUtility.InstantiatePrefab(card2DPrefab, cellObj.transform) as GameObject;
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
                    controller.SetupClick(onCardClicked);
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

    public void ClearSpawnedCards()
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
}
