# Project Work Log: Pokemon TCG App (June 9 - July 5)


### Week 1: June 9 – June 14 (Total: 20 Hours)
* **June 9 (Tue) — 2 hours**
  * Did research on Pokemon Trading Card Game (TCG) layouts and rules.
  * Planned out the main features we need: pack opening, card viewing in 3D, and the collection grid.
* **June 10 (Wed) — 2 hours**
  * Searched for card templates, UI designs, and suitable backgrounds.
  * Set up the Unity project folder structure and imported basic assets.
* **June 11 (Thu) — 2 hours**
  * Set up the Git repository and pushed the initial Unity project files.
  * Configured build settings and standard project preferences.
  * Researched animation systems in Unity, comparing Animator controllers versus tweening engines for smooth card-flip and UI animations.
* **June 12 (Fri) — 2 hours**
  * Researched how to render double-sided cards in Unity.
  * Experimented with 3D shaders and standard UI elements.
* **June 13 (Sat) — 6 hours**
  * Sourced the first set of Pokemon names, types, and stats.
  * Started building the 3D card prefab inside Unity.
* **June 14 (Sun) — 6 hours**
  * Finished the basic 3D card prefab.
  * Wrote the script to handle card-facing logic so cards show correct front and back textures.
  * Imported Pokemon images and fixed a rendering issue where card images looked blurry or blue.
  * Added a configurable background script.

---

### Week 2: June 15 – June 21 (Total: 20 Hours)
* **June 15 (Mon) — 2 hours**
  * Created custom 3D meshes for the cards to make them feel more like physical cards.
  * Connected Pokemon data (attacks, HP, name) to the card UI elements.
* **June 16 (Tue) — 2 hours**
  * Built the basic Game UI structure.
  * Coded the script that lets the user rotate cards in 3D when inspecting them.
* **June 17 (Wed) — 2 hours**
  * Sourced and imported PrimeTween to make card movements smooth.
  * Designed the visual container for the card packs.
* **June 18 (Thu) — 2 hours**
  * Programmed the pack opening script to pull cards randomly.
  * Added a shimmer shader and a shaking animation to the card pack to make opening it exciting.
  * Fixed UI rendering issues by using default sprite materials.
* **June 19 (Fri) — 2 hours**
  * Programmed a search bar to filter cards in the collection.
  * Built the summary screen that lists all cards you just opened from a pack.
  * Added a banner image and nicer background details to the main UI.
* **June 20 (Sat) — 5 hours**
  * Sourced and imported more Pokemon into the database.
  * Swapped UI fonts to make text legible and crisp.
  * Fixed alignment and placement bugs in the pack opening screen.
* **June 21 (Sun) — 5 hours**
  * Spent time playtesting the pack opening loop.
  * Cleaned up UI spacing and adjusted the duration of the animations to make them feel faster and smoother.

---

### Week 3: June 22 – June 28 (Total: 20 Hours)
* **June 22 (Mon) — 2 hours**
  * Planned an Editor Helper tool so we can edit card details directly inside Unity without writing code every time.
* **June 23 (Tue) — 2 hours**
  * Wrote the draft script for the card editor helper tool.
* **June 24 (Wed) — 2 hours**
  * Finished the custom Editor interface, allowing quick entry of Pokemon details (HP, type, attacks).
* **June 25 (Thu) — 2 hours**
  * Sourced audio files for the game (pack tearing sound, card flips, button clicks, and bg music).
* **June 26 (Fri) — 2 hours**
  * Researched how to save the player's card collection locally so they don't lose their cards when closing the app.
* **June 27 (Sat) — 5 hours**
  * Set up the grid system for the Collection screen.
  * Made sure the grid automatically adjusts sizes for different screen aspect ratios (mobile vs. PC).
* **June 28 (Sun) — 5 hours**
  * Tested the Editor Helper tool and created new Pokemon cards using it.
  * Resolved a bug where opening multiple cards at once caused them to overlap or flip incorrectly.

---

### Week 4: June 29 – July 5 (Total: 20 Hours)
* **June 29 (Mon) — 2 hours**
  * Created the AudioManager script to handle all sound triggers.
  * Wired up the card flips, button clicks, and pack tearing sounds.
  * Fixed a bug in the editor where already owned cards would show up incorrectly when filtering.
* **June 30 (Tue) — 2 hours**
  * Polished the collection grid animations.
  * Set up card counts (e.g., showing how many duplicates a player owns).
  * Organized project files and documented the progress.
* **July 1 (Wed) — 3 hours**
  * Fixed Unity Editor build issues and configured mobile orientation controls.
  * Resolved graphics API bugs for Qualcomm GPUs by enabling auto-fallback to OpenGL ES 3.
  * Exported and verified the final Android build (`pokemon.apk`).
* **July 2 (Thu) — 2 hours**
  * Tested the Android build on physical devices.
  * Validated image caching, WebRequest loading speed, and offline image retrieval.
* **July 3 (Fri) — 2 hours**
  * Performed codebase cleanup, optimized comments, and deleted temporary asset files.
  * Updated README instructions and verified the setup workflow.
* **July 4 (Sat) — 5 hours**
  * Made new builds, fixed multiple UI issues, and resolved logical issues found in the builds.
  * Tested and resolved edge cases during device playtesting.
* **July 5 (Sun) — 4 hours**
  * Made final builds, fixed UI/logical issues, and resolved remaining build edge cases.
  * Verified overall build stability and finalized deliverables.
