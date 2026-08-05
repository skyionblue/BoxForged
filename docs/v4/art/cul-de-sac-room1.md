# BoxForged – Room 1: The Arrival

## Objective

Build **Room 1 – "The Arrival"** from **Phase 2 – The Cul-de-Sac** using the Unity Asset Store package:

> https://assetstore.unity.com/packages/3d/environments/low-poly-mega-pack-polyworks-58821

This room serves as the player's first introduction to the Cul-de-Sac biome and should establish the visual language for the entire zone.

The environment should immediately communicate that **Kid is not seeing reality**. The player is walking through a normal suburban neighborhood that Kid imagines as a classic Wild West town.

This room is intentionally low pressure. It exists to establish atmosphere, teach navigation, and introduce the first enemy type.

---

# Design Goals

When the player enters this room they should immediately feel:

- Wonder
- Curiosity
- Nostalgia
- Adventure

The player should think:

> "This was just a neighborhood... but through Kid's imagination it's become a Wild West town."

Combat is secondary.

The environment is the star.

---

# Room Size

Approximate playable area:

- Width: **40 meters**
- Length: **30 meters**

Layout should be mostly open.

Avoid long narrow corridors.

Leave generous space for movement and dodging.

---

# Player Flow

```
Spawn
   │
   ▼

 ────────────────────────────────

      Covered Wagon

 House             House

        Main Street

 Wagon             Wagon

 House             House

        Open Combat

             ▼

         Exit Gate
```

Player enters from one side.

Exit is visible from the opposite end.

The player should naturally move forward through the environment.

---

# Environment Construction

Use the **Simple Town Cartoon Assets** pack as the foundation.

The environment should still look like a suburban neighborhood, but transformed by imagination.

Do **NOT** build an authentic western town.

Instead, imagine a child has looked at suburban houses and decided they are western buildings.

---

# Buildings

Place **4–6 houses** from the asset pack.

Convert them visually into western buildings using decorations.

Suggested buildings:

- Saloon
- Sheriff's Office
- General Store
- Barber Shop
- Hotel
- Trading Post

Do not alter the building geometry dramatically.

The transformation should feel believable from a child's imagination.

---

# Streets

Create one central street running through the room.

The street should feel like:

- dusty
- warm
- sun baked
- lightly traveled

Scatter:

- rocks
- dead grass
- tumbleweeds
- dirt patches

Avoid perfectly clean roads.

---

# Cover Objects

Convert parked suburban vehicles into imagined covered wagons.

Place:

- 3–5 covered wagons

Purpose:

- soft cover
- visual interest
- combat navigation

Do not create choke points.

Leave multiple movement paths.

---

# Props

Populate the environment with western-inspired props.

Suggested props:

- hitching posts
- barrels
- rope coils
- lantern posts
- wooden signs
- water trough
- mailbox telegraph office
- broken wagon wheel
- dead bushes
- small rocks
- fence pieces

Most props should be placed around the outside edges of the room.

Keep the center mostly open.

---

# Visual Style

## Lighting

Permanent golden hour.

Warm sunlight.

Long shadows.

No blue lighting.

No cold atmosphere.

---

## Color Palette

Primary colors:

- warm tan
- dusty orange
- faded teal
- worn brown
- muted red
- weathered wood

Avoid:

- bright modern colors
- neon colors
- saturated suburban paint

---

# Combat Space

Approximately **60%** of the room should remain open.

Avoid excessive clutter.

The player should always have room to dodge.

---

# Enemy Encounter

Spawn:

- 2–3 Tumbleweed Rollers

No ranged enemies.

No ambushes.

No elite enemies.

Enemies should slowly wander until the player approaches.

The encounter is intended to teach movement and dodge timing.

---

# Player Introduction

When the player enters:

Delay combat for approximately **3 seconds**.

Allow the player to observe:

- saloon fronts
- covered wagons
- warm lighting
- drifting tumbleweeds
- western atmosphere

Enemies should not attack until the player advances into the street.

---

# Camera Composition

Create memorable sight lines.

From the spawn point the player should immediately notice:

- the western main street
- saloon-style buildings
- covered wagons
- mountains in the distance
- the road leading deeper into the Cul-de-Sac

The first impression should be visually striking.

---

# Performance Requirements

- Use prefabs whenever possible.
- Mark static geometry appropriately.
- Enable GPU instancing on repeated props.
- Bake lighting where appropriate.
- Minimize draw calls.
- Reuse materials.
- Leave space for future procedural decoration.

---

# Things to Avoid

Do **NOT** create:

- realistic western towns
- abandoned ghost towns
- dark horror environments
- cluttered streets
- maze-like layouts
- excessive prop density
- narrow combat spaces

This room should feel welcoming and adventurous.

---

# Success Criteria

The room is complete when:

- The player immediately understands the Wild West imagination theme.
- The room naturally guides the player toward the exit.
- Combat space is open and readable.
- The environment tells the story before combat begins.
- The first enemy encounter teaches movement without overwhelming the player.
- The visual presentation matches the whimsical cardboard-and-marker aesthetic established in the game design document for Room 1, "The Arrival." GDD-v2-cul-de-sac.md