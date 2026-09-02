---
name: feedback-prefab-shadow-value-verification
description: A prefab's own serialized MonoBehaviour fields shadow the C# script's field defaults for anything ever explicitly set — editing the default alone ships nothing; always re-read the prefab YAML on disk after a "modify the default" instruction touches a field the prefab has ever overridden.
metadata:
  type: feedback
---

Any `[SerializeField]` field that a prefab asset has ever had an explicit value written for (which happens the first time anyone touches it in the Inspector, or the first time a tool like `manage_prefabs modify_contents` sets it) is serialized into the prefab's own YAML and permanently shadows the C# class's field default from then on. Changing `private float x = 14f;` to `private float x = 7.9f;` in the script changes nothing that ships if the prefab already carries `x: 14` in its own `MonoBehaviour:` block — Unity deserializes the prefab's explicit value over the class default every time.

**Why this matters here:** this exact mistake happened once already, within the same day's WIP, on `pfb_enemy_grasscutter.prefab`'s `_introCamDistance` field — a C# default edit from 14→9 shipped as 14 anyway because the prefab's own serialized value never got touched. See [[project_backyard_dojo_build]]'s 17th-pass entry.

**How to apply:**
- When a task says "change field X's value" and X lives on a MonoBehaviour attached to a prefab (not a fresh field never yet serialized), assume the prefab shadows it until proven otherwise.
- Use `manage_prefabs modify_contents` with `component_properties: {"<ComponentTypeName>": {"<fieldName>": <value>, ...}}` to write the value through Unity's own `SerializedObject` API rather than hand-editing YAML — this guarantees correct serialization and updates the asset's dirty/save state properly.
- **Vector3/Vector2/quaternion-typed properties are NOT directly settable this way** — `manage_prefabs modify_contents` rejects a bare Vector3 dict with `"Unsupported SerializedPropertyType: Vector3"`. Use the dotted per-axis form instead: `{"m_LocalPosition.x": 0, "m_LocalPosition.y": 0, "m_LocalPosition.z": 0}` — this works for `Transform.m_LocalPosition` and should generalize to other Vector-typed serialized fields.
- After saving, **re-read the prefab file's YAML directly from disk** (`grep`/`Read`, not a live Editor query) and confirm the literal new numeric values are present. A clean compile, or a live `GetComponent<T>()` read immediately after a Unity Editor operation, is not sufficient evidence — see [[feedback_prefab_contents_stale_read]] for a related stale-read gotcha on `PrefabUtility.LoadPrefabContents`.
- New fields added to the C# script that the prefab has never seen will NOT be shadowed — they pick up the class default correctly the first time the asset is next serialized (e.g. after a `modify_contents` call touches any field on that component, Unity re-serializes the whole MonoBehaviour block, filling in the new field at its default or whatever explicit value you passed).
