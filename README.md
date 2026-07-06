# Pokémon TCG Pocket  - Companion App (Card Outpost)

A polished Pokémon Trading Card Game (TCG) companion MVP built in Unity. This project simulates booster pack openings, collection tracking, real-time search/filter/sort capabilities, 3D/2D card inspection, and persistent local storage.

---

## 🛠️ Project Setup & Installation

Follow these steps to run the project in your local development environment:

### Prerequisites
* **Unity Version:** Unity 6 LTS (or Unity 2022.3+).
* **Render Pipeline:** Universal Render Pipeline (URP).
* **Target Platform:** Android (APK built and tested on physical devices).

### Setup Steps
1. **Clone the Repository:**
   ```bash
   git clone git@github.com:santoshParihar/Pokemon.git
   cd Pokemon
   ```
2. **Open in Unity Hub:**
   * Open Unity Hub, click **Add > Add project from disk**, and select the cloned `Pokemon` folder.
   * Open the project using **Unity 6** (or your active LTS version).
3. **Open the Gameplay Scene:**
   * In the Project view, navigate to `Assets/Game Assets/Scenes/Game.unity` and open it.
4. **Card Creator Window (Editor Script):**
   * In the top Unity menu bar, go to **Pokemon TCG > Card Creator Window** to open the interactive interface where you can configure parameters (Width, Height, Thickness, Corner Radius, Rarity, etc.) and bake 3D card meshes or generate Card templates.
5. **Run the Project:**
   * Click the **Play** button in the Unity Editor to start.
   * Toggle **Simulator** mode in the Game view to test the mobile layout.

---

## 🚀 Key Features & Algorithms Used

Below is a detailed breakdown of the technical features and the algorithms behind them:

### 1. Booster Pack Opening (Probability-Weighted Drop Algorithm)
* **Feature:** Opens booster packs and awards 3 randomized cards to the player's collection.
* **Algorithm:** **Cumulative Weight Selection Algorithm**. 
  * Rather than picking cards purely at random, each card ScriptableObject has a `dropProbability` (weight).
  * We sum the weights of all cards to calculate a `totalWeight`. 
  * A random float between `0` and `totalWeight` is generated.
  * We iterate through the card list and sum their weights; as soon as the running sum meets or exceeds the random value, that card is selected.
  * This supports arbitrary rarity ratios (e.g., Ultra Rare cards drop far less frequently than Commons).

### 2. Real-Time Collection Search (Linear Scan Filtering)
* **Feature:** Filters the card grid instantly as the user types.
* **Algorithm:** **Linear String Matching ($O(N)$)**.
  * Input queries are sanitized (trimmed and converted to lowercase).
  * We run a fast linear scan using C#'s `string.Contains(query, StringComparison.OrdinalIgnoreCase)` on the `pokemonName` field of all owned cards.
  * To avoid GC allocations and stuttering, we do not destroy GameObjects. Instead, we toggle their active state (`gameObject.SetActive(true/false)`) to filter them visually in real-time.

### 3. Collection Sorting (IntroSort & UI Sibling Reordering)
* **Feature:** Sorts the collection by Name (A-Z / Z-A) and Market Price (Low to High / High to Low).
* **Algorithm:** **IntroSort ($O(N \log N)$)**.
  * Sorting is performed on the in-memory card list using C#'s native `List<T>.Sort()` (which implements IntroSort, a hybrid of QuickSort, HeapSort, and Insertion Sort).
  * After sorting the data, we update the UI order in real-time by rearranging the sibling indices of the UI cards using `transform.SetSiblingIndex()`. This utilizes Unity’s Auto-Layout Group to reposition the grid layout instantly with zero layout rebuilding overhead.

### 4.  Image Caching
* **Feature:** Loads card images asynchronously from URLs without blocking the main thread or causing network bottlenecks.
* **Algorithm:** **Multi-Tier Cached Remote Loading**.
  * **Memory Cache (Tier 1 - RAM):** Fast lookups using a C# `Dictionary<string, Sprite>`.
  * **Disk Cache (Tier 2 - Storage):** Files are saved locally using an MD5 hash of the card URL as the filename.
  * **Network Fetch (Tier 3 - Fallback):** Fetches the texture using `UnityWebRequestTexture`, bakes the sprite, updates Tier 1 and Tier 2 caches, and renders the image.

### 5. High-Performance Animations
* **Feature:** Smooth card packs shaking, tearing, and flipping in 3D.
* **Algorithm:** **Zero-Allocation Tweening (PrimeTween)**.
  * Card motion, scale pops, and rotations are driven by **PrimeTween** which operates with `0 GC Alloc` to avoid garbage collection spikes on mobile devices.
  * Built-in fallback coroutine-based interpolations handle transitions smoothly if the package is missing.

---

## 📁 Project Directory Structure

Below is the directory tree showing where the key assets and files of the project are located:

```text
Assets/
├── Game Assets/
│   ├── Audio/              # Sound effects (card flips, pack tearing, bg music)
│   ├── Data/               # ScriptableObject card data assets (.asset)
│   ├── Materials/          # Render materials (Card front, back, borders)
│   ├── Meshes/             # Baked 3D card geometry meshes
│   ├── Prefabs/            # 3D and 2D card UI templates
│   ├── Scenes/             # Core gameplay scene (Game.unity)
│   ├── Scripts/            # C# logic, handlers, and editor tools
│   └── Textures/           # Card UI sprites and energy icon textures
└── Plugins/
    └── PrimeTween/         # High-performance tweening animation library
```

---

## 📦 Third-Party Dependencies

The project relies on these libraries for performance and UI quality:

* **PrimeTween:** A highly optimized, zero-garbage-collection tweening library used to handle animations (card flipping, pack shaking, UI movement) smoothly on mobile hardware.
* **TextMeshPro:** Used for clean, sharp, and pixel-perfect card text rendering at any resolution.
* **Universal Render Pipeline (URP):** Serves as the primary rendering engine to support lightweight mobile-optimized graphics and custom shaders.

---

## 📐 Architectural Decisions

* **Data-Driven Architecture:** We chose Unity `ScriptableObjects` (`PokemonCardData`) to store card specifications. This allows designers to add new cards via the Inspector without modifying code.
* **Decoupled Handlers (SOLID):** We split the 2D Card UI controller into dedicated handlers (`Card2DBackgroundHandler`, `Card2DTextHandler`, `Card2DAttackHandler`, `Card2DBadgeHandler`) to prevent the "God Class" anti-pattern.
* **Save State Persistence:** Local persistence is handled by JSON-serializing the collection dictionary of `cardId` and duplicate counts, saving it securely in `PlayerPrefs`.

---

## ➕ How to Add a New Pokémon

To add a new Pokémon card to the game, you only need to create and register a single data asset:

1. **Create the Data File:**
   * In the Unity Project window, navigate to `Assets/Game Assets/Data/`.
   * Right-click and choose **Create > Pokemon TCG > Pokemon Card Data**.
   * Name the new asset file (e.g., `MewtwoData.asset`).
2. **Configure Card Details:**
   * Select the created asset file and fill in the card parameters in the Inspector window:
     * Basic info (Name, HP, Stage, Pokedex No, Pokedex Class, Card Type).
     * Attack information (Name, energy cost formatting string, Damage, Description).
     * Ability (if applicable).
     * Card stats (Weakness type & multiplier, Resistance type & value, Retreat cost, Rarity stars, Market price, Drop probability weight).
3. **Provide Image URLs:**
   * Enter the dynamic loading web links in the `ImageUrl` and `CustomBackgroundUrl` fields. The game will automatically handle remote downloading, baking, and disk/RAM caching.
4. **Register the Card:**
   * Locate the main scene controller object (e.g., `MainUIManager` or the database container) in your scene hierarchy.
   * Drag your new `PokemonCardData` asset into the **Cards Data** list.

*The game automatically processes the new card data asset, making it available in booster packs, showing it in the collection store, and rendering it in high detail without writing any extra code.*

