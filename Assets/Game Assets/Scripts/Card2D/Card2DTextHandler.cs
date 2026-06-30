using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Populates the basic text labels and Pokémon sprite image on a 2D card:
/// name, HP, type, pokedex class.
/// </summary>
public static class Card2DTextHandler
{
    public static void Apply(
        PokemonCardData cardData,
        TextMeshProUGUI nameText,
        TextMeshProUGUI hpText,
        TextMeshProUGUI typeText,
        TextMeshProUGUI pokedexClassText,
        Image pokemonImage)
    {
        if (nameText != null)         nameText.text         = cardData.pokemonName;
        if (hpText != null)           hpText.text           = $"{cardData.hp} HP";
        if (typeText != null)         typeText.text         = cardData.cardType.ToString();
        if (pokedexClassText != null) pokedexClassText.text = cardData.pokedexClass;

        if (pokemonImage != null)
        {
            if (Application.isPlaying && !string.IsNullOrEmpty(cardData.imageUrl))
            {
                pokemonImage.enabled = false; // Temporary off or placeholder state
                ImageCacheManager.Instance.LoadImage(cardData.imageUrl, (Sprite downloadedSprite) =>
                {
                    if (pokemonImage == null) return;
                    if (downloadedSprite != null)
                    {
                        pokemonImage.sprite = downloadedSprite;
                        pokemonImage.enabled = true;
                    }
                    else
                    {
                        // Fallback to local sprite if download fails
                        pokemonImage.sprite = cardData.pokemonSprite;
                        pokemonImage.enabled = cardData.pokemonSprite != null;
                    }
                });
            }
            else
            {
                pokemonImage.sprite  = cardData.pokemonSprite;
                pokemonImage.enabled = cardData.pokemonSprite != null;
            }
        }
    }
}
