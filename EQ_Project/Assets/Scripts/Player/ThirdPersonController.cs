using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 6.0f;

        [Tooltip("Sprint speed of the character in m/s (set equal to MoveSpeed to disable sprint)")]
        public float SprintSpeed = 8.5f;

        [Range(0.0f, 0.3f)]
        [Tooltip("How fast the character turns to face movement direction")]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 12.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.5f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again.")]
        public float JumpTimeout = 0.20f;

        [Tooltip("Time required to pass before entering the fall state.")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Movement Reference")]
        [Tooltip("Optional: reference transform that defines world-forward for movement (recommended for isometric camera).")]
        [SerializeField] Transform movementReference;

        [Header("Runtime")]
        [Tooltip("Set false to freeze movement (useful in Balance mode).")]
        public bool MovementEnabled = true;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        // player
        float _speed;
        float _animationBlend;
        float _targetRotation;
        float _rotationVelocity;
        float _verticalVelocity;
        const float _terminalVelocity = 53.0f;

        // timeout deltatime
        float _jumpTimeoutDelta;
        float _fallTimeoutDelta;

        // animation IDs
        int _animIDSpeed;
        int _animIDGrounded;
        int _animIDJump;
        int _animIDFreeFall;
        int _animIDMotionSpeed;

        Animator _animator;
        CharacterController _controller;
        StarterAssetsInputs _input;
        GameObject _mainCamera;

        bool _hasAnimator;

        void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

#if !ENABLE_INPUT_SYSTEM
            Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        void Update()
        {
            if (!MovementEnabled)
            {
                // keep gravity stable so controller doesn't "float" if disabled mid-air
                GroundedCheck();
                ApplyGravityOnly();
                AnimateIdle();
                return;
            }

            GroundedCheck();
            JumpAndGravity();
            Move();
        }

        void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
                _animator.SetBool(_animIDGrounded, Grounded);
        }

        void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else _speed = targetSpeed;

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                float referenceYaw = GetReferenceYaw();
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + referenceYaw;

                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

                FindObjectOfType<TutorialManager>()?.SetFlag("Moved");
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        float GetReferenceYaw()
        {
            if (movementReference != null)
                return movementReference.eulerAngles.y;

            if (_mainCamera != null)
                return _mainCamera.transform.eulerAngles.y;

            return transform.eulerAngles.y;
        }

        void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    FindObjectOfType<TutorialManager>()?.SetFlag("Jump");
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    if (_hasAnimator) _animator.SetBool(_animIDJump, true);
                }

                if (_jumpTimeoutDelta >= 0.0f)
                    _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f) _fallTimeoutDelta -= Time.deltaTime;
                else if (_hasAnimator) _animator.SetBool(_animIDFreeFall, true);

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
                _verticalVelocity += Gravity * Time.deltaTime;
        }

        void ApplyGravityOnly()
        {
            if (Grounded)
            {
                if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;
            }
            else
            {
                if (_verticalVelocity < _terminalVelocity)
                    _verticalVelocity += Gravity * Time.deltaTime;
            }

            _controller.Move(new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        void AnimateIdle()
        {
            if (!_hasAnimator) return;
            _animator.SetFloat(_animIDSpeed, 0f);
            _animator.SetFloat(_animIDMotionSpeed, 0f);
            _animator.SetBool(_animIDJump, false);
            _animator.SetBool(_animIDFreeFall, false);
        }

        void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = Grounded ? transparentGreen : transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight <= 0.5f) return;
            if (FootstepAudioClips == null || FootstepAudioClips.Length == 0) return;

            var index = Random.Range(0, FootstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }

        void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight <= 0.5f) return;
            if (LandingAudioClip == null) return;

            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }
}