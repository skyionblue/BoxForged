# BoxForged V4 — Canonical Ability Names

**Status:** Locked — ready for AbilitySO asset creation in Sprint 2
**Written by:** storyteller agent
**Last updated:** 2026-08-04

These are the canonical in-game names for all 24 weapon abilities (2 per weapon).
The **Flavor Name** is the string used in `AbilitySO.displayName`.
The **Tooltip** is the string used in the ability unlock screen.

---

| Weapon | Tier | Functional Name | Flavor Name | Tooltip |
|---|---|---|---|---|
| Bo Staff | Epic | Spin Strike | **The Morning Sweep** | One spin. Every direction. They all felt it. |
| Bo Staff | Legendary | Stagger | **Third Strike Rule** | Hit them twice. The third one sticks. |
| Shurikens | Epic | Ricochet | **Fold and Return** | It bounces once. That's not a miss. That's a plan. |
| Shurikens | Legendary | Triple Throw | **Three at Once** | I practiced this in the backyard. Now it counts. |
| Foam Sword | Epic | Unbreakable | **It Never Breaks** | This blade has never lost. It won't start now. |
| Foam Sword | Legendary | Resilience | **Gets Stronger** | Hit me. Go ahead. I dare you. |
| Quickdraw Blade | Epic | Flash Draw | **The First Strike** | First hit of every fight. Guaranteed critical. |
| Quickdraw Blade | Legendary | Ghost Step | **The Long Dodge** | When this blade is out, I go farther. Faster. |
| Katana | Epic | One Cut | **Clean Cut** | Critical hit. Double damage. That's just math. |
| Katana | Legendary | Iaijutsu | **The Draw Attack** | Dodge becomes strike. One motion. No warning. |
| Lightsaber | Epic | Parry Flash | **Lights Out** | Successful parry. They can't see for a second. I can. |
| Lightsaber | Legendary | Deflect | **Send It Back** | One projectile per room. I catch it. I send it back. |
| Water Whip | Epic | Pull | **Come Here** | Long reach. They don't get to stay back there. |
| Water Whip | Legendary | Soaked | **It Slows Them Down** | Wet and slow. The whip remembers. |
| Lasso | Epic | Grab | **Caught** | It lands. They stop. That's all they get. |
| Lasso | Legendary | Rodeo | **Spin and Throw** | Caught one. Threw them at another. My best idea yet. |
| Pressure Cannon | Epic | Charge | **Three Pumps** | Hold it. Hold it. Let go. Double damage. |
| Pressure Cannon | Legendary | Blast Wave | **Full Blast** | Charged shot. Everything nearby goes flying. Everything. |
| Magic Wand | Epic | Confusion | **Mixed Up** | Sometimes they start hitting each other. I don't know why. It's great. |
| Magic Wand | Legendary | Overload | **All Eight** | Every fifth cast fires in every direction. The wand decided. |
| Iron Standard (Shield) | Epic | Block | **Hold the Line** | Hold the button. Nothing gets through. Nothing. |
| Iron Standard (Shield) | Legendary | Counter | **Right Back** | Block it. Hit back. The Iron Standard invented this. |
| Dynamite Bundle | Epic | Wide Blast | **Bigger Bang** | Same squeeze. Bigger circle. Run faster. |
| Dynamite Bundle | Legendary | Chain Reaction | **It Spreads** | The first one goes. Then the cardboard goes. Then everything goes. |

---

## For AbilitySO Creation

Each `AbilitySO` asset needs:
- `abilityId` — use the functional name in snake_case (e.g. `spin_strike`, `fold_and_return`)
- `displayName` — the **Flavor Name** column above
- `flavorDescription` — the **Tooltip** column above
- `trigger`, `magnitude`, `cooldown` — from `docs/v4/design/weapon-creation-system.md` → Weapon Abilities table

Asset naming convention: `Ability_[WeaponName]_[Tier].asset`
Example: `Ability_BoStaff_Epic.asset`, `Ability_BoStaff_Legendary.asset`

Asset destination: `Assets/_Project/ScriptableObjects/Weapons/Abilities/`

---

*Names written by storyteller agent — 2026-08-04. Approved for Sprint 2.*
