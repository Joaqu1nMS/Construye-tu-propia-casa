using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Outline))]
public class DoorInteraction : MonoBehaviour
{
    [Header("Puerta")]
    [Tooltip("Transform que rota (el gozne/bisagra). Suele ser el padre de este objeto.")]
    public Transform doorPivot;

    [Tooltip("Collider de este mismo mesh. Se desactiva durante la animación.")]
    public Collider doorCollider;

    [Header("Label UI (World Space Canvas)")]
    public GameObject labelCanvas;
    public TMP_Text labelText;

    public string textoCerrada = "Presiona E para abrir";
    public string textoAbierta = "Presiona E para cerrar";

    //Rango
    public float rango = 3f;
    public GameObject player;


    [Header("Animación")]
    public float anguloCerrada = 0f;
    public float anguloAbierta = 130f;
    public float duracionAnim = 1.2f;

    [Header("Sonidos")]
    public AudioClip abrir;
    public AudioClip cerrar;
    public AudioSource audioSource;


    private Outline outline;
    private bool estaAbierta = false;
    private bool estaAnimando = false;
    private bool mouseEncima = false;

    void Awake()
    {
        player = GameObject.FindObjectOfType<PlayerController>().gameObject;
        outline = GetComponent<Outline>();
        audioSource = GetComponent<AudioSource>();

        if (doorCollider == null) doorCollider = GetComponent<Collider>();
        if (doorPivot == null) doorPivot = transform.parent;

        // Estado inicial: UI oculto, outline apagado
        SetOutlineActivo(false);
        SetLabelActivo(false);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
    }

    /// Cursor sale del Collider
    void OnMouseExit()
    {
        mouseEncima = false;
        SetLabelActivo(false);
        SetOutlineActivo(false);
    }

    /// Cursor está sobre el Collider — detecto tecla E
    void OnMouseOver()
    {
        if (Time.timeScale == 0f) return;

        if (Vector3.Distance(player.transform.position, transform.position) > rango)
        {
            // no muestro UI si el jugador está lejos, y si estaba mostrado, lo oculto
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
            {
                StartCoroutine(AnimarPuerta(anguloAbierta, anguloCerrada));
            }
            else
            {
                StartCoroutine(AnimarPuerta(anguloCerrada, anguloAbierta));
            }
        }
    }

    public IEnumerator AnimarPuerta(float anguloInicio, float anguloFin)
    {
        estaAnimando = true;

        // Desactivo hitbox durante el movimiento
        SetColliderActivo(false);

        float eulerX = doorPivot.localEulerAngles.x;
        float eulerZ = doorPivot.localEulerAngles.z;

        Quaternion rotInicio = Quaternion.Euler(eulerX, anguloInicio, eulerZ);
        Quaternion rotFin = Quaternion.Euler(eulerX, anguloFin, eulerZ);

        float t = 0f;

        if (!estaAbierta) ReproducirSonido(abrir);
        while (t < duracionAnim)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / duracionAnim);

            // EaseInOut suaviza inicio y final
            float smooth = progress * progress * (3f - 2f * progress);

            doorPivot.localRotation = Quaternion.Lerp(rotInicio, rotFin, smooth);
            if (t >= duracionAnim && estaAbierta) ReproducirSonido(cerrar);
            yield return null;
        }


        // Posicion al terminar
        doorPivot.localRotation = rotFin;

        // Cambiar estado
        estaAbierta = !estaAbierta;

        // Reactivar hitbox
        SetColliderActivo(true);

        estaAnimando = false;

        //  Si el mouse sigue encima, volver a mostrar UI
        if (mouseEncima)
        {
            ActualizarTextoLabel();
            SetLabelActivo(true);
            SetOutlineActivo(true);
        }
    }

    public void AbrePuertaVecino()
    {
        if (estaAbierta || estaAnimando)
        {
            Debug.Log("VECINO NO ABRE PUERTA");
            return;
        }

        Debug.Log("VECINO ABRE PUERTA");
        StartCoroutine(AnimarPuerta(anguloCerrada, anguloAbierta));
    }

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (doorPivot != null)
            Gizmos.DrawWireSphere(doorPivot.position, 0.1f);
    }

    private void ReproducirSonido(AudioClip sfx)
    {
        audioSource.volume = GameManager.gameM.SFX.volume;
        audioSource.pitch = 1;
        audioSource.clip = sfx;
        audioSource.Play();
    }
}
