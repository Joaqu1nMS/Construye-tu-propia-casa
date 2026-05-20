using UnityEngine;

/// <summary>
/// First-person player controller.
/// Requires: CharacterController component on the same GameObject.
/// The Camera must be a child of this GameObject (assigned in Inspector).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ──────────────────────────────────────────────
    //  Inspector Settings
    // ──────────────────────────────────────────────

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

    // ──────────────────────────────────────────────
    //  Private State
    // ──────────────────────────────────────────────

    private CharacterController _cc;
    private Vector3 _velocity;          // accumulated movement vector
    private float _verticalLookAngle;   // camera pitch tracker
    private bool _isCrouching;
    private bool _isSprinting;
    private float _targetHeight;

    // ──────────────────────────────────────────────
    //  Unity Lifecycle
    // ──────────────────────────────────────────────

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        // Lock and hide cursor for FPS feel
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _targetHeight = standHeight;
        _cc.height = standHeight;
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            crosshairRadius = 0f; // Hide crosshair when paused
            return;
        } 
        crosshairRadius = 4f; 
        HandleMouseLook();
        HandleCrouch();
        HandleMovement();
    }

    // ──────────────────────────────────────────────
    //  Mouse Look
    // ──────────────────────────────────────────────

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

    // ──────────────────────────────────────────────
    //  Movement & Jump
    // ──────────────────────────────────────────────

    private void HandleMovement()
    {
        bool grounded = _cc.isGrounded;

        if (grounded && _velocity.y < 0f)
            _velocity.y = -2f; // Small negative to keep grounded

        // WASD input (relative to player facing direction)
        float horizontal = Input.GetAxis("Horizontal"); // A / D
        float vertical   = Input.GetAxis("Vertical");   // W / S

        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // Sprint: hold Left or Right Ctrl (disabled while crouching)
        _isSprinting = !_isCrouching &&
                       (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));

        float currentSpeed = _isCrouching ? crouchSpeed : (_isSprinting ? sprintSpeed : walkSpeed);
        _velocity.x = move.x * currentSpeed;
        _velocity.z = move.z * currentSpeed;

        // Jump
        if (Input.GetButtonDown("Jump") && grounded && !_isCrouching)
            _velocity.y = jumpForce;

        // Gravity
        _velocity.y -= gravity * Time.deltaTime;

        _cc.Move(_velocity * Time.deltaTime);
    }

    // ──────────────────────────────────────────────
    //  Crouch
    // ──────────────────────────────────────────────

    private void HandleCrouch()
    {
        bool crouchPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Prevent standing if something is overhead
        if (!crouchPressed && _isCrouching && CanStandUp())
            _isCrouching = false;
        else if (crouchPressed)
            _isCrouching = true;

        _targetHeight = _isCrouching ? crouchHeight : standHeight;

        // Smoothly lerp CharacterController height
        if (!Mathf.Approximately(_cc.height, _targetHeight))
        {
            float newHeight = Mathf.Lerp(_cc.height, _targetHeight, crouchTransitionSpeed * Time.deltaTime);
            float heightDelta = newHeight - _cc.height;

            // Adjust center so feet stay on the ground
            _cc.center = new Vector3(0f, _cc.center.y + heightDelta / 2f, 0f);
            _cc.height = newHeight;

            // Move camera to match new eye level
            Vector3 camLocal = playerCamera.transform.localPosition;
            playerCamera.transform.localPosition = new Vector3(camLocal.x, _cc.height * 0.9f, camLocal.z);
        }
    }

    /// <summary>Returns true if there is no obstacle preventing the player from standing.</summary>
    private bool CanStandUp()
    {
        Vector3 origin = transform.position + Vector3.up * (_cc.height / 2f);
        float checkDistance = standHeight - crouchHeight;
        return !Physics.SphereCast(origin, _cc.radius * 0.9f, Vector3.up, out _, checkDistance);
    }

    // ──────────────────────────────────────────────
    //  Public Helpers
    // ──────────────────────────────────────────────

    /// <summary>Unlock the cursor (e.g., for UI menus).</summary>
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Re-lock the cursor after a menu closes.</summary>
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ──────────────────────────────────────────────
    //  Crosshair
    // ──────────────────────────────────────────────

    private void OnGUI()
    {
        float cx = Screen.width  * 0.5f;
        float cy = Screen.height * 0.5f;
        float diameter = crosshairRadius * 2f;

        GUI.color = crosshairColor;
        // DrawTexture expects a Rect; we draw a filled square and let the
        // circular shape be implied by its small size (4 px radius ≈ dot).
        GUI.DrawTexture(
            new Rect(cx - crosshairRadius, cy - crosshairRadius, diameter, diameter),
            Texture2D.whiteTexture
        );
        GUI.color = Color.white; // Reset so other GUI elements are unaffected
    }
}
