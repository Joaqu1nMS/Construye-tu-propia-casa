using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;


[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Outline))]
public class ExitDoorInteraction : MonoBehaviour
{
    [Header("Managers")]
    public ObjectManager objectManager;
    public Cronometro cronometro;
    public GameObject menuWin;

    [Header("Label UI (World Space Canvas)")]
    public GameObject labelCanvas;
    public TMP_Text labelText;

    [Header("Fade a negro")]
    [Tooltip("Image UI de pantalla completa. Color negro, alpha 0, desactivada al inicio.")]    
    public float duracionFade = 1.5f;

    [Header("Textos del label")]
    public string textoListo = "ESCAPAR  [E]";
    public string textoFaltaObjetos = "Necesitas todos los objetos";

    private Outline outline;
    private bool estaAnimando = false;
    private bool mouseEncima = false;

    //Rango
    public float rango = 3f;
    public GameObject player;

    void Awake()
    {
        outline = GetComponent<Outline>();
        menuWin.SetActive(false);

        if (objectManager == null) objectManager = FindObjectOfType<ObjectManager>();
        if (cronometro == null) cronometro = FindObjectOfType<Cronometro>();

        SetOutlineActivo(false);
        SetLabelActivo(false);
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

        if (Vector3.Distance(player.transform.position, transform.position) > rango)
        {
            // Si el jugador está fuera de rango, no mostrar UI ni permitir interacción
            SetLabelActivo(false);
            SetOutlineActivo(false);
            return;
        }else
        {
            mouseEncima = true;
            ActualizarTexto();            
            SetLabelActivo(true);
            SetOutlineActivo(true);
        }

        if (Input.GetKeyDown(KeyCode.E) && objectManager != null && objectManager.TodosRecogidos())
        {
            SetLabelActivo(false);
            SetOutlineActivo(false);
            StartCoroutine(SecuenciaVictoria());
        }
    }

    IEnumerator SecuenciaVictoria()
    {
        estaAnimando = true;
        FindObjectOfType<EnemyFSM>().GetComponents<CapsuleCollider>().FirstOrDefault(x => x.isTrigger).enabled = false;
        FindObjectOfType<PlayerController>().isBlocked = true;
        // Pausar y dejar que Cronometro gestione el ranking y el panel

        if (cronometro != null)
            cronometro.DetenerYComprobarRecord();

        // Fade a negro
        yield return StartCoroutine(GameManager.gameM.fade.FadeIn(duracionFade));
        GameManager.gameM.fade.SetOut();
        menuWin.SetActive(true);
        
        estaAnimando = false;
    }
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
