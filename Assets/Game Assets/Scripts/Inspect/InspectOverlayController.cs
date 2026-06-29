using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
public class InspectOverlayController : MonoBehaviour
{
    private GameObject inspectOverlay;
    private Button inspectOverlayCloseButton;
    private Transform inspect3DAnchor;
    private float inspectCardScale = 1f;
    private List<GameObject> card3DPrefabs;

    private GameObject spawned3DInspectCard;

    public void Initialize(
        GameObject inspectOverlay,
        Button inspectOverlayCloseButton,
        Transform inspect3DAnchor,
        float inspectCardScale,
        List<GameObject> card3DPrefabs)
    {
        this.inspectOverlay = inspectOverlay;
        this.inspectOverlayCloseButton = inspectOverlayCloseButton;
        this.inspect3DAnchor = inspect3DAnchor;
        this.inspectCardScale = inspectCardScale;
        this.card3DPrefabs = card3DPrefabs;

        SetupInspectOverlayClose();
    }

    private void SetupInspectOverlayClose()
    {
        if (inspectOverlayCloseButton != null)
        {
            inspectOverlayCloseButton.onClick.RemoveAllListeners();
            inspectOverlayCloseButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSound();
                HideInspectOverlay();
            });
        }

        if (inspectOverlay != null)
        {
            Button overlayBtn = inspectOverlay.GetComponent<Button>();
            if (overlayBtn != null)
            {
                overlayBtn.onClick.RemoveAllListeners();
                overlayBtn.onClick.AddListener(() => {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSound();
                    HideInspectOverlay();
                });
            }
        }
    }

    public void ShowInspectOverlay(PokemonCardData data)
    {
        // 1. Clean up any existing inspect card
        if (spawned3DInspectCard != null)
        {
            if (Application.isPlaying) Destroy(spawned3DInspectCard);
            else DestroyImmediate(spawned3DInspectCard);
        }

        // 2. Open dark overlay panel
        if (inspectOverlay != null)
        {
            inspectOverlay.SetActive(true);
        }

        // 3. Find and instantiate matching 3D card prefab
        if (inspect3DAnchor != null && card3DPrefabs != null && card3DPrefabs.Count > 0)
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
                    spawned3DInspectCard = UnityEditor.PrefabUtility.InstantiatePrefab(matchingPrefab, inspect3DAnchor) as GameObject;
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
}
