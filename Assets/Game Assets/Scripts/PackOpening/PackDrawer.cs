using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Pure utility class for weighted random card drawing from a master pool.
/// No MonoBehaviour — call statically from PackOpeningController.
/// </summary>
public static class PackDrawer
{
    /// <summary>Draw <paramref name="count"/> cards from <paramref name="pool"/> using dropProbability.</summary>
    public static List<PokemonCardData> Draw(List<PokemonCardData> pool, int count)
    {
        var result = new List<PokemonCardData>();

        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("[PackDrawer] masterCardPool is empty — nothing to draw.");
            return result;
        }

        // Filter out null cards and compute total weight sum
        var validPool = new List<PokemonCardData>();
        float totalWeight = 0f;
        foreach (var card in pool)
        {
            if (card != null)
            {
                validPool.Add(card);
                totalWeight += Mathf.Max(0.01f, card.dropProbability);
            }
        }

        if (validPool.Count == 0 || totalWeight <= 0f)
        {
            Debug.LogWarning("[PackDrawer] No cards with valid probability weight found.");
            return result;
        }

        // Draw cards
        for (int i = 0; i < count; i++)
        {
            float randomVal = Random.Range(0f, totalWeight);
            float currentSum = 0f;
            PokemonCardData pick = null;

            foreach (var card in validPool)
            {
                currentSum += Mathf.Max(0.01f, card.dropProbability);
                if (randomVal <= currentSum)
                {
                    pick = card;
                    break;
                }
            }

            if (pick == null) pick = validPool[validPool.Count - 1]; // Fallback
            result.Add(pick);
        }

        return result;
    }
}
