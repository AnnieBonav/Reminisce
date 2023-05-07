using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Moving")]
    [SerializeField] float _speed = 0.6f;
    [SerializeField] float _turnSpeed = 10f;
    [SerializeField] float _runSpeedMultiplier = 2f;

    [Header("Jumping")]
    [SerializeField] float _jumpForce = 12f;
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

    bool _dialogueOpened = false;
    bool _grabbedObject;
    bool _isGrounded;
    bool _isJumpPressed;
    bool _isGrabPressed;
    bool _isJumping;
    private float _timeLeftGrounded = -10f;

    bool _isDashPressed;
    float _idleTime;


    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _camera = Camera.main;
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        if(_groundCheck == null){
            _groundCheck = gameObject.transform.GetChild(0).GetComponent<Transform>();
        }

        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
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
    }

    void HandleJump()
    {
        // Jump
        if (!_isJumping && _isJumpPressed && (_isGrounded || Time.time < _timeLeftGrounded + _coyoteTime))
        {
            _isJumping = true;
            _moveDirection.y = _jumpForce;

            playSoundEffect(_jumpSounds);
        }
        else if (!_isJumpPressed && _isJumping && _isGrounded)
        {
            _isJumping = false;

            playSoundEffect(_jumpFallSounds);
        }

        // Fall faster and allow small jumps. _jumpVelocityFalloff is the point at which we start adding extra gravity. Using 0 causes floating
        if (_rb.velocity.y < _jumpVelocityFalloff || _rb.velocity.y > 0 && !_isJumpPressed)
        {
            _rb.velocity += Vector3.up * Physics.gravity.y * _fallMultiplier * Time.deltaTime;
        }
    }

    // Sound Effect Handler
    public void playSoundEffect(AudioClip[] sounds)
    {
        _audioSource.clip = sounds[Random.Range(0, sounds.Length)];
        _audioSource.volume = Random.Range(1 - _volumeChangeMultiplier, 1);
        _audioSource.pitch = Random.Range(1 - _pitchChangeMultiplier, 1 + _pitchChangeMultiplier);
        _audioSource.PlayOneShot(_audioSource.clip);
    }

    // Input Action Messages

    public void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();
        _movement = new Vector3(inputVector.x, 0, inputVector.y).normalized;

        _idleTime = 0f;
    }

    public void OnJump(InputValue value)
    {
        _isJumpPressed = value.isPressed;
    }

    public void OnDash(InputValue value)
    {
        _isDashPressed = value.isPressed;
    }

    public void OnGrabObject(InputValue value)
    {
        _isGrabPressed = value.isPressed;
        CheckObjectGrabbed();
    }

    void CheckObjectGrabbed()
    {
        _grabbedObject = Physics.CheckSphere(_groundCheck.position, 1f, _whatIsGrabbable);
        if (_grabbedObject)
        {
            GrabObject();
        }
        Debug.Log("Here" + _grabbedObject);

    }
    public void OnCollisionStay(Collision collidedObject)
    {
        //Debug.Log(collidedObject.collider.name);
        /* if (collidedObject.collider.CompareTag("Grabbable"))
        {
            //Debug.Log("here");
        }*/
    }

    void GrabObject()
    {

    }
}
