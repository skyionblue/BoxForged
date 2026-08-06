using UnityEngine;

namespace Boxhead.Enemy
{
    public class LaundryTumbler : MonoBehaviour
    {
        [SerializeField] private float _tumbleSpeed = 180f;
        [SerializeField] private Vector3 _tumbleAxis = Vector3.right;

        private void Update()
        {
            transform.Rotate(_tumbleAxis, _tumbleSpeed * Time.deltaTime, Space.Self);
        }
    }
}
