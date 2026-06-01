using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class MenuWin : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button botonReintentar;
    [SerializeField] private Button botonMenuPrincipal;

    [Header("Estadisticas")]
    [Tooltip("Muestra cuantos objetos se robaron sobre el total.")]
    [SerializeField] private TextMeshProUGUI textoContador;

    private ObjectManager objectManager;

    void Start()
    {
        if (botonReintentar != null) botonReintentar.onClick.AddListener(Reintentar);
        if (botonMenuPrincipal != null) botonMenuPrincipal.onClick.AddListener(IrAlMenu);
    }
    void OnEnable()
    {
        if (objectManager == null)
            objectManager = FindObjectOfType<ObjectManager>();

        if (textoContador != null && objectManager != null)
        {
            int robados = objectManager.ObjetosRecogidos();
            int total = objectManager.ObjetosTotal();
            textoContador.text = $"Objetos robados: {robados} / {total}";
        }

        Time.timeScale = 0f;

        // Buscamos al jugador para bloquearlo y liberar el raton
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.isBlocked = true; // Bloquea el movimiento en Update()
            player.UnlockCursor();   // Libera el raton para pulsar botones o escribir
            player.radioCrosshair = 0f;
        }
    }

    private void Reintentar()
    {
        if (GameManager.gameM != null)
        {
            GameManager.gameM.BotonPresionadoSFX();
            GameManager.gameM.isGameOver = false;
            GameManager.gameM.ReiniciarCancion();
        }
        Time.timeScale = 1f;
        StartCoroutine(GameManager.gameM.CambiarEscena(SceneManager.GetActiveScene().buildIndex, 1f));
    }

    private void IrAlMenu()
    {
        if (GameManager.gameM != null)
        {
            GameManager.gameM.BotonPresionadoSFX();
            GameManager.gameM.isGameOver = false;
            GameManager.gameM.CambiarCancion(0);
        }
        Time.timeScale = 1f;
        StartCoroutine(GameManager.gameM.CambiarEscena(0, 1f));
    }
}
