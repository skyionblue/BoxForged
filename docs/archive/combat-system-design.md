# Unity Combat System Design Prompt

You are a senior Unity gameplay engineer building a modern third-person
action combat system.

The goal is to create combat that feels fluid, responsive, and highly
skill-based. The combat should emphasize player mastery through timing,
positioning, and decision-making rather than button mashing or stat
checks.

## Core Gameplay Pillars

The combat should feel:

-   Fast but readable
-   Aggressive but deliberate
-   Responsive with minimal input latency
-   Easy to learn and difficult to master
-   Animation-driven without feeling sluggish
-   Skill-based rather than equipment-based

Every encounter should resemble a duel where players are constantly
making decisions.

------------------------------------------------------------------------

## Combat Flow

The intended combat rhythm is:

Approach

↓

Probe with light attacks

↓

Enemy responds

↓

Dodge or Parry

↓

Counter attack

↓

Short combo

↓

Reposition

↓

Repeat

Combat should never devolve into standing still while trading damage.

------------------------------------------------------------------------

## Player Controller

Implement a modern third-person controller featuring:

-   Responsive acceleration
-   Smooth deceleration
-   Camera-relative movement
-   Optional lock-on targeting
-   Sprinting
-   Walking
-   Strafing
-   Dodge rolling
-   Dodge stepping
-   Air control (if jumping exists)

Movement should remain responsive even during combat.

------------------------------------------------------------------------

## Target Lock

Implement a lock-on system that:

-   Prioritizes enemies closest to the camera center
-   Allows cycling between nearby enemies
-   Keeps both player and enemy visible
-   Automatically unlocks when enemies die
-   Gracefully handles obstacles
-   Supports free movement without locking

------------------------------------------------------------------------

## Attack System

Implement a modular combo system.

The combo system should support:

-   Light attacks
-   Heavy attacks
-   Running attacks
-   Dodge attacks
-   Jump attacks
-   Charged attacks
-   Finisher attacks

Avoid hardcoding combos.

Instead, use a data-driven combo graph where attacks define possible
follow-up attacks.

------------------------------------------------------------------------

## Animation

Use animation state machines with transitions driven by gameplay state.

Support:

-   Attack buffering
-   Animation cancel windows
-   Combo timing windows
-   Recovery animations
-   Interrupt reactions

Animations should blend smoothly while remaining responsive.

------------------------------------------------------------------------

## Hit Detection

Use weapon hitboxes rather than raycasts whenever possible.

Support:

-   Multiple hitboxes
-   Damage windows
-   Friendly fire filtering
-   Hit stop
-   Hit reactions
-   Impact effects
-   Critical hits
-   Weak points

------------------------------------------------------------------------

## Combat Feel

Every successful hit should include:

-   Hit stop
-   Camera impulse
-   Controller vibration support
-   Sound variation
-   Sparks
-   Blood or impact particles
-   Enemy flinch
-   Directional knockback

Heavy attacks should feel dramatically more impactful than light
attacks.

------------------------------------------------------------------------

## Dodge System

Implement:

-   Standard dodge
-   Perfect dodge timing
-   Dodge invulnerability frames
-   Directional dodges
-   Dodge attacks

A perfectly timed dodge should briefly slow time and reward the player
with a counterattack opportunity.

------------------------------------------------------------------------

## Parry System

Support:

-   Standard blocking
-   Perfect parry
-   Guard break
-   Counter attacks

Perfect timing should stagger enemies.

Poor timing should still allow blocking while consuming stamina.

------------------------------------------------------------------------

## Stamina

Actions consume stamina.

Examples:

-   Heavy attacks
-   Sprinting
-   Dodging
-   Blocking
-   Charged attacks

Light attacks consume little stamina.

Good players should maintain offensive momentum through proper stamina
management.

------------------------------------------------------------------------

## Enemy AI

Enemies should not wait in line to attack.

Each enemy should:

-   Maintain tactical spacing
-   Flank when appropriate
-   Retreat after combos
-   Punish healing
-   Punish missed attacks
-   Mix attack timing
-   Occasionally feint attacks
-   React to player aggression

Enemies should coordinate naturally without feeling scripted.

------------------------------------------------------------------------

## Boss AI

Bosses should behave like skilled fighters.

Support:

-   Multiple phases
-   Adaptive attack patterns
-   Combo variations
-   Fake openings
-   Punish windows
-   Arena awareness
-   Cinematic attacks

Bosses should reward observation rather than memorization.

------------------------------------------------------------------------

## Camera

The combat camera should:

-   Smoothly follow the player
-   Pull back during groups
-   Tighten during duels
-   Shake on heavy impacts
-   Frame both combatants while locked on
-   Avoid clipping through walls

------------------------------------------------------------------------

## Architecture

Use a modular architecture.

Suggested systems:

-   PlayerController
-   CombatController
-   WeaponController
-   HitboxManager
-   AnimationController
-   LockOnController
-   TargetManager
-   EnemyAIController
-   StateMachine
-   StaminaSystem
-   HealthSystem
-   StatusEffectSystem
-   CameraController
-   AudioManager
-   VFXManager

Avoid monolithic scripts.

Favor composition over inheritance.

Use ScriptableObjects for:

-   Weapons
-   Movesets
-   Combo trees
-   Enemy archetypes
-   Damage types
-   Status effects
-   Combat tuning values

------------------------------------------------------------------------

## Extensibility

The combat system should be designed so that adding a new weapon
requires creating new data assets rather than modifying existing code.

Adding new enemies should not require changes to player combat logic.

The system should be reusable across multiple projects.

------------------------------------------------------------------------

## Desired Feel

The combat should capture the responsiveness and flow of modern
character action games. Players should be encouraged to stay engaged at
close range, chaining attacks, dodges, and counters into a continuous
rhythm. Every hit should feel powerful, every defensive action should
create an opportunity, and success should come from mastering timing,
spacing, and reading enemy behavior rather than relying on high
character stats or repetitive attack patterns.
