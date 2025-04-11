# Astral Drone Quest

## About

This university final project involved creating an arcade-style game in Unity with simple controls and compelling gameplay. During 20 weeks (part-time), multiple prototypes were developed and refined following player feedback.

Astral Drone Quest is an action-adventure game set in outer space, where the player's goal is to fly between sectors, upgrading their abilities by defeating enemies. Special emphasis was placed on creating a challenge for players of varying skill levels, as well as replayability through procedurally generated worlds and a final time score.

Code, art, and animation are original.

## Interesting sections of code

**GD-FP/Assets/Scripts/PlayerMovement.cs**  
Contains the movement curve, an eleven-parameter acceleration-based system with a player-activated dash.

**GD-FP/Assets/Scripts/Planet.cs**  
Controls the behaviour of planets, implementing physics laws to model accurate planetary gravity.

**GD-FP/Assets/Scripts/Generation.cs**  
Implements a novel algorithm to procedurally generate randomized rectangle-based fractal patterns which follow certain rules.

**GD-FP/Assets/Scripts/EnemyScripts/PlanetguardBossEnemy.cs**  
Contains state and coroutine logic for the game's first boss.

**GD-FP/Assets/Scripts/Rocket.cs**  
Contains tracking and detonation logic for one of the enemy attacks.
