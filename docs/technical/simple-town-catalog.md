# Simple Town Asset Catalog

Asset pack: `Assets/SimpleTown/`
Materials upgraded to URP Lit (39/39 — all converted 2026-07-23 during Room 1 build).
All prefabs located under `Assets/SimpleTown/Prefabs/`.

> **Polyworks note:** `Assets/Off Axis Studios/Polyworks/Meshes/Atlased/atlasCombiMat_AllMPBase.mat` was also on Standard shader and upgraded to URP Lit on the same date. This single atlas material drives all 4,000+ Atlased Polyworks prefabs.

---

## Scale Notes

| Item | Full Scale (1.0) | Recommended Game Scale (0.5) |
|---|---|---|
| House | ~18.5m wide x 14.5m tall x 10.6m deep | ~9.3m x 7.3m x 5.3m |
| Road segment | 5.0m wide x 10.0m long | 2.5m wide x 5.0m long |
| Roundabout | 7.8m diameter | 3.9m diameter |
| Player character | — | 2.0m tall |

**Recommended scale factor: 0.5** — houses feel like 2-story buildings relative to the player.

---

## Buildings (59 prefabs)

### Residential Houses (18)

| Prefab | Style Notes |
|---|---|
| `Building_House_01` | Blue, 2-story |
| `Building_House_02` | Cream/tan, 2-story |
| `Building_House_03` | Red/brick, 2-story |
| `Building_House_04` | Grey, 2-story |
| `Building_House_05` | Orange, 2-story |
| `Building_House_06` | Brown, 2-story |
| `Building_House_07` | Mixed color |
| `Building_House_08` | Mixed color |
| `Building_House_09` | Mixed color |
| `Building_House_010` | Mixed color |
| `Building_House_011` | Mixed color |
| `Building_House_012` | Mixed color |
| `Building_House_013` | Mixed color |
| `Building_House_014` | Mixed color |
| `Building_House_015` | Mixed color |
| `Building_House_Green` | Green paint |
| `Building_House_Orange` | Orange paint |
| `Building_House_Red` | Red paint |

### Apartments (6)

| Prefab | Style Notes |
|---|---|
| `Building_ApartmentLarge_Brown` | 3-story, brown |
| `Building_ApartmentLarge_Orange` | 3-story, orange |
| `Building_ApartmentLarge_Red` | 3-story, red |
| `Building_ApartmentSmall_Brown` | 2-story, brown |
| `Building_ApartmentSmall_Orange` | 2-story, orange |
| `Building_ApartmentSmall_Red` | 2-story, red |

### Commercial / Shops (17)

| Prefab | Style Notes |
|---|---|
| `Building_AutoRepair` | Auto repair garage |
| `Building_BaberShop` | Barber shop |
| `Building_Cinema` | Movie theater |
| `Building_CoffeeShop` | Coffee shop |
| `Building_Gym` | Gym |
| `Building_Mall` | Shopping mall (large) |
| `Building_PetrolStation` | Gas station |
| `Building_PoliceStation` | Police station |
| `Building_Shop_01` | Small shop variant 1 |
| `Building_Shop_02` | Small shop variant 2 |
| `Building_Shop_03` | Small shop variant 3 |
| `Building_Shop_04` | Small shop variant 4 |
| `Building_Shop_05` | Small shop variant 5 |
| `Building_Store_Drug` | Drug store |
| `Building_Store_Pawn` | Pawn shop |
| `Building_Store_Video` | Video store |
| `Building_StripClub` | Strip club |

### Corner Stores (3)

| Prefab | Style Notes |
|---|---|
| `Building_StoreCorner_Drug` | Corner drug store |
| `Building_StoreCorner_Pawn` | Corner pawn shop |
| `Building_StoreCorner_Video` | Corner video store |

### Offices (12)

| Prefab | Style Notes |
|---|---|
| `Building_OfficeLarge_Blue/Brown/Grey` | Large office towers |
| `Building_OfficeMedium_Blue/Brown/Grey` | Medium offices |
| `Building_OfficeSmall_Blue/Brown/Grey` | Small offices |
| `Building_OfficeStepped_Blue/Brown/Grey` | Stepped/tiered offices |

### Garages (3)

| Prefab | Style Notes |
|---|---|
| `Building_Garage_01` | Single garage variant 1 |
| `Building_Garage_02` | Single garage variant 2 |
| `Building_Garage_03` | Single garage variant 3 |

---

## Roads & Environment (44 prefabs)

### Road Pieces (15)

| Prefab | Use Case | Notes |
|---|---|---|
| `road_straight_mesh` | Main street segments | Has lane markings |
| `road_straight_clear_mesh` | Road without markings | Clean surface |
| `road_divider_mesh` | Road with center divider | Two-lane divided |
| `road_bend_left_mesh` | Road curves left | |
| `road_bend_right_mesh` | Road curves right | |
| `road_corner_mesh` | 90-degree corner | |
| `road_cornerLines_mesh` | 90-degree corner with lines | |
| `road_crossing_mesh` | Full intersection | 4-way |
| `road_crossing_center_mesh` | Intersection center tile | |
| `road_t_mesh` | T-intersection | 3-way |
| `road_square_mesh` | Square road tile | Flexible connector |
| `road_Roundabout` | Cul-de-sac circle / roundabout | **Use for cul-de-sac center** |
| `road_LaneTransition_Left` | Lane merge left | |
| `road_LaneTransition_Right` | Lane merge right | |
| `roadLane_straight_Centered_mesh` | Centered single lane | |

### Paths & Sidewalks (2)

| Prefab | Use Case |
|---|---|
| `path_driveway` | Driveway connecting house to road |
| `fence_short_spike` | Spiked fence (also in Props) |

### Bridges (3)

| Prefab | Use Case |
|---|---|
| `Env_Car_Bridge` | Vehicle bridge |
| `Env_Car_Bridge_02` | Vehicle bridge variant |
| `Env_Foot_Bridge` | Pedestrian bridge |

### Beach & Water (8)

| Prefab | Use Case |
|---|---|
| `Env_Beach_Corner` | Beach corner piece |
| `Env_Beach_Short` | Short beach segment |
| `Env_Beach_Straight` | Straight beach segment |
| `Env_Jetty` | Boat dock |
| `Env_Water_Tile` | Water surface tile |
| `Env_Seawall_Corner_01/02/03` | Seawall corners |
| `Env_Seawall_Straight` | Straight seawall |
| `Env_Seawall_Wall` | Seawall wall section |

### Canal (6)

| Prefab | Use Case |
|---|---|
| `Env_Canal_Corner_01/02` | Canal corners |
| `Env_Canal_End` | Canal dead end |
| `Env_Canal_Pipe_01/02` | Canal pipe sections |
| `Env_Canal_Straight` | Straight canal |

### Elevated Road (3)

| Prefab | Use Case |
|---|---|
| `Env_Road_Corner` | Elevated road corner |
| `Env_Road_Ramp` | Road ramp (entrance) |
| `Env_Road_Ramp_Pillar` | Ramp support pillar |
| `Env_Road_Ramp_Straight` | Elevated straight road |

### Misc Environment (3)

| Prefab | Use Case |
|---|---|
| `Env_Planter_Prop` | Planter box |
| `Env_Rocks_01/02/03` | Rock formations |

---

## Props (43 prefabs)

### Trees & Vegetation (8)

| Prefab | Size |
|---|---|
| `tree_large_mesh` | Large tree |
| `tree_medium_mesh` | Medium tree |
| `tree_small_mesh` | Small tree |
| `Prop_Tree_01` | Tree variant 1 |
| `Prop_Tree_02` | Tree variant 2 |
| `bush_large_mesh` | Large bush |
| `bush_small_mesh` | Small bush |
| `hedge_mesh` | Hedge row |

### Flowers & Grass (2)

| Prefab | Use |
|---|---|
| `flower_mesh` | Flower bunch |
| `grass_square_mesh` | Grass patch tile |

### Fences (3)

| Prefab | Use |
|---|---|
| `fence_long_mesh` | Long fence section |
| `fence_short_mesh` | Short fence section |
| `fence_short_spike_prop` | Short spiked fence |

### Street Furniture (7)

| Prefab | Use |
|---|---|
| `lamp_mesh` | Street lamp |
| `hydrant_mesh` | Fire hydrant |
| `bin_mesh` | Trash bin |
| `dumpster_mesh` | Dumpster |
| `trash_mesh` | Trash bag |
| `traffic_light_mesh` | Traffic light |
| `billboard_mesh` | Billboard sign |

### Road Signs (3)

| Prefab | Use |
|---|---|
| `Prop_Roadsign_01` | Road sign variant 1 |
| `Prop_Roadsign_02` | Road sign variant 2 |
| `Prop_Roadsign_03` | Road sign variant 3 |

### Paths (2)

| Prefab | Use |
|---|---|
| `path_cross_mesh` | Path crossroads |
| `path_straight_mesh` | Straight path segment |

### Building Accessories (4)

| Prefab | Use |
|---|---|
| `Aerial_mesh` | TV antenna (rooftop) |
| `dish_mesh` | Satellite dish (rooftop) |
| `flag_mesh` | Flag |
| `pipe_mesh` | Pipe |

### Graveyard (4)

| Prefab | Use |
|---|---|
| `grave_large_mesh` | Large gravestone |
| `grave_medium_mesh` | Medium gravestone |
| `grave_small_mesh` | Small gravestone |
| `memorial_mesh` | Memorial monument |

### Beach / Misc (8)

| Prefab | Use |
|---|---|
| `Prop_Beachseat_01/02/03` | Beach chairs |
| `Prop_Umbrella_01/02/03` | Beach umbrellas |
| `Props_Buoy_01/02` | Water buoys |
| `Prop_TirePile` | Tire pile (junkyard) |

---

## Vehicles (43 prefabs)

"Seperate" variants have detachable parts (doors, wheels) for destruction/physics.

### Cars (6 + 6 seperate)

| Prefab | Color |
|---|---|
| `car_blue` / `car_seperate_blue` | Blue |
| `car_green` / `car_seperate_green` | Green |
| `car_red` / `car_seperate_red` | Red |

### Utes / Pickups (12 + 6 seperate)

| Prefab | Variant |
|---|---|
| `ute_mesh_blue/red/yellow` | Loaded bed |
| `ute_empty_blue/red/yellow` | Empty bed |
| `ute_seperate_blue/red/yellow` | Detachable (loaded) |
| `ute_empty_seperate_blue/red/yellow` | Detachable (empty) |

### Vans (3 + 3 seperate)

| Prefab | Color |
|---|---|
| `van_mesh_blue/green/red` | Standard |
| `van_seperate_blue/green/red` | Detachable parts |

### Buses (3 + 3 seperate)

| Prefab | Color |
|---|---|
| `bus_blue/brown/grey` | Standard |
| `bus_seperate_blue/brown/grey` | Detachable |

### Emergency / Service (5 + 5 seperate)

| Prefab | Type |
|---|---|
| `ambo_mesh` / `ambo_seperate` | Ambulance |
| `cop_mesh` / `cop_seperate_mesh` | Police car |
| `fire_truck_mesh` / `fire_truck_seperate_mesh` | Fire truck |
| `rubbishTruck_mesh` / `rubbish_truck_seperate_mesh` | Garbage truck |
| `taxi_mesh` / `taxi_seperate_mesh` | Taxi |

### Boats (3)

| Prefab | Type |
|---|---|
| `Vehicle_Boat_01` | Boat variant 1 |
| `Vehicle_Boat_02` | Boat variant 2 |
| `Vehicle_Boat_03` | Boat variant 3 |

---

## Cul-de-Sac Level Design Template

### Layout Concept

```
        [House] [House] [House]
       /                       \
      /     (roundabout)        \
     /                           \
[House]          ROAD            [House]
     \                           /
      \                         /
       ------- ROAD ENTRY ------
              (player spawns here)
```

### Recommended Pieces

| Role | Prefab(s) | Qty | Scale |
|---|---|---|---|
| Street | `road_straight_mesh` | 5-6 | 0.5 |
| Cul-de-sac circle | `road_Roundabout` | 1 | 0.5 |
| Houses (circle) | `Building_House_01` through `07` | 5-7 | 0.5 |
| Driveways | `path_driveway` | per house | 0.5 |
| Street trees | `tree_medium_mesh` | 6-8 | 0.5 |
| Street lamps | `lamp_mesh` | 4-6 | 0.5 |
| Yard bushes | `bush_large_mesh` / `bush_small_mesh` | 8-12 | 0.5 |
| Fences | `fence_short_mesh` | between houses | 0.5 |
| Parked cars | `car_blue`, `car_red`, `ute_mesh_yellow` | 2-3 | 0.5 |
| Hydrant | `hydrant_mesh` | 1-2 | 0.5 |
| Trash | `bin_mesh`, `dumpster_mesh` | 2-3 | 0.5 |

### Spacing Guidelines (at 0.5 scale)

| Measurement | Value |
|---|---|
| Road width | 2.5m |
| House width | ~9m |
| House depth | ~5m |
| Gap between houses | 3-4m minimum |
| Circle radius (house placement) | 14-18m from center |
| Street length (entry to circle) | 20-25m |
| Player spawn | 3-5m before street start |

### Notes for Scripted Level Generation

- All objects should use `localScale = Vector3.one * 0.5f`
- Houses on the circle face **inward** (toward the roundabout center)
- Use `Mathf.Sin`/`Mathf.Cos` with angle spacing for circular arrangement
- Mark all ENV/props as `isStatic = true`
- Road segments tile along their local Z axis (5m per segment at 0.5 scale)
- Ground plane should extend beyond the houses (80m x 80m covers the full area)
- URP materials already applied — no shader conversion needed at runtime
