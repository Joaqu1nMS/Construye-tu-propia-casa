using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public enum MoveState { Idle, Walk, Run, Crouch }
    public MoveState CurrentMoveState { get; private set; }

    [Header("References")]
    [Tooltip("Assign the child Camera used as the player's eyes.")]
    public Camera playerCamera;

    [Header("Movement")]
    [Tooltip("Walking speed in m/s.")]
    public float walkSpeed = 5f;

    [Tooltip("Sprinting speed in m/s (hold Ctrl).")]
    public float sprintSpeed = 10f;

    [Tooltip("Crouching speed in m/s.")]
    public float crouchSpeed = 2.5f;

    [Header("Jump & Gravity")]
    [Tooltip("Vertical velocity applied when the player jumps.")]
    public float jumpForce = 5f;

    [Tooltip("Gravity scale (positive = downward).")]
    public float gravity = 20f;

    [Header("Mouse Look")]
    [Tooltip("Mouse sensitivity.")]
    public float mouseSensitivity = 2f;

    [Tooltip("Maximum vertical look angle (degrees above horizon).")]
    public float maxLookAngle = 80f;

    [Header("Crosshair")]
    [Tooltip("Radius of the crosshair dot in pixels.")]
    public float crosshairRadius = 4f;

    [Tooltip("Color of the crosshair dot.")]
    public Color crosshairColor = Color.white;

    [Header("Crouch")]
    [Tooltip("CharacterController height while standing.")]
    public float standHeight = 2f;

    [Tooltip("CharacterController height while crouching.")]
    public float crouchHeight = 1f;

    [Tooltip("How fast the player transitions between crouch and stand.")]
    public float crouchTransitionSpeed = 10f;

    private CharacterController _cc;
    private Vector3 _velocity;          
    private float _verticalLookAngle;  
    private bool _isCrouching;
    private bool _isSprinting;
    private float _targetHeight;

    private AudioSource audioSource;
    private Coroutine pasosCoroutine;
    [SerializeField] private AudioClip paso;


    public bool isBlocked = false;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _targetHeight = standHeight;
        _cc.height = standHeight;

        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        bool moviendose = CurrentMoveState != MoveState.Idle && CurrentMoveState != MoveState.Crouch;
        Debug.Log($"Supuesto estado {CurrentMoveState}");
        if (moviendose && pasosCoroutine == null)
        {
            Debug.Log("Inicio coroutina");
            pasosCoroutine = StartCoroutine(SonidoPasos());
        }
        else if (!moviendose && pasosCoroutine != null)
        {
            Debug.Log("Borro coroutina");
            StopCoroutine(pasosCoroutine);
            pasosCoroutine = null;
        }

        if (isBlocked) {
            _cc.Move(Vector3.zero); 
            return;
        }
        
        if (Time.timeScale == 0f)
        {
            crosshairRadius = 0f; // quitar mira cuando pausa
            return;
        }
        crosshairRadius = 4f;
        HandleMouseLook();
        HandleCrouch();
        HandleMovement();
    }

    //raton
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Horizontal rotation – rotate the whole player body
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation – pitch the camera only
        _verticalLookAngle -= mouseY;
        _verticalLookAngle = Mathf.Clamp(_verticalLookAngle, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(_verticalLookAngle, 0f, 0f);
    }

    //movimiento y salto
    private void HandleMovement()
    {
        bool grounded = _cc.isGrounded;

        if (grounded && _velocity.y < 0f)
            _velocity.y = -2f;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        _isSprinting = !_isCrouching &&
                       (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));

        float currentSpeed = _isCrouching ? crouchSpeed : (_isSprinting ? sprintSpeed : walkSpeed);
        _velocity.x = move.x * currentSpeed;
        _velocity.z = move.z * currentSpeed;

        if (Input.GetButtonDown("Jump") && grounded && !_isCrouching)
            _velocity.y = jumpForce;

        _velocity.y -= gravity * Time.deltaTime;

        _cc.Move(_velocity * Time.deltaTime);

        if (_isCrouching)
        {
            CurrentMoveState = MoveState.Crouch;
        }
        else if (horizontal != 0 || vertical != 0) 
        {
            CurrentMoveState = _isSprinting ? MoveState.Run : MoveState.Walk;
        }
        else
        {
            CurrentMoveState = MoveState.Idle;
        }
    }

    private void HandleCrouch()
    {
        bool crouchPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (!crouchPressed && _isCrouching && CanStandUp())
            _isCrouching = false;
        else if (crouchPressed)
            _isCrouching = true;

        _targetHeight = _isCrouching ? crouchHeight : standHeight;

        if (!Mathf.Approximately(_cc.height, _targetHeight))
        {
            float newHeight = Mathf.Lerp(_cc.height, _targetHeight, crouchTransitionSpeed * Time.deltaTime);
            float heightDelta = newHeight - _cc.height;

            _cc.center = new Vector3(0f, _cc.center.y + heightDelta / 2f, 0f);
            _cc.height = newHeight;

            Vector3 camLocal = playerCamera.transform.localPosition;
            playerCamera.transform.localPosition = new Vector3(camLocal.x, _cc.height * 0.9f, camLocal.z);
        }
    }

    private bool CanStandUp()
    {
        Vector3 origin = transform.position + Vector3.up * (_cc.height / 2f);
        float checkDistance = standHeight - crouchHeight;
        return !Physics.SphereCast(origin, _cc.radius * 0.9f, Vector3.up, out _, checkDistance);
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator SonidoPasos()
    {
        while (true)
        {
            MoveState estadoActual = CurrentMoveState;

            if (estadoActual != MoveState.Walk && estadoActual != MoveState.Run)
            {
                pasosCoroutine = null;
                yield break;
            }

            ReproducirSonido(paso);

            float espera = estadoActual == MoveState.Run ? 0.2f : 0.5f;
            yield return new WaitForSeconds(espera);
        }
    }

    private void ReproducirSonido(AudioClip sfx)
    {
        audioSource.volume = GameManager.gameM.SFX.volume;
        audioSource.pitch = Random.Range(0.9f, 1.5f);        
        audioSource.clip = sfx;
        audioSource.Play();
    }

    private void OnGUI()
    {
        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        float diameter = crosshairRadius * 2f;

        GUI.color = crosshairColor;
        GUI.DrawTexture(
            new Rect(cx - crosshairRadius, cy - crosshairRadius, diameter, diameter),
            Texture2D.whiteTexture
        );
        GUI.color = Color.white; 
    }
}
