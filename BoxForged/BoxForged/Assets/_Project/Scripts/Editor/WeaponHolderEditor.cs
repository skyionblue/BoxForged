using UnityEditor;
using UnityEngine;
using Boxhead.Player;

namespace Boxhead.Editor
{
    [CustomEditor(typeof(WeaponHolder))]
    public class WeaponHolderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!Application.isPlaying) return;

            EditorGUILayout.Space(8);
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("⟳  Reapply Grip (Live Tuning)", GUILayout.Height(32)))
                ((WeaponHolder)target).ReapplyGrip();
            GUI.backgroundColor = Color.white;
            EditorGUILayout.HelpBox(
                "Adjust Grip fields on the WeaponData ScriptableObject, then press this button to see the change immediately without re-picking up the weapon.",
                MessageType.None);
        }
    }
}
