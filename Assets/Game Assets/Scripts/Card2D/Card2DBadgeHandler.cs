using UnityEngine;
using TMPro;

/// <summary>
/// Populates the stat badge text fields (stage, CP, retreat cost, weakness,
/// resistance, rarity) on a 2D Pokémon card.
/// </summary>
public static class Card2DBadgeHandler
{
    private static readonly string[] RarityNames = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };

    public static void Apply(
        PokemonCardData cardData,
        TextMeshProUGUI badgeStageTmp,
        TextMeshProUGUI badgeCPTmp,
        TextMeshProUGUI badgeRetreatTmp,
        TextMeshProUGUI badgeWeakTmp,
        TextMeshProUGUI badgeResistTmp,
        TextMeshProUGUI badgeRarityTmp)
    {
        if (badgeStageTmp != null)
            badgeStageTmp.text = $"<color=#1E222B><b>Stage: {cardData.stage}</b></color>";

        if (badgeCPTmp != null)
        {
            int cp = cardData.hp * 5 + cardData.attack1Damage * 3;
            badgeCPTmp.text = $"<color=#1E222B><b>CP {cp}</b></color>";
        }

        if (badgeRetreatTmp != null)
        {
            int cost = Mathf.Clamp(cardData.retreatCost, 0, 5);
            badgeRetreatTmp.text = $"<color=#1E222B><b>Retreat: {(cost == 0 ? "None" : cost.ToString())}</b></color>";
        }

        if (badgeWeakTmp != null)
            badgeWeakTmp.text = $"<color=#1E222B><b>Weak: {cardData.weakness} {cardData.weaknessValue}</b></color>";

        if (badgeResistTmp != null)
        {
            badgeResistTmp.text = cardData.hasResistance
                ? $"<color=#1E222B><b>Resist: {cardData.resistance} {cardData.resistanceValue}</b></color>"
                : "<color=#1E222B><b>Resist: None</b></color>";
        }

        if (badgeRarityTmp != null)
        {
            int stars = Mathf.Clamp(cardData.rarityStars, 1, 5);
            badgeRarityTmp.text = $"<color=#1E222B><b>Rarity: {RarityNames[stars - 1]}</b></color>";
        }
    }
}
