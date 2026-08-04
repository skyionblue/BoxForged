# Cross-Project Integration: Unboxed Heroes × Hard Knocks Workshop

## Why This Document Exists

Both projects exist for the same reason: kids are spending too much time on screens and not enough time with other people or learning real skills. Unboxed Heroes is the Trojan horse — the only medium that reliably gets a kid's attention. Hard Knocks Workshop is the destination. The game creates the hunger; the workshop feeds it.

---

## The Non-Negotiable Lore Anchor

> **The BoxHead elders were the ones that killed the Internet.**

This is the load-bearing idea that connects both projects at the story level. The elders did not lose the Internet — they *chose* to destroy it. A deliberate sacrifice to save something more important: the ability of the next generation to make things with their own hands.

The moment it happened has a confirmed canonical name: **The Great Hush**.

The "Great" adds historical weight — something passed down, whispered by adults who lived through it, with the gravity of a folktale. In the game world it is not official history. It is grief language. The kid hears "The Great Hush" and knows not to ask what it means.

---

## The Unified Narrative

The BoxHead elders watched a generation lose the ability to make, fix, or survive anything. Screens replaced skills. Dependency replaced competence. The Unimaginative rose to power by controlling the machines everyone relied on. Rather than let that dependence consume the next generation, the elders made a choice: they killed the Internet.

**The Elders were not ancient sages or sci-fi villains.** They were engineers — Network Engineers, DevOps Engineers, Support Engineers. The people who actually built and maintained the systems that held the Internet together. They had the access, the technical understanding, and eventually the will. The Great Hush was a coordinated act by people who knew exactly what they were doing and did it anyway.

**The timeline is present-day: 2026.** Not a far-future dystopia. The world of Unboxed Heroes is recognizably now — suburban America, minivans, cracked cul-de-sacs — with the Internet simply gone.

The world that followed is harder — but it is honest. Kids who grew up after The Great Hush had to learn real things. How to build. How to navigate. How to fix a broken wheel or start a fire without a search engine. These are the heroes.

The Unimaginative are not evil. They are the wound the Elders accidentally made worse — people who outsourced everything to the network and had it taken away with nothing to replace it. They have seized the World Tree (the last raw material for handmade things) because they want to trade the ability to build for the comfort of consuming.

**The player's job:** Stop them. Not by being smarter online, but by being more capable offline.

**The question the game lives inside:** The Elders were right about the problem. Whether they were right about the cure is something the player has to decide for themselves.

---

## The Two Projects

| | Unboxed Heroes | Hard Knocks Workshop |
|---|---|---|
| **Type** | Mobile action game (iOS/Android) | Real-world youth skills program |
| **Location** | Global (digital) | Lehi, Utah (physical) |
| **Target age** | 10–15 | 10–15 |
| **Status** | Active development (Sprint 16) | Pre-launch viability assessment |
| **Brand relationship** | **Separate** — intentional connection, not co-branded |

The connection between them is thematic and intentional, but the brands stand alone. A player who loves Unboxed Heroes and later encounters Hard Knocks Workshop should feel immediate recognition — not because logos match, but because the story prepared them for it.

---

## Integration Roadmap

### Phase 1 — Story Does the Work *(Now, zero development cost)*

Make the narrative foundation the connection. No mechanics, no marketing — just ensure the lore is right.

- Update GDD-V2 to include The Great Hush origin story as canonical backstory — name confirmed, timeline confirmed (present-day 2026), Elders confirmed as engineers/DevOps/infrastructure people
- Reframe the Unimaginative as people who want to *restore* digital dependence, not just conquer territory
- Map in-game hero skills loosely to real HKW skills: navigation, building, fire-starting, axe-work
- Zone design: reference real-world craftsmanship in the Cul-de-Sac ENV (workshops, hand tools, gardens, things built by hand)

**Outcome:** Anyone who plays the game and later hears about Hard Knocks Workshop will feel an immediate, subconscious recognition. The story primes them without a word of marketing.

---

### Phase 2 — The Bridge *(At or near game launch)*

Add the explicit connection. Players discover HKW naturally.

- In the game's pause menu / credits: a subtle "Real Heroes Train Here" section with a link to hardknocks-workshop.com
- After the final boss / end screen: an unlockable lore page — the elders' manifesto — that ends with "The skills in this game are real. Want to learn them?" with a QR code
- HKW social content references the game story: "The elders in Unboxed Heroes did what we're trying to do in real life"
- Game social content references HKW philosophy: "The Unimaginative are everywhere. What are you teaching your kids?"

**Outcome:** Warm leads. Players who connect with the story want to know more. The QR code converts curiosity into HKW enrollment interest.

---

### Phase 3 — The Loop *(After HKW is live)*

Real-world participation and in-game rewards create a reinforcing cycle.

- **Real-world skill unlocks:** HKW instructors issue stamped cards or QR codes when students complete a skill. Scanning the code unlocks the corresponding in-game reward.
  - Complete axe-work → unlock the Splitting Maul ability
  - Complete wilderness navigation → unlock the Compass Dash (dodge upgrade)
  - Complete fire-starting → unlock a fire-based weapon special
  - Complete woodworking → unlock a crafted weapon skin
- **Co-op near-field mechanic:** One game mode requires two players to be physically near each other (Bluetooth / local Wi-Fi). Forces in-person play. The screen becomes a reason to gather, not a reason to stay home.
- **Seasonal events tied to HKW calendar:** Summer camp session = summer in-game event. HKW students earn exclusive limited-time cosmetics. Players who don't attend see the items and want to know how to get them.

**Outcome:** Enrollment driver. The game creates FOMO for HKW. HKW completion creates pride in the game. The two projects become a single reinforcing loop.

---

### Phase 4 — The Platform *(Long term)*

The game becomes the digital arm of HKW's brand.

- A "Skill Log" section inside Unboxed Heroes: kids log real-world skills, parents or instructors co-sign completions, logged skills earn in-game collectibles — a digital badge book for real achievements
- An HKW-exclusive in-game character skin: workshop apron, safety goggles, tool belt layered over the ninja/cowgirl designs — the real-world HKW aesthetic applied to the Unboxed Heroes characters
- Partner with schools and youth programs in Alpine School District to embed the game + workshop combo into after-school programming

---

## Canonical Lore — Locked

| Detail | Canonical answer |
|---|---|
| **Event name** | The Great Hush |
| **When it happened** | Present day — 2026 |
| **Who the Elders are** | Engineers: Network, DevOps, Support — people who built and ran the Internet |
| **Why they did it** | People forgot how to build, fix, and maintain things — and relationships. The Elders pulled the plug. Kids had to live. The Elders considered that a feature. |
| **The open question** | Were they right about the cure? That's what the game lives inside. |

These details are confirmed and should be used in all storytelling, GDD updates, zone design, and social content.

---

## Next Actions

- Update GDD-V2 with The Great Hush backstory — route to `game-design-doc-writer`
- Ensure zone and enemy design carries the "building/making/surviving without tech" theme — route design decisions through this doc before implementation
