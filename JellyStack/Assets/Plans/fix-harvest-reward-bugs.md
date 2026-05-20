# Project Overview
- Game Title: Jelly Stack (Card stacking harvest game)
- High-Level Concept: A game where players stack cards (like Villagers on Berry Bushes) to perform tasks and get rewards.
- Players: Single player
- Inspiration: Stacklands
- Target Platform: PC (StandaloneWindows64)
- Render Pipeline: Built-in

# Bug Analysis
1. **ProgressTask.cs (Infinite Spawn)**:
   - **Cause**: `Destroy(this)` is processed at the end of the frame. If `onComplete` triggers an exception or if `Update` is called again before destruction, it can lead to multiple `Complete()` calls. If an exception occurs during `onComplete`, `Destroy(this)` might never be reached, and if `enabled = false` doesn't stop it (e.g. if re-enabled or if it's the same frame), it spawns infinitely.
   - **Solution**: Use an `isCompleted` boolean flag to guard `Complete()` and a `try...finally` block to ensure `Destroy(this)` is called even if errors occur.

2. **CardSpawner.cs (NullReferenceException)**:
   - **Cause**: `GetComponent<Card>()` returns null because the `ResouceCard` prefab (used for rewards like Berries) is missing the `Card` component.
   - **Solution**: Add a null check for the `card` variable and log an error. Also, ensure the prefab is fixed.

3. **ResourceCardUI.cs (NullReferenceException)**:
   - **Cause**: `card` field is initialized in `Start()`, but it might be accessed earlier, or `GetComponent<Card>()` fails because the component is missing on the prefab.
   - **Solution**: Use `Awake()` for initialization and `GetComponentInParent<Card>()` as a fallback. Add null checks before accessing `card.data`.

# Key Asset & Context
- `Assets/Script/ProgressTask.cs`: Handles the timing and completion of card tasks.
- `Assets/Script/Card/CardSpawner.cs`: Handles instantiating new cards from data.
- `Assets/Script/Card/ResourceCardUI.cs`: Handles displaying icon and name on resource cards.
- `Assets/1.Prefab/Card/ResouceCard.prefab`: The reward card prefab missing the `Card` script.

# Implementation Steps

## 1. Fix ProgressTask Logic
- Modify `Assets/Script/ProgressTask.cs`.
- Add `private bool isCompleted;`.
- Update `Complete()` to use the flag and `try...finally`.
- Update `Update()` to check the flag.

## 2. Fix CardSpawner Robustness
- Modify `Assets/Script/Card/CardSpawner.cs`.
- Add null checks after `GetComponent<Card>()` in `Spawn` and `SpawnIntoStack`.
- Log an error if `Card` component is missing.

## 3. Fix ResourceCardUI Initialization
- Modify `Assets/Script/Card/ResourceCardUI.cs`.
- Move initialization to `Awake()`.
- Use `GetComponent<Card>()` and fallback to `GetComponentInParent<Card>()`.
- Add null guards in `UpdateUI()`.

## 4. Fix ResouceCard Prefab
- Add the `Card` component to the `ResouceCard` prefab located at `Assets/1.Prefab/Card/ResouceCard.prefab`.
- Set the `BoxCollider` size or properties if needed (though it already has one).

# Verification & Testing
1. **Manual Test**: Stack a Villager on a Berry Bush.
2. **Verify Harvest**: Wait for the progress bar to finish.
3. **Verify Spawn**: Check if Berry cards appear without errors.
4. **Verify Infinite Spawn**: Ensure only the correct number of rewards spawn (based on `recipe.resultCount`).
5. **Console Check**: Ensure no NullReferenceExceptions are thrown.
