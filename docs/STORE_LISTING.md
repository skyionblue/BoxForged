# BoxForged — Store Listing & Compliance Prep

**Status:** Draft. Copy and compliance answers below are ready for owner review and to paste into App Store Connect / Google Play Console — nothing here has been submitted to either store. Screenshots, the final app icon, and the actual submission remain outstanding (see §7).

**Decisions locked 2026-09-08 (owner), scope for this listing:**
- **No ads, no IAP in this release.** Ship monetization-free, matching what's actually built (`docs/CREATIVE_STATE.md`'s ads+IAP plan is future scope — see `docs/ROADMAP.md`). Revisit this whole document's Data Safety / privacy answers when that changes.
- **General-audience, all-ages rating** — not opted into Google Play Families or Apple's Kids Category. BoxForged is an all-ages game with a child protagonist, not a children's-program-classified app; this is the common classification for games like it.
- **Privacy policy will be hosted at `https://boxforged.com/privacy/`.** All references below assume that URL. If the final URL differs, update every reference in this doc and in `docs/media/social-launch-playbook.md`'s checklist.

Source material for all copy below: `docs/CREATIVE_STATE.md` (CANON), `docs/STORY_BIBLE.md` (emotional core, protagonist, themes) — not `docs/media/media-kit.md`, which is written for sponsors/press about the *show*, not players about the *game*, and shouldn't be reused as store copy.

---

## 1. App identity

| Field | Value |
|---|---|
| App name | **BoxForged** |
| Bundle ID | `com.boxforged` (Android + iOS, both platforms — set 2026-09-08) |
| Company/publisher | SkyionBlue |
| Category (primary) | Games → Action (secondary: Adventure) |
| Platforms | iOS + Android, landscape only |
| Price | Free |
| Contains ads | No |
| In-app purchases | No |
| Privacy policy URL | `https://boxforged.com/privacy/` |
| Support URL / contact | `https://boxforged.com/support/` (or a contact-form page there) — see §6 for placeholder copy if that page doesn't exist yet |

---

## 2. Store copy

### Short description (Google Play, 80 chars max)
> A cardboard box turns her backyard into a battlefield. Imagination is real.

(74 characters)

### Subtitle (iOS, 30 chars max)
> Imagination is a weapon

(24 characters)

### Full description (both stores)

> **The internet went silent. She never stopped imagining.**
>
> Somewhere between the garage and the back fence, a girl with a cardboard box is the last line of defense for a world that forgot how to see.
>
> BoxForged is a fast, roguelite action game about a kid, her forge, and the neighborhood she's reclaiming one short run at a time. Pick up anything lying around — a jump rope, a garden trowel, a beat-up push mower — and her box shows you what it really is: a lasso, a blade, a weapon worth fighting for. Nothing is pretend. She just sees correctly.
>
> **Read, don't grind.** Combat is dodge-parry-jump — watch the tell, time the block, make the opening. Every fight is a conversation, not a stat check.
>
> **Every run is yours.** Runs are short (10-15 minutes), start fresh, and end with a boss who was never just an appliance — a mower reimagined as a blade-master, a wagon wheel with a grudge. Beat one, and the color comes back to the street.
>
> **Two Fighting Styles to start** — Ninja and Cowboy — each with its own rhythm, its own reads, its own way of turning a backyard into a dojo.
>
> No ads. No pressure to buy anything. Just a kid, a box, and a world worth seeing clearly.
>
> *BoxForged is being built live, in public, by two engineers with no prior game-dev experience — follow the build at theunboxedheroes.com.*

**Notes on this draft:**
- Deliberately doesn't promise co-op (designed in architecturally, not shippable in Phase 1 — `docs/CREATIVE_STATE.md` §Game Structure) or name specific weapons/enemies that might change before launch.
- The closing line links the show for anyone who finds it through that channel, without making the *store listing* read like a show pitch — cut it if you'd rather keep the two fully separate.
- "No ads. No pressure to buy anything." is a factual, true-today statement and a deliberate selling point — reconsider this exact line (not the underlying decision) the day ads/IAP ship, since it would then be false advertising.

### Keywords (iOS, 100 chars max, comma-separated, no spaces after commas)
> action,roguelite,kids,adventure,ninja,cowboy,combat,dodge,parry,boss,imagination,family

### Suggested tags (Google Play)
Action, Adventure, Family, Casual-friendly session length

---

## 3. Age / content rating

**Recommendation: Everyone (ESRB) / PEGI 7 / Apple 9+**, driven by these content facts (cite the source before answering any questionnaire — don't guess beyond what's documented):

| Questionnaire category | Answer | Basis |
|---|---|---|
| Violence | **Fantasy/cartoon violence only.** Combat is against reimagined household objects and Skeptic Grunts (kids depicted holding the same everyday objects, "dangerous with them, but they don't know what they're holding" — `STORY_BIBLE.md`), not realistic weapons or graphic violence. | `docs/STORY_BIBLE.md` §Antagonists; `docs/CREATIVE_STATE.md` §World 2 Enemies |
| Blood/gore | **None.** No blood, no gore anywhere in current content. | Confirmed absent from all design/story docs |
| Realistic weapons | **No.** "Six-Shooter," "Pressure Cannon," etc. are cardboard/imagination-forged toy props transformed from household objects (a watering can, a garden trowel) — not depicted as real firearms. | `docs/BACKLOG.md` B51 (Six-Shooter's origin object); `docs/CREATIVE_STATE.md` "The box is a lens... reveals imagination" |
| Defeat/death framing | **Restorative, not violent.** "Boss defeats are moments of restoration, not conquest — the defeat moment and the Imagination Restore are one event" (cherry blossoms, color returning, no death animations depicting harm to a child character). | `docs/STORY_BIBLE.md` §Combat and defeat framing |
| Language | None. No profanity anywhere in design docs. | — |
| Controlled substances, gambling, sexual content | None present. | — |
| User-generated content / chat | None — no multiplayer, no chat, no UGC sharing in the shipped game. (The *show* has Discord/community voting, but that's outside the app itself.) | `docs/CREATIVE_STATE.md` §Game Structure — "Phase 1 is single-player" |
| Data collection / interaction with other users | None currently — see §5 Data Safety. | Release audit, 2026-09-08 |

**Do not self-certify a higher or lower rating than this table supports without re-checking against actual shipped content** — if a future boss or enemy introduces anything not covered here (blood, realistic weapon likeness, romantic content), revisit this table before resubmitting.

---

## 4. Privacy policy (draft — publish at `https://boxforged.com/privacy/`)

```
# BoxForged Privacy Policy

Last updated: [DATE OF PUBLICATION]

BoxForged ("the App," "we," "us") is developed by SkyionBlue. This policy
explains what information the App collects and how it's used. We wrote
it to be short because there isn't much to say: the App does not collect,
store, or share any personal information from its players.

## What we collect

Nothing. BoxForged has no account system, no login, no chat, no user-
generated content, no location access, no camera or microphone access,
and no advertising or analytics SDKs. The App does not connect to the
internet to send or receive any player data during normal play.

Your device's operating system (iOS or Android) and the App Store /
Google Play Store may independently collect standard technical
information (like crash reports or download statistics) as part of
their own platform services. That collection is governed by Apple's
and Google's own privacy policies, not this one — we don't receive or
see any of it in a form tied to you personally.

## Children's privacy

BoxForged is an all-ages game and is not directed specifically at
children under 13, but we know kids play it. Because the App collects
no data from anyone, of any age, there is nothing to disclose or
protect under COPPA or similar children's privacy laws — the same "we
collect nothing" answer applies regardless of who's playing.

## Changes to this policy

If BoxForged ever adds features that do collect data — for example,
ads, analytics, or an online leaderboard — we will update this policy
before that feature ships and note the change and date at the top of
this page.

## Contact

Questions about this policy or the App: [SUPPORT EMAIL — see §6]
```

**Fill in before publishing:** the actual publication date, and the support email address (see §6). If you'd rather have a dedicated privacy-specific inbox instead of the general contact address, say so and I'll update the placeholder.

---

## 5. Data Safety (Google Play) / App Privacy "nutrition label" (Apple) — draft answers

Both forms ask, in different formats, the same underlying question: what data does the app collect, and what's it used for. Current honest answer for both:

| Form | Answer |
|---|---|
| **Google Play Data Safety** | "No data collected." Every category (Location, Personal info, Financial info, Health & fitness, Messages, Photos/videos, Audio, Files/docs, Calendar, Contacts, App activity, Web browsing, App info/performance, Device/other IDs) — answer **No** to collection for all of them. Data is not shared with third parties (there are no third parties integrated). Data is not sold. |
| **Apple App Privacy** | Select **"Data Not Collected."** This is Apple's own pre-defined label for exactly this case — no custom disclosure needed as long as it stays true. |

**This entire section becomes wrong the moment any of the following ship, and must be redone at that point, not patched:** an ad SDK, an analytics SDK beyond Unity's dormant built-in module, a save-to-cloud/account system, or any online multiplayer/leaderboard feature. Treat "no data collected" as a snapshot of the current build, not a permanent claim.

---

## 6. Support page (draft — publish at `https://boxforged.com/support/` or wherever the support URL points)

```
# BoxForged Support

Having a problem with BoxForged, or found a bug? We want to hear about it.

Email us: [SUPPORT EMAIL]

Please include:
- What device and OS version you're playing on (e.g. iPhone 14, iOS 17)
- What you were doing when the problem happened
- A screenshot or screen recording, if you have one

We're a two-person team building this game live — we read every message.
```

**Open decision:** what email address should this point to? The known existing contact is `unboxedheroes.imagination@gmail.com` (from `docs/media/media-kit.md`), but given the `boxforged.com` domain and the `com.boxforged` bundle ID both signal a distinct game-brand identity from the podcast, you may want a dedicated address (e.g. `support@boxforged.com` or `hello@boxforged.com`) instead. Not set here — placeholder left as `[SUPPORT EMAIL]` in both this section and §4 until you decide.

---

## 7. Still outstanding — not resolved by this document

These need either an asset/art pass, a device, or a decision that isn't copy:

- **App icon gaps** (from the 2026-09-08 release audit, `docs/SPRINT.md` Sprint 2): Android adaptive (Kind 2) and round (Kind 1) icon slots are empty; iOS 180×180 slot references a different source file than every other iOS icon slot. Route to `art-director`/`asset-engineer`.
- **Screenshots** — both stores require real in-game screenshots (Play Mode or on-device captures), not mockups. Needs World 2 actually reachable first (fixed 2026-09-08, B131) and ideally the fresh on-device profiling pass done so captures reflect a game that's performing correctly, not an in-progress state.
- **App preview video** (15–30s, per the existing `docs/media/social-launch-playbook.md` checklist) — needs real gameplay footage.
- **Domain/hosting** — `boxforged.com` needs the `/privacy/` and `/support/` pages actually published before either store will accept the submission; App Store Connect and Play Console both validate that the URLs resolve.
- **Support email decision** (§6).
- **Final owner read-through of §2's copy and §3's rating table** before either goes into a store console — this document is a draft, not a submission.

Once the privacy/support pages are live and the icon gaps are fixed, cross these off in `docs/media/social-launch-playbook.md`'s existing "App Store and Google Play" checklist rather than duplicating tracking here.
