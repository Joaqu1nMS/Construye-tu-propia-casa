using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public enum MoveState { Idle, Walk, Run, Crouch }
    public MoveState estadoPlayer { get; private set; }

    [Header("Referencias")]
    public Camera playerCamera;

    [Header("Movimiento")]
    [Tooltip("Velocidad andando")]
    public float walkSpeed = 5f;

    [Tooltip("Velocidad corriendo")]
    public float sprintSpeed = 10f;

    [Tooltip("Velocidad agachado")]
    public float crouchSpeed = 2.5f;

    [Header("Salto y gravedad")]
    [Tooltip("Fuerza de salto")]
    public float jumpForce = 5f;

    [Tooltip("Escala para la gravedad")]
    public float gravity = 20f;

    [Header("Control de raton")]
    [Tooltip("Sensibilidad")]
    public float sensibilidadRaton = 2f;

    [Tooltip("Angulo maximo hacia arriba")]
    public float maxLookAngle = 80f;

    [Header("Crosshair")]
    [Tooltip("Radio del punto")]
    public float radioCrosshair = 4f;

    [Tooltip("Color del punto")]
    public Color colorCrosshair = Color.white;

    [Header("Crouch")]
    [Tooltip("Altura de pie")]
    public float alturaDepie = 2f;

    [Tooltip("Altura agachado")]
    public float alturaAgachado = 1f;

    [Tooltip("Velocidad Depie -> Agachado")]
    public float crouchTransitionSpeed = 10f;

    private CharacterController characterController;
    private Vector3 velocidad;          
    private float anguloVerticalAct;  // Angulo de la mirada
    private bool estaAgachado;
    private bool estaCorriendo;
    private float alturaActual; // Depie o agachado

    private AudioSource audioSource;
    private Coroutine pasosCoroutine;
    [SerializeField] private AudioClip paso;

    public bool isBlocked = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        alturaActual = alturaDepie;
        characterController.height = alturaDepie;

        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        bool moviendose = estadoPlayer != MoveState.Idle && estadoPlayer != MoveState.Crouch;
        //Debug.Log($"Supuesto estado {estadoPlayer}");
        if (moviendose && pasosCoroutine == null)
        {
            //Debug.Log("Inicio coroutina");
            pasosCoroutine = StartCoroutine(SonidoPasos());
        }
        else if (!moviendose && pasosCoroutine != null)
        {
            //Debug.Log("Borro coroutina");
            StopCoroutine(pasosCoroutine);
            pasosCoroutine = null;
        }

        if (isBlocked) {
            characterController.Move(Vector3.zero); 
            return;
        }
        
        if (Time.timeScale == 0f)
        {
            radioCrosshair = 0f; // quitar mira cuando pausa
            return;
        }
        radioCrosshair = 4f;
        HandleMouseLook();
        HandleCrouch();
        HandleMovement();
    }

    // Raton
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadRaton;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadRaton;

        // Rotacion horizontal del player
        transform.Rotate(Vector3.up * mouseX);

        // Angulo de mirada
        anguloVerticalAct -= mouseY;
        anguloVerticalAct = Mathf.Clamp(anguloVerticalAct, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(anguloVerticalAct, 0f, 0f);
    }

    // Movimiento y salto
    private void HandleMovement()
    {
        bool grounded = characterController.isGrounded;

        if (grounded && velocidad.y < 0f)
            velocidad.y = -2f;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        estaCorriendo = !estaAgachado &&
                       (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));

        float currentSpeed = estaAgachado ? crouchSpeed : (estaCorriendo ? sprintSpeed : walkSpeed);
        velocidad.x = move.x * currentSpeed;
        velocidad.z = move.z * currentSpeed;

        if (Input.GetButtonDown("Jump") && grounded && !estaAgachado)
            velocidad.y = jumpForce;

        velocidad.y -= gravity * Time.deltaTime;

        characterController.Move(velocidad * Time.deltaTime);

        if (estaAgachado)
        {
            estadoPlayer = MoveState.Crouch;
        }
        else if (horizontal != 0 || vertical != 0) 
        {
            estadoPlayer = estaCorriendo ? MoveState.Run : MoveState.Walk;
        }
        else
        {
            estadoPlayer = MoveState.Idle;
        }
    }

    private void HandleCrouch()
    {
        bool crouchPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (!crouchPressed && estaAgachado && CanStandUp())
            estaAgachado = false;
        else if (crouchPressed)
            estaAgachado = true;

        alturaActual = estaAgachado ? alturaAgachado : alturaDepie;

        if (!Mathf.Approximately(characterController.height, alturaActual))
        {
            float newHeight = Mathf.Lerp(characterController.height, alturaActual, crouchTransitionSpeed * Time.deltaTime);
            float heightDelta = newHeight - characterController.height;

            characterController.center = new Vector3(0f, characterController.center.y + heightDelta / 2f, 0f);
            characterController.height = newHeight;

            Vector3 camLocal = playerCamera.transform.localPosition;
            playerCamera.transform.localPosition = new Vector3(camLocal.x, characterController.height * 0.9f, camLocal.z);
        }
    }

    private bool CanStandUp()
    {
        Vector3 origin = transform.position + Vector3.up * (characterController.height / 2f);
        float checkDistance = alturaDepie - alturaAgachado;
        return !Physics.SphereCast(origin, characterController.radius * 0.9f, Vector3.up, out _, checkDistance);
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
            MoveState estadoActual = estadoPlayer;

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
        float diameter = radioCrosshair * 2f;

        GUI.color = colorCrosshair;
        GUI.DrawTexture(
            new Rect(cx - radioCrosshair, cy - radioCrosshair, diameter, diameter),
            Texture2D.whiteTexture
        );
        GUI.color = Color.white; 
    }
}