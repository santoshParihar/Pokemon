using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Handles attack and ability slot layout, anchor positioning, and rich-text
/// cost formatting for a 2D Pokémon card. Mirrors the 3D card's alignment logic.
/// </summary>
public static class Card2DAttackHandler
{
    public static void Apply(
        PokemonCardData cardData,
        TextMeshProUGUI attack1Name, TextMeshProUGUI attack1Cost,
        TextMeshProUGUI attack1Damage, TextMeshProUGUI attack1Description,
        TextMeshProUGUI attack2Name, TextMeshProUGUI attack2Cost,
        TextMeshProUGUI attack2Damage, TextMeshProUGUI attack2Description)
    {
        // Shared anchor vectors
        var leftAnchor  = new Vector2(0f, 0.5f);
        var rightAnchor = new Vector2(1f, 0.5f);
        var descPivot   = new Vector2(0f, 1f);

        // Apply anchor/pivot to cost & name fields
        ApplyAnchor(attack1Cost,   leftAnchor,  leftAnchor);
        ApplyAnchor(attack1Name,   leftAnchor,  leftAnchor);
        ApplyAnchor(attack2Cost,   leftAnchor,  leftAnchor);
        ApplyAnchor(attack2Name,   leftAnchor,  leftAnchor);
        ApplyAnchor(attack1Damage, rightAnchor, rightAnchor);
        ApplyAnchor(attack2Damage, rightAnchor, rightAnchor);

        // Apply style to description fields
        ApplyDescriptionStyle(attack1Description, leftAnchor, descPivot);
        ApplyDescriptionStyle(attack2Description, leftAnchor, descPivot);

        // Apply style to cost fields
        ApplyCostStyle(attack1Cost);
        ApplyCostStyle(attack2Cost);

        // Layout slot rects
        RectTransform slot1 = attack1Name != null ? attack1Name.transform.parent as RectTransform : null;
        RectTransform slot2 = attack2Name != null ? attack2Name.transform.parent as RectTransform : null;

        bool singleAttack = !cardData.ability.hasAbility;
        if (slot1 != null && slot2 != null)
            LayoutSlots(slot1, slot2, singleAttack,
                        attack1Name, attack1Cost, attack1Damage, attack1Description,
                        attack2Name, attack2Cost, attack2Damage, attack2Description);

        // Populate content
        if (singleAttack)
        {
            PopulateAttack(cardData.attack1CostText, cardData.attack1Damage, cardData.attack1Description,
                           attack1Cost, attack1Name, attack1Damage, attack1Description);
            ClearSlot(attack2Name, attack2Cost, attack2Damage, attack2Description);
        }
        else
        {
            PopulateAbility(cardData, attack1Cost, attack1Name, attack1Damage, attack1Description);
            PopulateAttack(cardData.attack1CostText, cardData.attack1Damage, cardData.attack1Description,
                           attack2Cost, attack2Name, attack2Damage, attack2Description);
        }
    }

    // ── Layout helpers ──────────────────────────────────────────────────────────

    private static void LayoutSlots(
        RectTransform slot1, RectTransform slot2, bool singleAttack,
        TextMeshProUGUI a1Name, TextMeshProUGUI a1Cost, TextMeshProUGUI a1Dmg, TextMeshProUGUI a1Desc,
        TextMeshProUGUI a2Name, TextMeshProUGUI a2Cost, TextMeshProUGUI a2Dmg, TextMeshProUGUI a2Desc)
    {
        if (singleAttack)
        {
            // Single centred attack slot
            slot1.anchorMin = new Vector2(0f, 0.25f); slot1.anchorMax = new Vector2(1f, 0.75f);
            slot1.offsetMin = new Vector2(0, 5);       slot1.offsetMax = new Vector2(0, -5);
            slot1.gameObject.name = "Attack";
            slot2.gameObject.SetActive(false);
            slot2.gameObject.name = "DisabledSlot";

            if (a1Name != null) a1Name.gameObject.name = "AttackName";
            if (a1Cost != null) a1Cost.gameObject.name = "AttackCost";
            if (a1Dmg  != null) a1Dmg.gameObject.name  = "AttackDamage";
            if (a1Desc != null) a1Desc.gameObject.name = "AttackDescription";
        }
        else
        {
            // Top half = Ability, bottom half = Attack
            slot1.anchorMin = new Vector2(0f, 0.5f); slot1.anchorMax = new Vector2(1f, 1f);
            slot1.offsetMin = new Vector2(0, 5);      slot1.offsetMax = new Vector2(0, -5);
            slot1.gameObject.name = "Ability";

            slot2.anchorMin = new Vector2(0f, 0f);   slot2.anchorMax = new Vector2(1f, 0.5f);
            slot2.offsetMin = new Vector2(0, 5);      slot2.offsetMax = new Vector2(0, -5);
            slot2.gameObject.name = "Attack";
            slot2.gameObject.SetActive(true);

            if (a1Name != null) a1Name.gameObject.name = "AbilityName";
            if (a1Cost != null) a1Cost.gameObject.name = "AbilityLabel";
            if (a1Dmg  != null) a1Dmg.gameObject.name  = "UnusedDamageField";
            if (a1Desc != null) a1Desc.gameObject.name = "AbilityDescription";

            if (a2Name != null) a2Name.gameObject.name = "AttackName";
            if (a2Cost != null) a2Cost.gameObject.name = "AttackCost";
            if (a2Dmg  != null) a2Dmg.gameObject.name  = "AttackDamage";
            if (a2Desc != null) a2Desc.gameObject.name = "AttackDescription";
        }
    }

    private static void PopulateAbility(PokemonCardData data,
        TextMeshProUGUI costTMP, TextMeshProUGUI nameTMP,
        TextMeshProUGUI damageTMP, TextMeshProUGUI descTMP)
    {
        if (costTMP != null)
        {
            float w = 250f + (string.IsNullOrEmpty(data.ability.abilityName) ? 0f : data.ability.abilityName.Length * 14f);
            costTMP.text = $"<color=#1E222B><b>Ability</b></color> <color=#C1121F><b>[{data.ability.abilityName}]</b></color>";
            costTMP.rectTransform.anchoredPosition = new Vector2(25, 32);
            costTMP.rectTransform.sizeDelta        = new Vector2(w, 36);
        }
        if (nameTMP   != null) nameTMP.text   = "";
        if (damageTMP != null) damageTMP.text = "";
        if (descTMP   != null) descTMP.text   = data.ability.abilityDescription;
    }

    private static void PopulateAttack(
        string costText, int damage, string description,
        TextMeshProUGUI costTMP, TextMeshProUGUI nameTMP,
        TextMeshProUGUI damageTMP, TextMeshProUGUI descTMP)
    {
        if (costTMP != null)
        {
            costTMP.text = $"<color=#1E222B><b>Attack</b></color> {FormatCostText(costText)}";
            costTMP.rectTransform.anchoredPosition = new Vector2(25, 32);
            costTMP.rectTransform.sizeDelta        = new Vector2(GetCostWidth(costText), 36);
        }
        if (nameTMP   != null) nameTMP.text   = "";
        if (damageTMP != null) damageTMP.text = damage > 0 ? $"<color=#1E222B><b>{damage}</b></color>" : "";
        if (descTMP   != null) descTMP.text   = description;
    }

    private static void ClearSlot(TextMeshProUGUI n, TextMeshProUGUI c, TextMeshProUGUI d, TextMeshProUGUI desc)
    {
        if (n    != null) n.text    = "";
        if (c    != null) c.text    = "";
        if (d    != null) d.text    = "";
        if (desc != null) desc.text = "";
    }

    // ── Style helpers ───────────────────────────────────────────────────────────

    private static void ApplyAnchor(TextMeshProUGUI tmp, Vector2 anchor, Vector2 pivot)
    {
        if (tmp == null) return;
        tmp.rectTransform.anchorMin = anchor;
        tmp.rectTransform.anchorMax = anchor;
        tmp.rectTransform.pivot     = pivot;
    }

    private static void ApplyCostStyle(TextMeshProUGUI tmp)
    {
        if (tmp == null) return;
        tmp.enableAutoSizing   = false;
        tmp.fontSize           = 28;
        tmp.enableWordWrapping = false;
    }

    private static void ApplyDescriptionStyle(TextMeshProUGUI tmp, Vector2 anchor, Vector2 pivot)
    {
        if (tmp == null) return;
        tmp.rectTransform.anchorMin        = anchor;
        tmp.rectTransform.anchorMax        = anchor;
        tmp.rectTransform.pivot            = pivot;
        tmp.rectTransform.anchoredPosition = new Vector2(25, 14);
        tmp.rectTransform.sizeDelta        = new Vector2(530, 70);
        tmp.fontStyle          = FontStyles.Bold;
        tmp.enableAutoSizing   = true;
        tmp.fontSizeMin        = 16;
        tmp.fontSizeMax        = 22;
        tmp.enableWordWrapping = true;
        tmp.color              = new Color(0.12f, 0.14f, 0.17f, 0.95f);
    }

    // ── Cost width & rich-text formatting ───────────────────────────────────────

    private static float GetCostWidth(string costText)
    {
        if (string.IsNullOrEmpty(costText)) return 250f;
        int active = 0;
        foreach (var part in costText.Split(new char[]{' '}, System.StringSplitOptions.RemoveEmptyEntries))
        {
            string t = part.Trim().ToUpper();
            if (t.StartsWith("[") && t.EndsWith("]")) t = t.Substring(1, t.Length - 2);
            if (t != "C" && t != "COLORLESS") active++;
        }
        return 250f + active * 85f;
    }

    /// <summary>Converts raw cost strings (e.g. "W F C") into coloured rich-text badges.</summary>
    public static string FormatCostText(string rawCost)
    {
        if (string.IsNullOrEmpty(rawCost)) return "";
        var parts = new List<string>();
        foreach (var token in rawCost.Split(' '))
        {
            string t = token.Trim().ToUpper();
            if (t.StartsWith("[") && t.EndsWith("]")) t = t.Substring(1, t.Length - 2);
            switch (t)
            {
                case "C": case "COLORLESS": break; // Colorless costs are intentionally hidden
                case "G": case "GRASS":      parts.Add("<color=#1E5F34><b>[Grass]</b></color>");     break;
                case "F": case "FIRE":       parts.Add("<color=#9B2226><b>[Fire]</b></color>");      break;
                case "W": case "WATER":      parts.Add("<color=#0A4F8F><b>[Water]</b></color>");     break;
                case "L": case "LIGHTNING":  parts.Add("<color=#B58A03><b>[Lightning]</b></color>"); break;
                case "P": case "PSYCHIC":    parts.Add("<color=#5A189A><b>[Psychic]</b></color>");   break;
                case "FTR": case "FIGHT": case "FIGHTING":
                    parts.Add("<color=#7F3F10><b>[Fighting]</b></color>"); break;
                case "D": case "DARK": case "DARKNESS":
                    parts.Add("<color=#1A2530><b>[Darkness]</b></color>"); break;
                case "M": case "METAL": case "STEEL":
                    parts.Add("<color=#4E5E60><b>[Metal]</b></color>"); break;
                case "Y": case "DRAGON":
                    parts.Add("<color=#B05C00><b>[Dragon]</b></color>"); break;
                default:
                    if (!string.IsNullOrEmpty(token)) parts.Add($"<b>[{token}]</b>"); break;
            }
        }
        return string.Join(" ", parts);
    }
}
