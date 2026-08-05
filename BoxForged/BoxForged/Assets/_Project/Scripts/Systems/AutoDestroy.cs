using UnityEngine;

namespace Boxhead.Systems
{
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField] private float lifetime = 1f;

        private void Start() => Destroy(gameObject, lifetime);
    }
}
