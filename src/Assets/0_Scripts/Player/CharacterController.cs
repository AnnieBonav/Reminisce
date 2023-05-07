using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    public static event Action GrabbedObject;

    [Header("Moving")]
    [SerializeField] float _speed = 8f;
    [SerializeField] float _turnSpeed = 10f;
    [SerializeField] float _runSpeedMultiplier = 2f;

    [Header("Jumping")]
    [SerializeField] float _jumpForce = 20f;
    [SerializeField] private float _coyoteTime = 0.3f;
    [SerializeField] float _jumpVelocityFalloff = 5f;
    [SerializeField] float _fallMultiplier = 8f;

    [Header("Ground Detection")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundRadius = 0.1f;
    [SerializeField] private LayerMask _whatIsGround;
    [SerializeField] private LayerMask _whatIsGrabbable;

    [Header("Audio Clips")]
    [SerializeField] AudioClip[] _plantGrabSounds;
    [SerializeField] AudioClip[] _jumpSounds;
    [SerializeField] AudioClip[] _jumpFallSounds;

    [Header("Audio Parameters")]
    [Range(0.1f, 0.5f)]
    float _volumeChangeMultiplier = 0.2f;
    [Range(0.1f, 0.5f)]
    float _pitchChangeMultiplier = 0.2f;

    Rigidbody _rb;
    Camera _camera;
    Animator _animator;
    AudioSource _audioSource;

    Vector3 _movement;
    Vector3 _moveDirection;
    float _turnSmoothVelocity;
    float _movementSmoothing;

    bool _isGrounded;
    bool _isJumpPressed;
    bool _isGrabPressed;
    bool _isJumping = false;
    private float _timeLeftGrounded = -10f;

    bool _isDashPressed;
    float _idleTime;

    private GameObject _grabbedObject;

    // Grabbing 
    bool _isCollidingPopup;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _camera = Camera.main;
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        if(_groundCheck == null){
            _groundCheck = gameObject.transform.GetChild(0).GetComponent<Transform>();
        }

        // TODO: Check if GameStateManager is needed
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;

        // Grabbing objects
        Popup.EnteredPopup += SetCollidingPopup;
        Popup.LeftPopup += RemoveCollidingPopup;
    }

    private void OnDestroy()
    {
        Popup.EnteredPopup -= SetCollidingPopup;
        Popup.LeftPopup -= RemoveCollidingPopup;
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void SetCollidingPopup(string text) // Do not really use it
    {
        _isCollidingPopup = true;
    }

    private void RemoveCollidingPopup()
    {
        _isCollidingPopup = false;
    }

    private void OnGameStateChanged(GameState newGameState)
    {
        enabled = newGameState == GameState.Gameplay;
    }

    void Update()
    {
        HandlePlayerRotation();
        HandleAnimation();
    }

    void FixedUpdate()
    {
        // Moving behavior
        HandleMove();
        HandleDash();

        // Jumping behavior
        HandleGrounding();
        HandleJump();

        // Move player towards direction
        _rb.AddForce(_moveDirection * _speed);
    }


    void HandleAnimation()
    {
        bool isWalking = _animator.GetBool("IsWalking");

        _animator.SetBool("IsDashing", (_isDashPressed && !_moveDirection.Equals(Vector3.zero)) || (!_isGrounded && !_moveDirection.y.Equals(0f)));
        _animator.SetBool("IsWalking", !_moveDirection.Equals(Vector3.zero));
        _animator.SetBool("IsLongIdle", _idleTime % 20f > 10f);
        _animator.SetBool("IsGrabbing", _isGrabPressed);
    }

    void HandlePlayerRotation()
    {
        if (!_movement.Equals(Vector3.zero))
        {
            // Rotate player based on input direction, taking into account isometric perspective
            float targetAngle = Mathf.Atan2(_movement.x, _movement.z) * Mathf.Rad2Deg + _camera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, 1f / _turnSpeed);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        else
        {
            _rb.angularVelocity = Vector3.zero;
            _idleTime += Time.deltaTime;
        }
    }

    void HandleDash()
    {
        // Run
        float moveSpeed = _speed;

        if (_isDashPressed)
        {
            moveSpeed *= _runSpeedMultiplier;
        }

        _moveDirection *= moveSpeed;
    }

    void HandleMove()
    {
        // Move direction relative to isometric camera
        _moveDirection = _movement.x * _camera.transform.right + _movement.z * _camera.transform.forward;
        _moveDirection.y = 0;
    }

    void HandleGrounding()
    {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundRadius, _whatIsGround);
        print("Is gorunded: " + _isGrounded);
    }

    public void OnJump(InputValue value)
    {
        _isJumpPressed = value.isPressed;
    }

    void HandleJump()
    {
        if (!_isJumping && _isJumpPressed && (_isGrounded || Time.time < _timeLeftGrounded + _coyoteTime))
        {
            _isJumping = true;
            _moveDirection.y = _jumpForce;
            //_rb.AddForce(new Vector3(0, _jumpForce, 0));

            PlaySoundEffect(_jumpSounds);
        }
        else if (!_isJumpPressed && _isJumping && _isGrounded)
        {
            _isJumping = false;

            PlaySoundEffect(_jumpFallSounds);
        }

        // Fall faster and allow small jumps. _jumpVelocityFalloff is the point at which we start adding extra gravity. Using 0 causes floating
        if (_rb.velocity.y < _jumpVelocityFalloff || _rb.velocity.y > 0 && !_isJumpPressed)
        {
            _rb.velocity += Vector3.up * Physics.gravity.y * _fallMultiplier * Time.deltaTime;
        }
    }

    // Sound Effect Handler
    public void PlaySoundEffect(AudioClip[] sounds)
    {
        _audioSource.clip = sounds[UnityEngine.Random.Range(0, sounds.Length)];
        _audioSource.volume = UnityEngine.Random.Range(1 - _volumeChangeMultiplier, 1);
        _audioSource.pitch = UnityEngine.Random.Range(1 - _pitchChangeMultiplier, 1 + _pitchChangeMultiplier);
        _audioSource.PlayOneShot(_audioSource.clip);
    }

    public void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();
        _movement = new Vector3(inputVector.x, 0, inputVector.y).normalized;

        _idleTime = 0f;
    }

    public void OnDash(InputValue value)
    {
        _isDashPressed = value.isPressed;
    }

    public void OnGrabObject(InputValue value)
    {
        _isGrabPressed = value.isPressed;
        CheckInteractPopup();
    }

    private void CheckInteractPopup()
    {
        if (_isCollidingPopup)
        {
            GrabbedObject?.Invoke();
        }
    }
}
