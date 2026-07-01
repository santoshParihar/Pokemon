# Pokémon TCG Pocket Clone - Architecture & Workflow Overview

This document provides a simple, high-level overview of how this application works, how data flows, and how to manage the game content. It is designed to help engineers, product managers, or reviewers quickly understand the project workflow without diving too deep into the code.

---

## 1. How the Game Works (Core Game Loop)
The application is a simulated digital card collection and booster pack opening game. The player loop consists of:
1. **The Store:** The player visits the Store to open booster packs.
2. **Booster Packs:** Opening a pack runs a probability-weighted selection algorithm to draw cards.
3. **The Reveal Scene:** The player interacts with 3D cards to flip and reveal their pulls.
4. **My Collection:** Opened cards are instantly saved to the player's permanent collection inventory.
5. **Collection Management:** The player can browse their owned cards, search by name, sort by price or name, and inspect individual cards in 3D.

---

## 2. Card Data Layer & Dynamic Loading
Instead of hardcoding card visuals or bloating the app size with heavy textures, the game uses a dynamic, data-driven approach:

*   **ScriptableObjects (`PokemonCardData`):** Each card's stats (Name, HP, Type, Attacks, Price, Rarity, and Drop Probability) are defined in modular data assets stored in `Assets/Game Assets/Data/`.
*   **Web-based Image Loading:** The card data assets contain image URLs (`imageUrl` and `customBackgroundUrl`).
*   **Smart Cache System (`ImageCacheManager`):** 
    *   When a card is displayed, the game queries the local device memory first.
    *   If not found, it checks the local device storage (disk cache).
    *   If it's the first time loading, it downloads the image from the web, saves a copy to the device's persistent storage (so it works offline next time), and applies it to the card.

---

## 3. How to Add a New Pokémon (Content Management)
To add a new Pokémon card to the game, you only need to create a single data asset:
1. **Create the Data File:** In `Assets/Game Assets/Data/`, right-click and choose **Create > Pokemon TCG > Pokemon Card Data**. Name the file (e.g., `MewtwoData.asset`).
2. **Fill in Details:** Enter the card statistics (HP, Stage, Attacks, Retreat Cost, Weakness, Rarity, and Market Price).
3. **Provide URLs:** Enter the image links in the `ImageUrl` and `CustomBackgroundUrl` fields.
4. **Register with UIManager:** Select the main scene controller (`MainUIManager`) and drag your new asset into the **Cards Data** list.

*The game automatically handles the rest. The new card will immediately become available in packs, register in the store, and render properly in 3D without writing any new code.*

---

## 4. Pack Opening & Card Reveal Workflow
When a player clicks "Open Pack" in the store:
1. **Card Selection (Probability Weighted):** The system looks at all registered card data assets and picks 5 cards. It uses each card's `dropProbability` value as a weight (e.g., common cards have higher weights and drop frequently; rare cards have lower weights and drop rarely).
2. **Collection Save:** The selected card IDs are instantly appended to the player's saved inventory.
3. **Visual Swiping/Tapping:** The 3D Pack Opening view is instantiated. The player swipes to discard wrapping paper and taps/slides cards to rotate and reveal the front face.
4. **Collection Update:** When the player returns to the collection tab, the grid is refreshed with the new cards.

---

## 5. Player Inventory & State Persistence
*   **PlayerPrefs Saving:** The player's collection is saved locally on their device using JSON-serialized player profiles.
*   **Data Integrity:** This ensures that even if the app crashes, is closed, or gets updated, the player's card collection and count of duplicates remain intact.
*   **Testing Tool:** A quick helper menu (**Pokemon TCG > Clear PlayerPrefs**) is included in the editor to let developers reset their collection to empty for testing.

---

## 6. Collection Search & Sort Mechanics
When browsing "My Collection", the user can search and sort cards dynamically:

*   **Search Filter:** 
    *   As the player types, the system scans the text input.
    *   It checks the `pokemonName` field on all owned cards (case-insensitively).
    *   Matching cards are shown; non-matching cards are instantly hidden.
*   **Sorting Logic:**
    *   **Name: A-Z / Z-A:** Compares the alphabetical characters of the Pokémon names.
    *   **Price: Low to High / High to Low:** Compares the `marketPrice` value of the card data assets.
    *   The collection grid repositions the cards in real-time based on the active sort option.
