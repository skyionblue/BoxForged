using UnityEngine;

namespace Boxhead.Enemy
{
    public class LaundryTumbler : MonoBehaviour
    {
        [SerializeField] private float _tumbleSpeed = 180f;

        private void Update()
        {
            transform.Rotate(0f, _tumbleSpeed * Time.deltaTime, 0f, Space.Self);
        }
    }
}
