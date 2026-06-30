using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Resolves and applies the background sprite for a 2D Pokémon card
/// based on its type or a custom override on the ScriptableObject.
/// </summary>
public static class Card2DBackgroundHandler
{
    public static void Apply(
        PokemonCardData cardData,
        Image bgImage,
        List<Card2DUIController.TypeSpriteMapping> typeBackgrounds,
        Sprite defaultFrontSprite)
    {
        if (bgImage == null) return;

        if (Application.isPlaying && !string.IsNullOrEmpty(cardData.customBackgroundUrl))
        {
            ImageCacheManager.Instance.LoadImage(cardData.customBackgroundUrl, (Sprite downloadedSprite) =>
            {
                if (bgImage == null) return;
                if (downloadedSprite != null)
                {
                    bgImage.sprite = downloadedSprite;
                }
                else
                {
                    ApplyFallback(cardData, bgImage, typeBackgrounds, defaultFrontSprite);
                }
            });
        }
        else
        {
            ApplyFallback(cardData, bgImage, typeBackgrounds, defaultFrontSprite);
        }
    }

    private static void ApplyFallback(
        PokemonCardData cardData,
        Image bgImage,
        List<Card2DUIController.TypeSpriteMapping> typeBackgrounds,
        Sprite defaultFrontSprite)
    {
        Sprite bgSprite = null;

        // 1. Custom override wins
        if (cardData.customBackgroundSprite != null)
        {
            bgSprite = cardData.customBackgroundSprite;
        }
        else
        {
            // 2. Match by card type
            foreach (var mapping in typeBackgrounds)
            {
                if (mapping.type == cardData.cardType)
                {
                    bgSprite = mapping.backgroundSprite;
                    break;
                }
            }
        }

        // 3. Fall back to default
        if (bgSprite == null) bgSprite = defaultFrontSprite;
        bgImage.sprite = bgSprite;
    }
}
