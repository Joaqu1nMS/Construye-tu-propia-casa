using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuOpciones : MonoBehaviour
{
    [SerializeField] private GameObject panelOpciones;
    [SerializeField] private Button volverOpc;
    [SerializeField] private Slider volumen;
    [SerializeField] private Slider efectos;
    [SerializeField] private TMP_Dropdown dropdownResolucion;
    [SerializeField] private TMP_Dropdown dropdownPantalla;
    [SerializeField] private Button controles;
    private Resolution[] resoluciones;

    [SerializeField] private GameObject panelControles;
    [SerializeField] private Button volverControles;

    // Start is called before the first frame update
    void Start()
    {
        volverOpc.onClick.AddListener(() => VolverOpciones());        
        volumen.onValueChanged.AddListener(delegate { Volumen(); });
        efectos.onValueChanged.AddListener(delegate { VolumenSFX(); });
        controles.onClick.AddListener(() => AbrirControles());

        volverControles.onClick.AddListener(() => CerrarControles());

        ConfigurarResolucion();
        ConfigurarModoPantalla();

        CargarAjustesGuardados();
    }

    void OnEnable()
    {
        if (GameManager.gameM != null)
        {
            volumen.value = GameManager.gameM.music.volume;
            efectos.value = GameManager.gameM.SFX.volume;
        }
    }

    private void VolverOpciones()
    {
        GameManager.gameM?.BotonPresionadoSFX();
        gameObject.SetActive(false);
    }

    private void Volumen()
    {        
        GameManager.gameM.music.volume = volumen.value;
        PlayerPrefs.SetFloat("VolumenMusica", volumen.value);
    }

    private void VolumenSFX()
    {        
        GameManager.gameM.SFX.volume = efectos.value;
        PlayerPrefs.SetFloat("VolumenSFX", efectos.value);
    }

    private void ConfigurarResolucion()
    {
        resoluciones = Screen.resolutions;
        dropdownResolucion.ClearOptions();

        List<string> opciones = new List<string>();
        int resolucionActual = 0;

        for (int i = 0; i < resoluciones.Length; i++)
        {
            string opcion = resoluciones[i].width + " x " + resoluciones[i].height;
            opciones.Add(opcion);

            if (resoluciones[i].width == Screen.currentResolution.width &&
                resoluciones[i].height == Screen.currentResolution.height)
            {
                resolucionActual = i;
            }
        }

        dropdownResolucion.AddOptions(opciones);
        int resolucionGuardada = PlayerPrefs.GetInt("Resolucion", resolucionActual);
        if (resolucionGuardada >= resoluciones.Length) resolucionGuardada = resolucionActual;

        dropdownResolucion.value = resolucionGuardada;
        dropdownResolucion.RefreshShownValue();

        dropdownResolucion.onValueChanged.AddListener(CambiarResolucion);
    }

    public void CambiarResolucion(int indiceResolucion)
    {
        Resolution res = resoluciones[indiceResolucion];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
        PlayerPrefs.SetInt("Resolucion", indiceResolucion);
    }

    private void ConfigurarModoPantalla()
    {
        dropdownPantalla.ClearOptions();
        List<string> modos = new List<string> { "Pantalla Completa", "Ventana sin Bordes", "Ventana" };
        dropdownPantalla.AddOptions(modos);

        dropdownPantalla.onValueChanged.AddListener(CambiarModoPantalla);
    }

    public void CambiarModoPantalla(int indice)
    {
        switch (indice)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case 2: Screen.fullScreenMode = FullScreenMode.Windowed; break;
        }
        PlayerPrefs.SetInt("ModoPantalla", indice);
    }

    private void CargarAjustesGuardados()
    {
        // 1. Cargar volumen (Si no existe, devuelve 0.5f por defecto)
        volumen.value = PlayerPrefs.GetFloat("VolumenMusica", 0.5f);
        efectos.value = PlayerPrefs.GetFloat("VolumenSFX", 0.5f);

        // 2. Cargar Modo Pantalla (Si no existe, devuelve 0 por defecto)
        int modoPantalla = PlayerPrefs.GetInt("ModoPantalla", 0);
        dropdownPantalla.value = modoPantalla;
        CambiarModoPantalla(modoPantalla); // Lo aplicamos directamente

        // (La resolución ya se carga y aplica dentro de ConfigurarResolucion)
        
        // Forzamos un guardado manual por seguridad
        PlayerPrefs.Save(); 
    }

    private void AbrirControles()
    {
        GameManager.gameM?.BotonPresionadoSFX();
        panelOpciones.SetActive(false);
        panelControles.SetActive(true);
    }

    public void CerrarControles()
    {
        GameManager.gameM?.BotonPresionadoSFX();
        panelControles.SetActive(false);
        panelOpciones.SetActive(true);        
    }
}
