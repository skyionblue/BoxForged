using UnityEngine;

namespace Boxhead.Systems
{
    public class TumbleweedRoller : MonoBehaviour
    {
        [SerializeField] private float _speed = 2.5f;
        [SerializeField] private Vector3 _windDirection = new Vector3(1f, 0f, 0.4f);
        [SerializeField] private float _boundaryHalfSize = 12f;
        [SerializeField] private float _rollDegreesPerUnit = 200f;
        [SerializeField] private float _startDelayMax = 5f;

        private Vector3 _rollAxis;
        private float _delayRemaining;
        private Vector3 _localMeshCenter;
        private float _sphereRadius;

        private void Awake()
        {
            _windDirection = _windDirection.normalized;
            _rollAxis = Vector3.Cross(Vector3.up, _windDirection).normalized;
            _delayRemaining = Random.Range(0f, _startDelayMax);

            var mf = GetComponentInChildren<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                _localMeshCenter = mf.sharedMesh.bounds.center;
                var ext = mf.sharedMesh.bounds.extents;
                _sphereRadius = Mathf.Max(ext.x, ext.y, ext.z) * transform.localScale.x;
            }
            else
            {
                _localMeshCenter = Vector3.zero;
                _sphereRadius = 0.3f;
            }
        }

        private void Update()
        {
            if (_delayRemaining > 0f)
            {
                _delayRemaining -= Time.deltaTime;
                return;
            }

            float step = _speed * Time.deltaTime;
            transform.position += _windDirection * step;
            transform.Rotate(_rollAxis, _rollDegreesPerUnit * step, Space.World);

            // Keep the visual sphere center at sphere-radius height so the bottom
            // always sits on the ground regardless of pivot offset or rotation angle.
            Vector3 worldCenter = transform.TransformPoint(_localMeshCenter);
            Vector3 p = transform.position;
            p.y += _sphereRadius - worldCenter.y;

            if (p.x > _boundaryHalfSize)
                p.x -= _boundaryHalfSize * 2f;
            else if (p.x < -_boundaryHalfSize)
                p.x += _boundaryHalfSize * 2f;

            if (p.z > _boundaryHalfSize)
                p.z -= _boundaryHalfSize * 2f;
            else if (p.z < -_boundaryHalfSize)
                p.z += _boundaryHalfSize * 2f;

            transform.position = p;
        }
    }
}
