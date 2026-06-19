using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Static service layer for the player's card collection and daily pack cooldown.
/// All state is persisted to PlayerPrefs so it survives between play sessions.
/// </summary>
public static class PlayerCollection
{
    // ─── PlayerPrefs keys ───────────────────────────────────────────────────
    private const string OWNED_KEY        = "OwnedCards";          // Comma-separated card names
    private const string LAST_PACK_KEY    = "LastPackOpenedTicks";  // DateTime ticks (long)
    private const double COOLDOWN_HOURS   = 1.0 / 3600.0; // ← 1 second (change back to 24.0 for release)

    // ────────────────────────────────────────────────────────────────────────
    // Owned cards
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>Returns the set of card names the player currently owns.</summary>
    private static HashSet<string> LoadOwnedNames()
    {
        string raw = PlayerPrefs.GetString(OWNED_KEY, "");
        var set = new HashSet<string>(System.StringComparer.Ordinal);
        if (!string.IsNullOrEmpty(raw))
        {
            foreach (var name in raw.Split(','))
            {
                if (!string.IsNullOrEmpty(name)) set.Add(name);
            }
        }
        return set;
    }

    private static void SaveOwnedNames(HashSet<string> names)
    {
        PlayerPrefs.SetString(OWNED_KEY, string.Join(",", names));
        PlayerPrefs.Save();
    }

    /// <summary>Adds a card to the player's owned collection (idempotent).</summary>
    public static void AddCard(PokemonCardData card)
    {
        if (card == null) return;
        var names = LoadOwnedNames();
        names.Add(card.pokemonName);
        SaveOwnedNames(names);
    }

    /// <summary>Adds a batch of cards at once (used at end of pack reveal).</summary>
    public static void AddCards(IEnumerable<PokemonCardData> cards)
    {
        var names = LoadOwnedNames();
        foreach (var card in cards)
        {
            if (card != null) names.Add(card.pokemonName);
        }
        SaveOwnedNames(names);
    }

    /// <summary>Returns true if the player owns the given card.</summary>
    public static bool OwnsCard(PokemonCardData card)
    {
        if (card == null) return false;
        return LoadOwnedNames().Contains(card.pokemonName);
    }

    /// <summary>
    /// Filters the master pool down to only cards the player owns.
    /// Returns an empty list if the player owns nothing yet.
    /// </summary>
    public static List<PokemonCardData> GetOwnedCards(List<PokemonCardData> masterPool)
    {
        var owned = LoadOwnedNames();
        var result = new List<PokemonCardData>();
        foreach (var card in masterPool)
        {
            if (card != null && owned.Contains(card.pokemonName))
                result.Add(card);
        }
        return result;
    }

    /// <summary>Debug helper — clears all owned cards. Call from Inspector context menu.</summary>
    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(OWNED_KEY);
        PlayerPrefs.DeleteKey(LAST_PACK_KEY);
        PlayerPrefs.Save();
        Debug.Log("[PlayerCollection] All data cleared.");
    }

    // ────────────────────────────────────────────────────────────────────────
    // Daily free pack cooldown
    // ────────────────────────────────────────────────────────────────────────

    private static System.DateTime LastPackOpenedTime()
    {
        string raw = PlayerPrefs.GetString(LAST_PACK_KEY, "0");
        long ticks;
        return long.TryParse(raw, out ticks) && ticks > 0
            ? new System.DateTime(ticks, System.DateTimeKind.Utc)
            : System.DateTime.MinValue;
    }

    /// <summary>Returns true when the player is allowed to open a free pack.</summary>
    public static bool CanOpenFreePack()
    {
        System.TimeSpan elapsed = System.DateTime.UtcNow - LastPackOpenedTime();
        return elapsed.TotalHours >= COOLDOWN_HOURS;
    }

    /// <summary>Returns the remaining cooldown time, or TimeSpan.Zero if ready.</summary>
    public static System.TimeSpan CooldownRemaining()
    {
        System.TimeSpan elapsed = System.DateTime.UtcNow - LastPackOpenedTime();
        if (elapsed.TotalHours >= COOLDOWN_HOURS) return System.TimeSpan.Zero;
        return System.TimeSpan.FromHours(COOLDOWN_HOURS) - elapsed;
    }

    /// <summary>Records that the player just opened a pack (starts the 24h cooldown).</summary>
    public static void RecordPackOpened()
    {
        PlayerPrefs.SetString(LAST_PACK_KEY, System.DateTime.UtcNow.Ticks.ToString());
        PlayerPrefs.Save();
    }
}
