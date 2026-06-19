using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Pure utility class for weighted random card drawing from a master pool.
/// Guarantees a high-rarity (3+ star) card in the last slot if one exists.
/// No MonoBehaviour — call statically from PackOpeningController.
/// </summary>
public static class PackDrawer
{
    // Weights indexed by (rarityStars - 1): 1★→8, 2★→4, 3★→2, 4★→1, 5★→1
    private static readonly int[] RarityWeights = { 8, 4, 2, 1, 1 };

    /// <summary>Draw <paramref name="count"/> cards from <paramref name="pool"/> using weighted rarity.</summary>
    public static List<PokemonCardData> Draw(List<PokemonCardData> pool, int count)
    {
        var result = new List<PokemonCardData>();

        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("[PackDrawer] masterCardPool is empty — nothing to draw.");
            return result;
        }

        // Build the weighted pool
        var weightedPool = new List<PokemonCardData>();
        foreach (var card in pool)
        {
            if (card == null) continue;
            int w = RarityWeights[Mathf.Clamp(card.rarityStars, 1, 5) - 1];
            for (int i = 0; i < w; i++) weightedPool.Add(card);
        }

        // Find the highest rarity card for the guaranteed last-slot pull
        PokemonCardData guaranteed = null;
        int maxRarity = 0;
        foreach (var c in pool)
            if (c != null && c.rarityStars > maxRarity) { maxRarity = c.rarityStars; guaranteed = c; }

        // Draw cards
        for (int i = 0; i < count; i++)
        {
            bool isLastSlot = i == count - 1;
            PokemonCardData pick = isLastSlot && guaranteed != null && maxRarity >= 3
                ? guaranteed
                : weightedPool[Random.Range(0, weightedPool.Count)];
            result.Add(pick);
        }

        return result;
    }
}
