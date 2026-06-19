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

[System.Serializable]
public struct PokemonAbility
{
    public bool hasAbility;
    public string abilityName;
    [TextArea(2, 5)]
    public string abilityDescription;
}

[CreateAssetMenu(fileName = "NewPokemonCard", menuName = "Pokemon TCG/Pokemon Card Data")]
public class PokemonCardData : ScriptableObject
{
    [Header("Basic Information")]
    public string pokemonName = "Bulbasaur";
    public int hp = 60;
    public string stage = "Basic";
    public string pokedexNo = "#001";
    public string pokedexClass = "Seed Pokémon";
    public PokemonType cardType = PokemonType.Grass;
    public Sprite pokemonSprite;
    public Sprite customBackgroundSprite;

    [Header("Attack 1")]
    public string attack1Name = "Tackle";
    public string attack1CostText = "C"; // C for Colorless/Normal, G for Grass, F for Fire, etc.
    public int attack1Damage = 10;
    [TextArea(2, 5)]
    public string attack1Description = "A basic tackle attack.";



    [Header("Ability")]
    public PokemonAbility ability;

    [Header("Stats & Rarity")]
    public PokemonType weakness = PokemonType.Fire;
    public string weaknessValue = "×2";
    public PokemonType resistance = PokemonType.Water;
    public string resistanceValue = "-30";
    public bool hasResistance = false;
    [Range(0, 4)]
    public int retreatCost = 1;
    [Range(1, 5)]
    public int rarityStars = 1;
}
