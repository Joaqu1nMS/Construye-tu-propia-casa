using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Cronometro : MonoBehaviour
{
    [Header("Interfaz Cronómetro")]
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private TextMeshProUGUI textoMenuWin;

    [Header("Panel Nuevo Récord")]
    [SerializeField] private GameObject panelNombre;
    [SerializeField] private TMP_InputField inputNombre;
    [SerializeField] private Button botonGuardar;

    // AÑADIDO: Referencia al menú de victoria normal
    [Header("Panel Victoria Normal")]
    [SerializeField] private GameObject panelVictoriaNormal;

    private float tiempoActual;
    private bool estaCorriendo = false;
    private string bossActual;

    void Start()
    {
        tiempoActual = 0f;
        estaCorriendo = true;

        if (panelNombre != null) panelNombre.SetActive(false);
        // Aseguramos que el menú de victoria empiece apagado
        if (panelVictoriaNormal != null) panelVictoriaNormal.SetActive(false);
    }

    void Update()
    {
        if (estaCorriendo)
        {
            tiempoActual += Time.deltaTime;
            ActualizarTexto();
        }
    }

    private void ActualizarTexto()
    {
        int minutos = Mathf.FloorToInt(tiempoActual / 60);
        int segundos = Mathf.FloorToInt(tiempoActual % 60);
        int milisegundos = Mathf.FloorToInt((tiempoActual * 100) % 100);

        textoTiempo.text = string.Format("{0:00}:{1:00}:{2:00}", minutos, segundos, milisegundos);
        textoMenuWin.text = string.Format("{0:00}:{1:00}:{2:00}", minutos, segundos, milisegundos);
    }

    public void DetenerYComprobarRecord(string nombreBoss)
    {
        if (!estaCorriendo) return;

        estaCorriendo = false;
        bossActual = nombreBoss;        

        string claveRecord = "Top5_" + nombreBoss;
        List<ScoreEntry> mejoresTiempos = CargarScores(claveRecord);

        // Comprobamos si entramos en el Top 5
        if (mejoresTiempos.Count < 5 || tiempoActual < mejoresTiempos[mejoresTiempos.Count - 1].tiempo)
        {
            // ¡Es récord! Encendemos el sub-panel para pedir el nombre
            Debug.Log("RECORD");
            AbrirPanelNombre();
        }
        else 
        {
            // NO es récord. Nos aseguramos de que el sub-panel esté apagado
            Debug.Log($"No lograste entrar al Top 5 de {nombreBoss}.");
            if (panelVictoriaNormal != null) panelVictoriaNormal.SetActive(true);
            if (panelNombre != null) panelNombre.SetActive(false);
        }
    }

    private void AbrirPanelNombre()
    {
        Debug.Log("Abriendo panel");
        panelNombre.SetActive(true);
        /*if (panelNombre != null) {
            panelNombre.SetActive(true);
        } else
        {
            Debug.Log("NO LO ABRO");
        }*/
        
        botonGuardar.onClick.RemoveAllListeners();
        botonGuardar.onClick.AddListener(() => GuardarRecordFinal());
    }

    private void GuardarRecordFinal()
    {
        string nombreJugador = string.IsNullOrEmpty(inputNombre.text) ? "Anonimo" : inputNombre.text;
        string claveRecord = "Top5_" + bossActual;

        List<ScoreEntry> mejoresTiempos = CargarScores(claveRecord);
        mejoresTiempos.Add(new ScoreEntry { nombre = nombreJugador, tiempo = tiempoActual });
        mejoresTiempos.Sort((a, b) => a.tiempo.CompareTo(b.tiempo));

        if (mejoresTiempos.Count > 5)
        {
            mejoresTiempos.RemoveRange(5, mejoresTiempos.Count - 5);
        }

        List<string> tiemposParaGuardar = new List<string>();
        foreach (ScoreEntry s in mejoresTiempos)
        {
            tiemposParaGuardar.Add($"{s.nombre}:{s.tiempo}");
        }

        string nuevosDatos = string.Join("|", tiemposParaGuardar);
        PlayerPrefs.SetString(claveRecord, nuevosDatos);
        PlayerPrefs.Save();

        // AL GUARDAR: Simplemente apagamos el sub-panel de pedir nombre.
        // El MenuWin general seguirá viéndose de fondo para que el jugador pueda elegir "Volver a jugar" o "Menu Principal".
        if (panelNombre != null) panelNombre.SetActive(false);
        if (panelVictoriaNormal != null) panelVictoriaNormal.SetActive(true);
        
        Debug.Log("¡Récord guardado con éxito!");
    }

    // AÑADIDO: Función para mostrar el menú de victoria
    private void MostrarVictoriaNormal()
    {
        if (panelVictoriaNormal != null)
        {
            panelVictoriaNormal.SetActive(true);
        }
    }

    private class ScoreEntry
    {
        public string nombre;
        public float tiempo;

    }
    private List<ScoreEntry> CargarScores(string clave)
    {
        List<ScoreEntry> lista = new List<ScoreEntry>();
        string datosGuardados = PlayerPrefs.GetString(clave, "");
        if (string.IsNullOrEmpty(datosGuardados)) return lista;
        string[] entradas = datosGuardados.Split('|');
        foreach (string entrada in entradas)
        {
            string[] partes = entrada.Split(':');
            if (partes.Length == 2)
            {
                if (float.TryParse(partes[1], out float tiempoParsed))
                {
                    lista.Add(new ScoreEntry { nombre = partes[0], tiempo = tiempoParsed });
                }
            }
        }
        return lista;
    }

    private string FormatearTiempo(float tiempo)
    {
        int minutos = Mathf.FloorToInt(tiempo / 60);
        int segundos = Mathf.FloorToInt(tiempo % 60);
        int milisegundos = Mathf.FloorToInt((tiempo * 100) % 100);
        return string.Format("{0:00}:{1:00}:{2:00}", minutos, segundos, milisegundos);
    }
}