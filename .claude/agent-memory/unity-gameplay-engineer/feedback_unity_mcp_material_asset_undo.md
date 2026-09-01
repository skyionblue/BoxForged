---
name: unity-mcp-material-asset-undo
description: manage_material's real parameter names, a manage_material write that reads back correctly but doesn't persist to disk until a manage_asset "modify" call, and manage_editor undo/redo operating on Unity's global undo stack rather than anything scoped to the session.
metadata:
  type: feedback
---

Three Unity MCP tool quirks found while implementing B118/ADR-0007 (the Grasscutter's
ground-plane dash-lane telegraph, see [[project_backyard_dojo_build]] 15th pass):

**1. `manage_material`'s actual parameter names are snake_case and don't match its tool
description's examples.** `set_material_shader_property` takes `material_path`, `property`,
`value` — not `materialPath`/`propertyName`/`propertyType`/`floatValue` or any camelCase
variant. `get_material_info` takes `material_path`. Guessing from the tool description's prose
wastes several round trips; if a call fails with "unexpected keyword argument", try the
hinted correction first (the error message names the actual accepted argument), then fall back to
`get_material_info`'s successful call shape as a template for parameter naming on sibling actions
in the same tool.

**2. A `manage_material set_material_shader_property` call can report success and have
`get_material_info` read back the new value correctly, while the `.mat` file on disk is
unchanged.** This happened for `mat_TelegraphLane.mat`'s `_ZTest` property — set via
`manage_material`, confirmed via `get_material_info` (value 4), but a subsequent C# script
compile (domain reload) didn't touch it, and `cat`-ing the file afterward still showed
`m_Floats: []`. The in-memory material object was dirty but not flushed. Fix: follow up with
`manage_asset` `action: "modify"`, `path`, `properties: {"_ZTest": 4}` — that call *did* persist
it (confirmed via a direct file read afterward, and the harness surfaced the changed-on-disk
notice). **Always verify a material/asset property write by reading the file's raw content
afterward, not just by trusting a subsequent `get_material_info`/similar read-back through the
same tool family** — the read-back can be serving the same possibly-unflushed in-memory state.

**3. `manage_editor` `undo`/`redo` operate on Unity's real, global Editor undo stack — not
anything scoped to the current MCP session or the objects it touched.** Called `undo` once just
to see what it would report, and it undid an unrelated prior group ("SaveDuringPlay") that had
nothing to do with this session's changes. Had to immediately call `redo` to put it back. Treat
`manage_editor undo`/`redo` as a real, shared, stateful action with side effects on whatever the
Editor's undo history currently holds — never call it "just to check" or out of curiosity; only
use it when actually intending to undo/redo a specific known change, and verify what you got back
(`current_group`/`next_group` in the response) before assuming it did what you meant.
