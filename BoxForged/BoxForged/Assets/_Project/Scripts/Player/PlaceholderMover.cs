using UnityEngine;
using UnityEngine.InputSystem;

namespace Boxhead.Player
{
    // Temporary test mover — camera-relative movement, rotation, animator Speed.
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlaceholderMover : MonoBehaviour
    {
        [SerializeField] float _speed        = 5f;
        [SerializeField] float _rotationSpeed = 720f;

        CharacterController _cc;
        Animator            _animator;
        Camera              _cam;
        Vector2             _moveInput;
        float               _verticalVelocity;

        static readonly int s_Speed          = Animator.StringToHash("Speed");
        static readonly int s_AttackTrigger  = Animator.StringToHash("AttackTrigger");
        static readonly int s_DodgeTrigger   = Animator.StringToHash("DodgeTrigger");
        static readonly int s_ParryTrigger   = Animator.StringToHash("ParryTrigger");
        static readonly int s_JumpTrigger    = Animator.StringToHash("Jump");

        void Awake()
        {
            _cc       = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>();
            _cam      = Camera.main;
        }

        void OnMove(InputValue value)   => _moveInput = value.Get<Vector2>();
        void OnAttack(InputValue value) { if (value.isPressed && _animator) _animator.SetTrigger(s_AttackTrigger); }
        void OnDodge(InputValue value)  { if (value.isPressed && _animator) _animator.SetTrigger(s_DodgeTrigger); }
        void OnParry(InputValue value)  { if (value.isPressed && _animator) _animator.SetTrigger(s_ParryTrigger); }
        void OnJump(InputValue value)   { if (value.isPressed && _animator) _animator.SetTrigger(s_JumpTrigger); }

        void Update()
        {
            // Grounded gravity — pin to ground rather than accumulate when grounded
            if (_cc.isGrounded)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += Physics.gravity.y * Time.deltaTime;

            // Camera-relative horizontal direction
            Vector3 camForward = _cam != null ? _cam.transform.forward : Vector3.forward;
            Vector3 camRight   = _cam != null ? _cam.transform.right   : Vector3.right;
            camForward.y = 0f; camForward.Normalize();
            camRight.y   = 0f; camRight.Normalize();

            Vector3 wish = (camForward * _moveInput.y + camRight * _moveInput.x);
            float   speed = Mathf.Clamp01(wish.magnitude);  // 0–1 for blend tree

            Vector3 move = wish.normalized * (_speed * Time.deltaTime);
            move.y = _verticalVelocity * Time.deltaTime;
            _cc.Move(move);

            // Rotate toward movement direction
            if (wish.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(wish.normalized);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, target, _rotationSpeed * Time.deltaTime);
            }

            if (_animator != null)
                _animator.SetFloat(s_Speed, speed);
        }
    }
}
