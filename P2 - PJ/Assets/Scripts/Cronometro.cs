using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// ============================================================
///  Cronometro.cs  —  Versión juego de robo (1 nivel, 1 ranking)
/// ============================================================
///
///  SETUP
///  ─────────────────────────────────────────────────────────
///  1. Añade este script a cualquier GameObject persistente
///     (o al mismo Canvas del HUD).
///  2. En el Inspector asigna:
///       • textoTiempo       → TMP_Text del HUD (tiempo en curso)
///       • textoMenuWin      → TMP_Text dentro del panel de victoria
///                             que muestra el tiempo final
///       • panelNombre       → Panel para pedir nombre si es récord
///       • inputNombre       → TMP_InputField del panel anterior
///       • botonGuardar      → Botón "Guardar" del panel anterior
///       • panelVictoriaNormal → Panel principal de victoria (MenuWin)
///
///  FLUJO:
///   ExitDoorInteraction llama a DetenerYComprobarRecord()
///   → Si entra en Top 5 → panelNombre (pide nombre) → guarda → panelVictoriaNormal
///   → Si no entra       → panelVictoriaNormal directamente
/// ============================================================

public class Cronometro : MonoBehaviour
{
    // ─── HUD ──────────────────────────────────────────────────
    [Header("Interfaz HUD")]
    [SerializeField] private TextMeshProUGUI textoTiempo;

    // ─── PANEL VICTORIA ───────────────────────────────────────
    [Header("Panel Victoria")]
    [SerializeField] private TextMeshProUGUI textoMenuWin;
    [SerializeField] private GameObject panelVictoriaNormal;

    // ─── PANEL RÉCORD ─────────────────────────────────────────
    [Header("Panel Nuevo Récord")]
    [SerializeField] private GameObject    panelNombre;
    [SerializeField] private TMP_InputField inputNombre;
    [SerializeField] private Button        botonGuardar;

    // ─── CONSTANTES ───────────────────────────────────────────
    private const string CLAVE_RANKING = "Ranking_Robo";
    private const int    MAX_SCORES    = 5;

    // ─── ESTADO ───────────────────────────────────────────────
    private float tiempoActual  = 0f;
    private bool  estaCorriendo = false;

    // ── API pública: el tiempo para MenuWin ───────────────────
    public float TiempoFinal => tiempoActual;

    // ══════════════════════════════════════════════════════════
    void Start()
    {
        tiempoActual  = 0f;
        estaCorriendo = true;

        if (panelNombre        != null) panelNombre.SetActive(false);
        if (panelVictoriaNormal != null) panelVictoriaNormal.SetActive(false);
    }

    void Update()
    {
        if (!estaCorriendo) return;
        tiempoActual += Time.deltaTime;
        ActualizarTextoHUD();
    }

    // ══════════════════════════════════════════════════════════
    //  API PÚBLICA — llamada desde ExitDoorInteraction
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Para el cronómetro y comprueba si el tiempo es Top 5.
    /// Abre el panel de nombre si es récord, o muestra victoria directamente.
    /// </summary>
    public void DetenerYComprobarRecord()
    {
        if (!estaCorriendo) return;
        estaCorriendo = false;

        // Actualizar el texto del tiempo final en el panel de victoria
        if (textoMenuWin != null)
            textoMenuWin.text = FormatearTiempo(tiempoActual);

        List<ScoreEntry> scores = CargarScores();

        bool esRecord = scores.Count < MAX_SCORES
                     || tiempoActual < scores[scores.Count - 1].tiempo;

        if (esRecord)
            AbrirPanelNombre();
        else
            MostrarVictoriaNormal();
    }

    // ══════════════════════════════════════════════════════════
    //  INTERNOS
    // ══════════════════════════════════════════════════════════
    private void ActualizarTextoHUD()
    {
        if (textoTiempo != null)
            textoTiempo.text = FormatearTiempo(tiempoActual);
    }

    private void AbrirPanelNombre()
    {
        if (panelNombre == null) { MostrarVictoriaNormal(); return; }

        panelNombre.SetActive(true);

        if (inputNombre != null) inputNombre.text = "";

        if (botonGuardar != null)
        {
            botonGuardar.onClick.RemoveAllListeners();
            botonGuardar.onClick.AddListener(GuardarRecordFinal);
        }
    }

    private void GuardarRecordFinal()
    {
        string nombre = (inputNombre != null && !string.IsNullOrWhiteSpace(inputNombre.text))
                        ? inputNombre.text.Trim()
                        : "Anonimo";

        List<ScoreEntry> scores = CargarScores();
        scores.Add(new ScoreEntry { nombre = nombre, tiempo = tiempoActual });
        scores.Sort((a, b) => a.tiempo.CompareTo(b.tiempo));
        if (scores.Count > MAX_SCORES) scores.RemoveRange(MAX_SCORES, scores.Count - MAX_SCORES);

        GuardarScores(scores);

        if (panelNombre != null) panelNombre.SetActive(false);
        MostrarVictoriaNormal();
    }

    private void MostrarVictoriaNormal()
    {
        if (panelVictoriaNormal != null) panelVictoriaNormal.SetActive(true);
    }

    // ── Persistencia ──────────────────────────────────────────
    private List<ScoreEntry> CargarScores()
    {
        var lista = new List<ScoreEntry>();
        string raw = PlayerPrefs.GetString(CLAVE_RANKING, "");
        if (string.IsNullOrEmpty(raw)) return lista;

        foreach (string entrada in raw.Split('|'))
        {
            string[] p = entrada.Split(':');
            if (p.Length == 2 && float.TryParse(p[1], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float t))
            {
                lista.Add(new ScoreEntry { nombre = p[0], tiempo = t });
            }
        }
        return lista;
    }

    private void GuardarScores(List<ScoreEntry> scores)
    {
        var partes = new List<string>();
        foreach (var s in scores)
            partes.Add($"{s.nombre}:{s.tiempo.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        PlayerPrefs.SetString(CLAVE_RANKING, string.Join("|", partes));
        PlayerPrefs.Save();
    }

    private string FormatearTiempo(float t)
    {
        int min  = Mathf.FloorToInt(t / 60f);
        int sec  = Mathf.FloorToInt(t % 60f);
        int ms   = Mathf.FloorToInt((t * 100f) % 100f);
        return string.Format("{0:00}:{1:00}:{2:00}", min, sec, ms);
    }

    private class ScoreEntry
    {
        public string nombre;
        public float  tiempo;
    }
}
