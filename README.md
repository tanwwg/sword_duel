# Sword Duel

Unity sword-fighting prototype focused on two knights dueling with light/heavy attacks, blocking, parrying, stun, ragdoll death, AI opponents, and Netcode multiplayer.

This README focuses on the custom scripts in `Assets/Scripts`.

## Project Version

- Unity: `6000.3.6f1`
- Main script packages used by the custom code:
  - Unity Netcode for GameObjects
  - Unity Transport
  - Unity Input System
  - Unity Gaming Services Authentication, Relay, and Lobbies
  - Cinemachine
  - TextMesh Pro / UGUI

## Script Layout

```text
Assets/Scripts
├── AI/                 Weighted enemy decision states
├── Inputs/             Local input capture and server-facing input adapters
├── Networking/         Lobby, relay, LAN discovery, and network UI scripts
├── Views/              Animation, UI, ragdoll, camera-facing, and VFX helpers
├── BlockSystem.cs      Block/parry timing logic
├── ComboSystem.cs      Attack state machine for light/heavy attacks
├── GameController.cs   Server-authoritative game loop and respawning
├── HitController.cs    Weapon hit resolution
├── Hittable.cs         Links hit colliders back to a player
├── KnightInfo.cs       Bundles references for one knight
├── NetworkPlayer.cs    Network spawn ownership and input setup
├── PlayerAnimator.cs   Animation state synchronization from gameplay state
├── PlayerController.cs Movement, health, stun, block, attack, and state logic
├── SinglePlayerGameScript.cs Simple local host bootstrap
├── StateWatcher.cs     Generic state-change helper
└── Weapon.cs           Weapon overlap checks and damage data
```

Unity tutorial helper scripts also exist under `Assets/TutorialInfo/Scripts`, but they are not part of the main gameplay code.

## Runtime Flow

`NetworkPlayer.OnNetworkSpawn()` configures each spawned knight. The owning player enables local `PlayerInput`; non-owners and the server read from `RemoteInputHandler`. AI players call `KnightInfo.SetupAi()` so their `EnemyAi` becomes the active input source.

`GameController.RebuildPlayerList()` finds all active `KnightInfo` objects, assigns names, respawns server-side players, links each knight to the other as its target, and fires `onStartGame` once two knights are present.

Every frame, `GameController.Update()` runs the match loop. On the server it checks death/respawn timers and ticks each player using input plus animation state. On all clients it ticks `PlayerAnimator` so visual state follows replicated gameplay state.

Combat is server-authoritative. `ComboSystem` decides the active attack state, `PlayerAnimator.GetAnimState()` reports animation windows such as active hit frames and combo windows, and `HitController.HandleWeaponHit()` resolves weapon overlaps, block/parry outcomes, damage, stun, knockback, and hit effects.

## Core Gameplay Scripts

### `PlayerController.cs`

Owns a knight's gameplay state:

- Movement with `CharacterController`
- Health via a server-writable `NetworkVariable<int>`
- State via `NetworkVariable<PlayerState>`
- Stun via `NetworkVariable<float>`
- Lock-on rotation toward `lockTarget`
- Delegation to `ComboSystem` and `BlockSystem`

The main entry point is:

```csharp
public void Tick(PlayerControllerInput frameInput, PlayerAnimState animState)
```

`Tick()` applies gravity, computes state, handles movement, drains stun, starts/stops block, advances attack combos, and updates the replicated `PlayerState`.

### `ComboSystem.cs`

Implements the attack state machine:

- `NotAttacking`
- `Charge`
- `HeavyCharged`
- `Heavy`
- `Light1`
- `Light2`

A short press releases into `Light1`; holding past `heavyChargeTime` enters `HeavyCharged`, then releasing performs `Heavy`. During `Light1`, animation events can enable `canCombo`, allowing a second click to transition to `Light2`.

Each attack assigns a `WeaponData` profile to the active `Weapon`.

### `BlockSystem.cs`

Tracks block timing:

- `StartBlock()` begins block mode and records the start time.
- `StopBlock()` exits block mode.
- `IsParry` is true during the early block window.
- `IsBlocking` becomes true after `canBlockTime`.

This means the first part of a block acts as a parry window, while sustained blocking becomes a normal guard.

### `Weapon.cs`

Stores the current `WeaponData` and checks for hits with `Physics.OverlapCapsuleNonAlloc()`. The weapon uses its `CapsuleCollider` dimensions in world space and ignores its owning `Hittable`.

`isProcessed` prevents a single active swing from applying multiple hits until `ResetHit()` is called.

### `HitController.cs`

Resolves weapon contacts:

- Parry: attacker is stunned and pushed back.
- Block: defender takes no damage for non-heavy attacks, attacker gets small block stun.
- Hit: defender takes damage, stun, and force.

It also asks `HitAnimator` to spawn the correct client-side VFX.

## Animation and View Scripts

### `PlayerAnimator.cs`

Bridges replicated gameplay state into Unity Animator parameters:

- Locomotion speed parameters are calculated from target movement.
- Attack states drive charge, light, and heavy animation flags.
- Stun changes trigger hit reactions.
- Death starts ragdoll mode.
- Returning from death resets ragdoll mode.

`GetAnimState()` reads `PlayerAnimationEvents` flags to tell gameplay code whether the current animation can hit, combo, or has exited attack animation.

### `PlayerAnimationEvents.cs`

Called by animation events. It controls:

- `canCombo`
- `isAttacking`
- swing sound/event playback through `onPlaySwing`

Gameplay depends on these flags, so attack animations need correctly placed events.

### `RagdollSystem.cs`

Caches ragdoll part transforms, colliders, and rigidbodies. It can:

- Build its part list from `ragdollRoot` through the context menu.
- Disable ragdoll physics on start.
- Enable ragdoll physics on death.
- Restore saved local poses and rebind the animator on respawn.

### UI and VFX Helpers

- `HealthBar.cs` updates a UI fill from replicated health.
- `StunBar.cs` shows stun duration while stun is active.
- `HitAnimator.cs` spawns hit/block/parry VFX on clients.
- `FaceCamera.cs` keeps world-space UI facing the main camera.
- `DestroyAfter.cs` destroys temporary VFX objects after a delay.

## Input Scripts

### `PlayerControllerInput`

Defined in `PlayerController.cs`. It contains:

- `Vector2 moveInput`
- `bool isAttack`
- `bool isBlock`

This is the common input shape consumed by both player and AI control.

### `BaseInputHandler.cs`

Base class for anything that can provide `PlayerControllerInput`.

### `RpcInputs.cs`

Receives Unity Input System callbacks and forwards them to the server with `ServerRpc` methods. The server stores the latest input values and exposes them through `ReadInputs()`.

### `RemoteInputHandler.cs`

Adapter that reads input from `RpcInputs`. `NetworkPlayer` assigns this as the active input handler for network-controlled knights.

## AI Scripts

`EnemyAi` inherits from `BaseInputHandler`, so AI can drive a knight through the same input path as a human player.

The AI periodically evaluates weighted states:

- `EnemyAiState.cs` defines a base weight using `defaultWeight`, `distanceCurve`, `maxDistance`, and `weightMultiplier`.
- `EnemyAiMoveState.cs` returns configured movement input.
- `EnemyAiAttackState.cs` emits one attack input when the state starts.
- `EnemyAiBlockState.cs` holds block input while active.

`EnemyAi.PickState()` samples the configured states by weight, allowing designers to tune behavior in the Inspector.

## Networking Scripts

### `NetworkPlayer.cs`

Handles player setup after Netcode spawn:

- Enables local input for the owner.
- Routes server/non-owner input through `RemoteInputHandler`.
- Switches AI players to `EnemyAi`.
- Calls `GameController.RebuildPlayerList()` after spawn.

### `NetworkScreen.cs`

Main network menu/controller for online play:

- Initializes Unity Gaming Services.
- Signs in anonymously.
- Starts Relay-backed host sessions.
- Creates and heartbeats public lobbies.
- Queries lobbies and joins them.
- Supports direct IP client connection.
- Starts an AI match by hosting and spawning an AI player.

Relay is configured with WebSockets and currently selects the `asia-southeast1` region.

### LAN Discovery

`HostBeacon.cs` broadcasts JSON packets containing game id, port, and device name over UDP.

`BeaconListener.cs` listens for these packets, tracks recently seen servers, rebuilds the server list UI, and connects through `UnityTransport`.

`NetworkListRow.cs` is the small UI row component used for both lobby and LAN server lists.

## Scene Wiring Notes

The custom scripts rely heavily on Inspector references. Important links include:

- `GameController`
  - `hitController`
  - two `spawnPoints`
  - `onStartGame`
- `KnightInfo`
  - `inputHandler`
  - `controller`
  - `animator`
  - `aiHandler`
  - `headTransform`
  - `cams`
- `PlayerController`
  - `CharacterController`
  - `ComboSystem`
  - `BlockSystem`
- `ComboSystem`
  - `Weapon`
  - `WeaponData` entries for `light1`, `light2`, and `heavy`
- `Weapon`
  - `CapsuleCollider`
  - owning `Hittable` in `ignore`
- `PlayerAnimator`
  - `Animator`
  - `PlayerAnimationEvents`
  - `RagdollSystem`
  - hit reaction clip
  - heavy-charge glint object
- `NetworkScreen`
  - lobby UI fields
  - row prefab
  - status text
  - lobby panel

## Extending the Scripts

To add a new attack, extend `AttackState`, add a `WeaponData` field in `ComboSystem`, update the state transitions, and map the new state in `PlayerAnimator.Tick()`.

To tune block/parry feel, adjust `BlockSystem.canBlockTime`, `HitController.blockStun`, and `HitController.parryStun`.

To tune AI, add or configure `EnemyAiState` components and adjust their distance curves and weights.

To add new hit feedback, update `HitType`, `HitAnimator.GetFab()`, and the relevant branch in `HitController.HandleWeaponHit()`.

## Known Script Considerations

- `StateWatcher<T>.IsChanged` currently stores the equality result between current and previous values. If it is intended to mean "changed", the comparison should likely be inverted.
- `NetworkScreen` has a hard-coded Relay region of `asia-southeast1`.
- Several gameplay scripts assume exactly two active knights.
- Many scripts use public fields for Inspector wiring, so missing references will usually fail at runtime rather than compile time.
