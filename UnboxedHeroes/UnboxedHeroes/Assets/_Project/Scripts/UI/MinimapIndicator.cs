using UnityEngine;

namespace Boxhead.UI
{
    /// <summary>
    /// Attaches a small colored disc above an entity that is only visible to the
    /// minimap camera (layer 20 = "Minimap"). The disc floats at a fixed world-Y
    /// altitude so the minimap camera always sees it regardless of terrain height.
    /// </summary>
    public class MinimapIndicator : MonoBehaviour
    {
        [SerializeField] private Color  _color        = Color.red;
        [SerializeField] private float  _discRadius   = 0.8f;
        [SerializeField] private float  _altitudeY    = 40f;   // world Y where the disc sits
        [SerializeField] private int    _minimapLayer = 20;

        private Transform _disc;

        private void Awake()
        {
            // Create a flat cylinder disc as the minimap blip
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "MinimapBlip";
            go.layer = _minimapLayer;
            Destroy(go.GetComponent<CapsuleCollider>());

            go.transform.SetParent(null); // world-space, not parented to entity
            go.transform.localScale = new Vector3(_discRadius * 2f, 0.05f, _discRadius * 2f);

            // Solid color URP material
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = _color;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.GetComponent<MeshRenderer>().receiveShadows = false;

            _disc = go.transform;
        }

        private void LateUpdate()
        {
            if (_disc == null) return;
            // Keep disc directly above entity at fixed altitude
            Vector3 pos = transform.position;
            pos.y = _altitudeY;
            _disc.position = pos;
        }

        private void OnDestroy()
        {
            if (_disc != null)
                Destroy(_disc.gameObject);
        }
    }
}
