# CRITICAL BUGS / SOFTLOCKS
* [ ] SOFTLOCK: Player gets stuck and cannot select anyone after choosing a grenade, clicking it multiple times, and then throwing it. (Suspected to also occur with medkits).
* [?] SOFTLOCK/CRASH: Game crashes (NullReferenceException) during Megatron-containing map load, resulting in mission failure (everyone dead, 0 Juice) and run loss.
  * [ ] Investigate if units are dying off-screen or if it's purely a load failure.
* [x] GAMEPLAY BLOCKER: Player getting blocked/stuck in the first room, sometimes near a cryopod. (Happened multiple times).
* [x] GAMEPLAY BLOCKER: Megatron being flung by grenade explosion causes unspecified issues (needs more info if it's a consistent blocker).

# GENERAL BUGS & ISSUES

## Starting Sequence & Main Menu
* [ ] BUG: Game doesn't load directly into the main menu (requires F4 presses).
* [ ] BUG: Pressing a key during intro text animation skips the slide instead of fully revealing the current text first.
* [ ] BUG: Pressing Escape in the main menu reopens the last chosen sub-window (Options/Help) instead of consistently returning to the main view.
* [ ] UI BUG: Cursor icon doesn't revert from "Exit" state if hovering over an exit while the map menu is loading.

## In-Game - Gameplay & Mechanics
* [x] BUG: Search progress bar doesn't disappear if the searching unit is interrupted by movement (RMB).
* [x] BUG: Skills are behaving weirdly or not stacking as expected (verify intended functionality).
* [ ] BUG: Upgraded item stats don't update immediately in the inventory UI (e.g., shows +1 instead of +2) until assigned to a unit.
* [ ] BUG: Flamethrower fire can traverse the entire map; it needs a defined range/lifespan.
* [ ] BUG: No way to close a unit's inventory with the Esc key while on the station.
* [ ] BUG: Options settings (e.g., sound) are not applied in-game, only in the main menu.
* [ ] BUG: Units can sometimes move diagonally between tightly spaced objects (e.g., two beds), potentially leading to them getting stuck.
* [ ] BUG: Cannot deselect a grenade (or similar item) after initiating its use but before confirming a target.
* [x] BUG: Damage sometimes visually displayed as "0" (refer to screenshot).
* [ ] BUG: Enemies (Sniper Guy, Megatron, Alien dude) can spawn directly on top of player units when loading onto a map.
* [ ] BUG (Collision): Units (e.g., zombies) can be pushed/shoved through walls.
* [ ] BUG: Exit arrow sometimes missing or stops working.

## In-Game - UI & Visuals
* [x] UI BUG: Skill buttons aren't grayed out when the player has no skill points to spend.
* [ ] UI BUG: Text in the options menu is weirdly centered (refer to screenshot).
* [ ] LEVEL DESIGN: Unreachable loot reported (refer to screenshot).

# AUDIO ISSUES
* [ ] AUDIO: Sounds (general) are reported as very quiet.
* [ ] AUDIO: Intense music track has a lot of empty space at the start, causing an abrupt cut-off feel instead of a build-up.
* [ ] AUDIO BUG: Specific sounds (shooting, picking up, hit) are sometimes extremely loud or "cut off."
* [ ] AUDIO: Review "UI sounds" setting – implement UI sounds or remove/clarify the setting if none exist.

# TEXT & LOCALIZATION FIXES
* [ ] TEXT: Intro - Add period at the end of the first sentence.
* [ ] TEXT: Intro - Remove unnecessary comma before "derelict."
* [ ] TEXT: Intro - Standardize apostrophe usage (use regular `'` consistently, not `’`).
* [x] TEXT: Change "Land" button/text to "Dock" or a more appropriate term for space stations.
* [x] TEXT (Help Menu):
  * [x] Add hyphen: "F4 - go back to menu".
  * [x] Clarify: "LMB - click/drag to select".
  * [x] Clarify: "I - open/close inventory" (or "Toggle inventory").
* [x] GRAMMAR: Review sentence: "Exploration phase ends when all your deployed crew either escaped or died." (Suggests: "...escapes or dies" or "...have escaped or died").

# UX IMPROVEMENTS & FEATURE REQUESTS (Preferences & Ideas)

## UI/UX Enhancements
* [ ] UX: Allow clicking to skip to the next intro slide.
* [ ] UX (Consider): Add a dedicated "Skip Intro" button.
* [ ] UX (Review): Intuition of clicking an already selected unit to close their inventory (causes frustration for tester).
* [ ] UX (Consider): Enhance current location indicator on map node (e.g., "YOU ARE HERE" text, especially at game start).
* [ ] UX (Consider): Differentiate visual cues for exit arrows vs. arrows to units/people.
* [ ] UX (Consider): Display a summary of a unit's active skill effects when hovering over them.
* [ ] UX (Review): Unit inventory feels clunky; e.g., changing weapons moves the new weapon to the bottom of the inventory.
* [ ] UX (Consider): Add a small icon on unit portraits/cards indicating they have unspent skill points.
* [ ] UX (Consider): Add a "> " or similar visual cue to the clickable "We are ready" Megatron text to make it more obviously interactive.
* [ ] UX (Content): Hacking Device description should include what it actually does.
* [ ] UX (Consider): Add an in-game options window, at least for sound settings.

## Gameplay Enhancements
* [ ] GAMEPLAY (Consider): Implement Shift+RMB (or similar) to force movement/override current action for a busy unit.
* [ ] GAMEPLAY (Consider): Allow "Embark" actions to span multiple map nodes*
