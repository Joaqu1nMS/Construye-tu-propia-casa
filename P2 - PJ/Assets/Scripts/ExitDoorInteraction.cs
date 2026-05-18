using System.Collections;
using UnityEngine;
using TMPro;

/// ============================================================
///  ExitDoorInteraction.cs  —  Puerta de salida / victoria
/// ============================================================
///
///  SETUP
///  ─────────────────────────────────────────────────────────
///  1. Añade este script al GameObject de la puerta principal.
///  2. Requiere Collider (no trigger) y Outline en el mismo objeto.
///  3. Canvas World Space con TMP_Text hijo:
///       → labelCanvas / labelText
///  4. En el Inspector asigna:
///       • objectManager → ObjectManager de la escena
///       • cronometro    → Cronometro de la escena
///       • fadeImage     → Image UI pantalla completa, negro, alpha 0,
///                         DESACTIVADA al inicio
///
///  FLUJO:
///   Cursor encima → label según objetos recogidos
///   Pulsa E (con todos los objetos) → fade negro
///   → llama a cronometro.DetenerYComprobarRecord()
///   → Cronometro abre el panel correcto (récord o victoria normal)
/// ============================================================

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Outline))]
public class ExitDoorInteraction : MonoBehaviour
{
    // ─── REFERENCIAS ──────────────────────────────────────────
    [Header("Managers")]
    public ObjectManager objectManager;
    public Cronometro cronometro;

    // ─── LABEL ────────────────────────────────────────────────
    [Header("Label UI (World Space Canvas)")]
    public GameObject labelCanvas;
    public TMP_Text labelText;

    // ─── FADE ─────────────────────────────────────────────────
    [Header("Fade a negro")]
    [Tooltip("Image UI de pantalla completa. Color negro, alpha 0, desactivada al inicio.")]
    public UnityEngine.UI.Image fadeImage;
    public float duracionFade = 1.5f;

    // ─── TEXTOS ───────────────────────────────────────────────
    [Header("Textos del label")]
    public string textoListo = "ESCAPAR  [E]";
    public string textoFaltaObjetos = "Necesitas todos los objetos";

    // ─── ESTADO ───────────────────────────────────────────────
    private Outline outline;
    private bool estaAnimando = false;
    private bool mouseEncima = false;

    // ══════════════════════════════════════════════════════════
    void Awake()
    {
        outline = GetComponent<Outline>();

        if (objectManager == null) objectManager = FindObjectOfType<ObjectManager>();
        if (cronometro == null) cronometro = FindObjectOfType<Cronometro>();

        SetOutlineActivo(false);
        SetLabelActivo(false);

        if (fadeImage != null)
        {
            Color c = fadeImage.color; c.a = 0f; fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    // ══════════════════════════════════════════════════════════
    //  OnMouse*
    // ══════════════════════════════════════════════════════════
    void OnMouseEnter()
    {
        if (Time.timeScale == 0f || estaAnimando) return;
        mouseEncima = true;
        ActualizarTexto();
        SetLabelActivo(true);
        SetOutlineActivo(true);
    }

    void OnMouseExit()
    {
        mouseEncima = false;
        SetLabelActivo(false);
        SetOutlineActivo(false);
    }

    void OnMouseOver()
    {
        if (Time.timeScale == 0f || estaAnimando) return;

        if (Input.GetKeyDown(KeyCode.E) && objectManager != null && objectManager.TodosRecogidos())
        {
            SetLabelActivo(false);
            SetOutlineActivo(false);
            StartCoroutine(SecuenciaVictoria());
        }
    }

    // ══════════════════════════════════════════════════════════
    //  COROUTINE: fade negro → delegar en Cronometro
    // ══════════════════════════════════════════════════════════
    IEnumerator SecuenciaVictoria()
    {
        estaAnimando = true;

        // Fade a negro
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float t = 0f;
            while (t < duracionFade)
            {
                t += Time.deltaTime;
                Color c = fadeImage.color;
                c.a = Mathf.Clamp01(t / duracionFade);
                fadeImage.color = c;
                yield return null;
            }
            Color final = fadeImage.color; final.a = 1f; fadeImage.color = final;
        }
        else
        {
            yield return new WaitForSeconds(duracionFade);
        }

        // Pausar y dejar que Cronometro gestione el ranking y el panel
        Time.timeScale = 0f;

        if (cronometro != null)
            cronometro.DetenerYComprobarRecord();

        estaAnimando = false;
    }

    // ══════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════
    void ActualizarTexto()
    {
        if (labelText == null) return;
        bool listo = objectManager != null && objectManager.TodosRecogidos();
        labelText.text = listo ? textoListo : textoFaltaObjetos;
    }

    void SetOutlineActivo(bool activo) { if (outline != null) outline.enabled = activo; }
    void SetLabelActivo(bool activo) { if (labelCanvas != null) labelCanvas.SetActive(activo); }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.15f);
    }
}
