using UnityEngine;

public enum PokemonType
{
    Grass,
    Fire,
    Water,
    Lightning,
    Psychic,
    Fighting,
    Darkness,
    Metal,
    Dragon,
    Normal
}

[CreateAssetMenu(fileName = "NewPokemonCard", menuName = "Pokemon TCG/Pokemon Card Data")]
public class PokemonCardData : ScriptableObject
{
    [Header("Basic Information")]
    public string pokemonName = "Bulbasaur";
    public int hp = 60;
    public PokemonType cardType = PokemonType.Grass;
    public Sprite pokemonSprite;

    [Header("Attack 1")]
    public string attack1Name = "Tackle";
    public string attack1CostText = "C"; // C for Colorless/Normal, G for Grass, F for Fire, etc.
    public int attack1Damage = 10;
    [TextArea(2, 5)]
    public string attack1Description = "A basic tackle attack.";

    [Header("Attack 2")]
    public string attack2Name = "Vine Whip";
    public string attack2CostText = "G C";
    public int attack2Damage = 30;
    [TextArea(2, 5)]
    public string attack2Description = "Whip the target with vines.";

    [Header("Stats & Rarity")]
    public PokemonType weakness = PokemonType.Fire;
    public PokemonType resistance = PokemonType.Water;
    [Range(0, 4)]
    public int retreatCost = 1;
    [Range(1, 5)]
    public int rarityStars = 1;
}
