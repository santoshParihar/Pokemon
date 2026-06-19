using UnityEngine;
using TMPro;

public class CollectionSearchField : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform gridContentContainer;

    private int lastChildCount = 0;
    private string lastQuery = "";

    private void Awake()
    {
        if (inputField == null) inputField = GetComponent<TMP_InputField>();
    }

    private void Start()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener((val) => ApplyFilter());
        }
    }

    private void Update()
    {
        // Automatically re-apply filter if new cards are spawned or query changes
        if (gridContentContainer != null)
        {
            string currentQuery = inputField != null ? inputField.text : "";
            if (gridContentContainer.childCount != lastChildCount || currentQuery != lastQuery)
            {
                ApplyFilter();
            }
        }
    }

    public void ApplyFilter()
    {
        if (gridContentContainer == null) return;

        string query = inputField != null ? inputField.text : "";
        lastQuery = query;
        lastChildCount = gridContentContainer.childCount;

        bool hasQuery = !string.IsNullOrEmpty(query);
        string lowerQuery = query.ToLower();

        for (int i = 0; i < gridContentContainer.childCount; i++)
        {
            Transform cell = gridContentContainer.GetChild(i);
            if (cell == null) continue;

            Card2DUIController controller = cell.GetComponentInChildren<Card2DUIController>();
            if (controller != null && controller.CardData != null)
            {
                bool matches = !hasQuery || controller.CardData.pokemonName.ToLower().Contains(lowerQuery);
                cell.gameObject.SetActive(matches);
            }
            else
            {
                cell.gameObject.SetActive(!hasQuery);
            }
        }
    }
}
