using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// ============================================================
///  MenuWin.cs  —  Panel de victoria (asignado en Cronometro)
/// ============================================================
///
///  SETUP
///  ─────────────────────────────────────────────────────────
///  1. Crea un Panel UI (Screen Space Overlay), fondo negro,
///     DESACTIVADO al inicio.
///  2. Añade este script al Panel.
///  3. Asigna en el Inspector:
///       • botonReintentar    → vuelve a jugar
///       • botonMenuPrincipal → va a la escena 0
///       • textoContador      → "Objetos robados: X / Y"
///  4. Asigna este Panel al campo "panelVictoriaNormal"
///     del Cronometro.
///
///  El textoTiempo lo maneja el propio Cronometro mediante
///  su campo "textoMenuWin", así no necesitamos duplicarlo aquí.
/// ============================================================

public class MenuWin : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button botonReintentar;
    [SerializeField] private Button botonMenuPrincipal;

    [Header("Estadísticas")]
    [Tooltip("Muestra cuántos objetos se robaron sobre el total.")]
    [SerializeField] private TextMeshProUGUI textoContador;

    // Auto-buscado si no se asigna
    private ObjectManager objectManager;

    // ══════════════════════════════════════════════════════════
    void Start()
    {
        if (botonReintentar    != null) botonReintentar.onClick.AddListener(Reintentar);
        if (botonMenuPrincipal != null) botonMenuPrincipal.onClick.AddListener(IrAlMenu);
    }

    // OnEnable → estadísticas frescas cada vez que el panel se activa
    void OnEnable()
    {
        if (objectManager == null)
            objectManager = FindObjectOfType<ObjectManager>();

        if (textoContador != null && objectManager != null)
        {
            int robados = objectManager.ObjetosRecogidos();
            int total   = objectManager.ObjetosTotal();
            textoContador.text = $"Objetos robados: {robados} / {total}";
        }

        Time.timeScale = 0f;

        // Buscamos al jugador para bloquearlo y liberar el ratón
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.isBlocked = true; // Bloquea el movimiento en Update()
            player.UnlockCursor();   // Libera el ratón para pulsar botones o escribir
        }
    }

    // ══════════════════════════════════════════════════════════
    private void Reintentar()
    {
        if (GameManager.gameM != null)
        {
            GameManager.gameM.BotonPresionadoSFX();
            GameManager.gameM.isGameOver = false;
            GameManager.gameM.ReiniciarCancion();
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        SceneManager.LoadScene(0);
    }
}
