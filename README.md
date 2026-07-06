# Pokémon TCG Pocket Clone - Companion App (Card Outpost)

A high-performance, polished Pokémon Trading Card Game (TCG) companion MVP built in Unity. This project simulates booster pack openings, collection tracking, real-time search/filter/sort capabilities, 3D/2D card inspection, and persistent local storage.

---

## 🛠️ Project Setup & Installation

Follow these steps to run the project in your local development environment:

### Prerequisites
* **Unity Version:** Unity 6 LTS (or Unity 2022.3+).
* **Render Pipeline:** Universal Render Pipeline (URP).
* **Target Platform:** Android (APK built and tested on physical devices) or iOS.

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
   * In the Project view, navigate to [Assets/Game Assets/Scenes/Game.unity](file:///Users/santosh-parihar/PokemonNew/Assets/Game%20Assets/Scenes/Game.unity) and open it.
4. **Setup UI Layouts (Automated Editor Script):**
   * In the top Unity menu bar, go to **Pokemon TCG > Setup Main Scene UI**. This automatically instantiates the collection scroll view, builds UI elements, updates the card template prefabs, and binds all references.
5. **Run the Project:**
   * Click the **Play** button in the Unity Editor to start.
   * Toggle **Simulator** mode in the Game view to test various mobile screen aspect ratios.

---

## 🚀 Key Features & Algorithms Used

Below is a detailed breakdown of the technical features and the algorithms behind them:

### 1. Booster Pack Opening (Probability-Weighted Drop Algorithm)
* **Feature:** Opens booster packs and awards 5 randomized cards to the player's collection.
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

### 4. Smart Multi-Tier Image Caching
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

## 📐 Architectural Decisions

* **Data-Driven Architecture:** We chose Unity `ScriptableObjects` (`PokemonCardData`) to store card specifications. This allows designers to add new cards via the Inspector without modifying code.
* **Decoupled Handlers (SOLID):** We split the 2D Card UI controller into dedicated handlers (`Card2DBackgroundHandler`, `Card2DTextHandler`, `Card2DAttackHandler`, `Card2DBadgeHandler`) to prevent the "God Class" anti-pattern.
* **Save State Persistence:** Local persistence is handled by JSON-serializing the collection dictionary of `cardId` and duplicate counts, saving it securely in `PlayerPrefs`.

---

## ⏳ Time Spent & Project Walkthrough

Refer to [project_timesheet.md](file:///Users/santosh-parihar/PokemonNew/project_timesheet.md) for the full day-by-day progress log.

* **Total Time Spent:** 75 Hours (over 4 weeks, averaging ~20 hours per week).
* **Core Focus Areas:**
  * **Week 1:** Core 3D/2D card prefab structure, double-sided render logic, and texture processing.
  * **Week 2:** Game loop integration, PrimeTween packs animations, search filtering, and layouts.
  * **Week 3:** Custom Editor Helper tools, local player collection saves, and responsive grid layouts.
  * **Week 4:** Sound system integration, build stabilization for mobile Qualcomm GPUs, and edge-case bug fixing.

---

## 🔮 Future Roadmap (If We Had 40 More Hours)

1. **Online Multiplayer Trades:** Replace local saves with a secure Firebase/PlayFab backend to support 1v1 trades and real-time multiplayer card exchanges.
2. **Physical Camera Scanning:** Integrate OpenCV or Google ML Kit into Unity to scan physical Pokémon cards directly from a device camera.
3. **Advanced Stats Dashboard:** Add analytics dashboards (e.g., historical price charts, collection completion percentages, and rare card distribution heatmaps).
4. **Enhanced 3D Holo Shaders:** Add custom holographic and foil shader effects to card meshes that react to device gyroscope rotation sensors.
