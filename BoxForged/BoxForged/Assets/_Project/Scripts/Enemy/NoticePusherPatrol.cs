using UnityEngine;

namespace Boxhead.Enemy
{
    [RequireComponent(typeof(CharacterController))]
    public class NoticePusherPatrol : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 1.8f;
        [SerializeField] private float _patrolRadius = 4f;
        [SerializeField] private float _rotateSpeed = 120f;

        private Animator _animator;
        private CharacterController _controller;
        private Vector3 _center;
        private float _angle;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _controller = GetComponent<CharacterController>();
            _center = transform.position;
        }

        private void Update()
        {
            _angle += _moveSpeed / _patrolRadius * Time.deltaTime * Mathf.Rad2Deg;
            if (_angle >= 360f) _angle -= 360f;

            float rad = _angle * Mathf.Deg2Rad;
            Vector3 target = _center + new Vector3(Mathf.Sin(rad) * _patrolRadius, 0f, Mathf.Cos(rad) * _patrolRadius);

            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion look = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, look, _rotateSpeed * Time.deltaTime);
            }

            Vector3 motion = dir.normalized * _moveSpeed * Time.deltaTime;
            motion.y -= 9.81f * Time.deltaTime;
            _controller.Move(motion);

            if (_animator != null)
                _animator.SetFloat("Speed", 1f);
        }

        // Silent receivers for RPG Mecanim footstep animation events
        private void FootR() { }
        private void FootL() { }
    }
}
