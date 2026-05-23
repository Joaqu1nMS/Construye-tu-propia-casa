using System.Collections;
using UnityEngine;
using TMPro; // Si usas Legacy Text, cambia por: using UnityEngine.UI;

/// ============================================================
///  DoorInteraction.cs  —  versión OnMouse*
/// ============================================================
///
///  SETUP
///  ─────────────────────────────────────────────────────────
///  Jerarquía recomendada:
///
///       [DoorPivot]          ← GameObject vacío en la bisagra
///       └── [DoorMesh]       ← Este script + Outline + Collider
///
///  1. Añade ESTE script al [DoorMesh] (necesita Collider).
///  2. Añade Outline.cs al mismo [DoorMesh].
///  3. El Collider del mesh debe estar ACTIVO al inicio.
///  4. Crea un Canvas (World Space) hijo de [DoorPivot]:
///       → Añade un TMP_Text dentro del Canvas
///       → Asigna Canvas  → campo "labelCanvas"
///       → Asigna TMP_Text → campo "labelText"
///  5. Asigna en el Inspector:
///       • doorPivot    → el Transform de [DoorPivot]
///       • doorCollider → el Collider de [DoorMesh]
///
///  REQUISITO: Physics.queriesHitTriggers o Collider NO trigger.
///  OnMouseOver requiere que la cámara tenga un PhysicsRaycaster
///  o simplemente que el objeto tenga un Collider no-trigger.
/// ============================================================

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Outline))]
public class DoorInteraction : MonoBehaviour
{
    // ─── REFERENCIAS ──────────────────────────────────────────
    [Header("Puerta")]
    [Tooltip("Transform que rota (el gozne/bisagra). Suele ser el padre de este objeto.")]
    public Transform doorPivot;

    [Tooltip("Collider de este mismo mesh. Se desactiva durante la animación.")]
    public Collider  doorCollider;

    // ─── LABEL ────────────────────────────────────────────────
    [Header("Label UI (World Space Canvas)")]
    public GameObject labelCanvas;
    public TMP_Text   labelText;
    // Si usas Legacy Text cambia TMP_Text por: public Text labelText;

    public string textoCerrada = "Presiona E para abrir";
    public string textoAbierta = "Presiona E para cerrar";

    //rango
    public float rango   = 3f;
    public GameObject player;


    // ─── ANIMACIÓN ────────────────────────────────────────────
    [Header("Animación")]
    public float anguloCerrada  = 0f;
    public float anguloAbierta  = 130f;
    public float duracionAnim   = 1.2f;

    // ─── ESTADO INTERNO ───────────────────────────────────────
    private Outline outline;
    private bool estaAbierta  = false;
    private bool estaAnimando = false;
    private bool mouseEncima  = false;

    // ══════════════════════════════════════════════════════════
    void Awake()
    {
        player = GameObject.FindObjectOfType<PlayerController>().gameObject; 
        outline = GetComponent<Outline>();

        if (doorCollider == null) doorCollider = GetComponent<Collider>();
        if (doorPivot    == null) doorPivot    = transform.parent;

        // Estado inicial: UI oculto, outline apagado
        SetOutlineActivo(false);
        SetLabelActivo(false);
    }

    void Update(){
        if (Time.timeScale == 0f) return;
    }

    // ══════════════════════════════════════════════════════════
    //  OnMouse* — Unity los llama automáticamente cuando el
    //  cursor pasa sobre el Collider de este GameObject
    // ══════════════════════════════════════════════════════════

    /// Cursor entra en el Collider
    
    /*void OnMouseEnter()
    {
        if (Time.timeScale == 0f) return; 
        if (estaAnimando) return;

        mouseEncima = true;
        ActualizarTextoLabel();
        SetLabelActivo(true);
        SetOutlineActivo(true);
    }*/

    /// Cursor sale del Collider
    void OnMouseExit()
    {
        mouseEncima = false;
        SetLabelActivo(false);
        SetOutlineActivo(false);
    }

    /// Cursor está sobre el Collider — detectamos la tecla E aquí
    void OnMouseOver()
    {   
        if (Time.timeScale == 0f) return; 

        if (Vector3.Distance(player.transform.position, transform.position) > rango)
        {
            // Si el jugador está fuera de rango, no mostrar UI ni permitir interacción
            SetLabelActivo(false);
            SetOutlineActivo(false);
            return;
        }
        else
        {
            mouseEncima = true;
            ActualizarTextoLabel();
            SetLabelActivo(true);
            SetOutlineActivo(true);
        }

        if (estaAnimando) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Ocultar UI al pulsar
            SetLabelActivo(false);
            SetOutlineActivo(false);

            if (estaAbierta)
                StartCoroutine(AnimarPuerta(anguloAbierta, anguloCerrada));
            else
                StartCoroutine(AnimarPuerta(anguloCerrada, anguloAbierta));
        }
    }

    // ══════════════════════════════════════════════════════════
    //  COROUTINE: rota la puerta y gestiona el Collider
    // ══════════════════════════════════════════════════════════
    public IEnumerator AnimarPuerta(float anguloInicio, float anguloFin)
    {
        estaAnimando = true;

        // 1. Desactivar hitbox durante el movimiento
        SetColliderActivo(false);

        float eulerX = doorPivot.localEulerAngles.x;
        float eulerZ = doorPivot.localEulerAngles.z;

        Quaternion rotInicio = Quaternion.Euler(eulerX, anguloInicio, eulerZ);
        Quaternion rotFin    = Quaternion.Euler(eulerX, anguloFin,    eulerZ);

        float t = 0f;

        while (t < duracionAnim)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / duracionAnim);

            // EaseInOut suaviza inicio y final
            float smooth = progress * progress * (3f - 2f * progress);

            doorPivot.localRotation = Quaternion.Lerp(rotInicio, rotFin, smooth);
            yield return null;
        }

        // 2. Posición exacta al terminar
        doorPivot.localRotation = rotFin;

        // 3. Cambiar estado
        estaAbierta = !estaAbierta;

        // 4. Reactivar hitbox
        SetColliderActivo(true);

        estaAnimando = false;

        // 5. Si el mouse sigue encima, volver a mostrar UI
        if (mouseEncima)
        {
            ActualizarTextoLabel();
            SetLabelActivo(true);
            SetOutlineActivo(true);
        }
    }

    public void AbrePuertaVecino()
    {   
        if(estaAbierta || estaAnimando)
        {
            Debug.Log("VECINO NO ABRE PUERTA");
            return; 
        }

        Debug.Log("VECINO ABRE PUERTA");
        StartCoroutine(AnimarPuerta(anguloCerrada, anguloAbierta));
    }

    // ══════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════
    void SetOutlineActivo(bool activo)
    {
        if (outline != null) outline.enabled = activo;
    }

    void SetLabelActivo(bool activo)
    {
        if (labelCanvas != null) labelCanvas.SetActive(activo);
    }

    void SetColliderActivo(bool activo)
    {
        if (doorCollider != null) doorCollider.enabled = activo;
    }

    void ActualizarTextoLabel()
    {
        if (labelText == null) return;
        labelText.text = estaAbierta ? textoAbierta : textoCerrada;
    }

    // ─── Gizmo de depuración ───────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (doorPivot != null)
            Gizmos.DrawWireSphere(doorPivot.position, 0.1f);
    }
}
