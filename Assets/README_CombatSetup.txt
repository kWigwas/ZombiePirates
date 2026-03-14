COMBAT FLOW SETUP

1. WORLD SCENE
- Put CombatEncounterTrigger on each enemy.
- Give the enemy a Collider2D set to Is Trigger (or a 3D trigger collider).
- Make sure the player object has the tag "Player".
- Set each enemy difficulty in the inspector:
  - Easy
  - Medium
  - Hard
- Set combatSceneName to the name of your rhythm combat scene.

2. WHAT HAPPENS
- When the player touches the enemy, the combat scene loads.
- A random target percent is generated and shown to the player:
  - Easy = 25% to 50%
  - Medium = 50% to 70%
  - Hard = 70% to 90%

3. COMBAT SCENE
- Add RhythmFightController to an empty GameObject.
- Create 4 UI lanes and assign RhythmLane components.
- Use a UI Image prefab with RhythmNote on it as the note prefab.
- Assign these UI references to RhythmFightController:
  - countdownText
  - feedbackText
  - stateText
  - targetText
  - difficultyText
  - scoreText
  - hitRateText
  - playerProgressSlider
  - targetProgressSlider
  - restartButton
  - returnButton (optional)

4. WIN CONDITION
- The player wins if successful hits / total notes >= generated target percent.
- Example: if the enemy rolls 74%, the player must hit at least 74% of the notes.
- If it becomes mathematically impossible to reach the target, the fight ends early.

5. DEFAULT INPUTS
- A / S / K / L

6. OPTIONAL
- Assign playerShipAnimator and enemyShipAnimator and add an "Attack" trigger.
- Assign an AudioSource with your combat music.
