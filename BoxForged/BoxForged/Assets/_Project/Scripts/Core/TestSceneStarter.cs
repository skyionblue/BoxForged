using System.Collections;
using UnityEngine;
using Boxhead.Systems;
using Boxhead.UI;

namespace Boxhead.Core
{
    // Wires the test scene's runtime-spawned workbenches to the ForgePanel.
    // Does NOT touch character selection — let the player pick normally via RunStartUI.
    // Remove from scenes before shipping — test use only.
    public class TestSceneStarter : MonoBehaviour
    {
        private IEnumerator Start()
        {
            // Wait one frame so LevelBuilder.Start() has finished spawning all props
            yield return null;

            var forgePanel = FindAnyObjectByType<ForgePanel>(FindObjectsInactive.Include);
            var workbenches = FindObjectsByType<WorkbenchProp>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < workbenches.Length; i++)
            {
                if (forgePanel != null)
                {
                    workbenches[i].OnPlayerEntered += forgePanel.Open;
                    workbenches[i].OnPlayerExited  += forgePanel.Close;
                }
            }

            Debug.Log($"[TestSceneStarter] Registered {workbenches.Length} workbench(es) with forge panel: {(forgePanel != null ? "found" : "NOT FOUND")}");
        }
    }
}
