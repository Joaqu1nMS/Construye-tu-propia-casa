using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuPrincipal : MonoBehaviour
{
    // Jugar
    [SerializeField] private Button botonJugar;
    [SerializeField] private AnimacionJugar animJugar;
    private bool toggleJugar = false;
    // Opciones
    [SerializeField] private Button botonOpciones;
    [SerializeField] private GameObject canvasOpciones;

    //Ranking
    [SerializeField] private GameObject canvasRanking;
    // Salir
    [SerializeField] private Button botonSalir;

    // Start is called before the first frame update
    void Start()
    {
        botonJugar.onClick.AddListener(() => ToggleJugar());

        botonOpciones.onClick.AddListener(() => MostrarOpciones());

        botonSalir.onClick.AddListener(() => Salir());
    }

    private void MostrarOpciones()
    {
        GameManager.gameM?.BotonPresionadoSFX();
        canvasOpciones.SetActive(true);
    }

    private void ToggleJugar()
    {
        botonJugar.gameObject.SetActive(false);
        botonOpciones.gameObject.SetActive(false);
        botonSalir.gameObject.SetActive(false);

        GameManager.gameM?.BotonPresionadoSFX();
        toggleJugar = !toggleJugar;
        animJugar.LanzarAnimacion();
    }

    private void MostrarRankingIndividual(GameObject columnaActiva)
    {
        GameManager.gameM?.BotonPresionadoSFX();

        // 2. Encendemos únicamente la que pasamos por parámetro
        if (columnaActiva != null)
        {
            columnaActiva.SetActive(true);
        }

        // 3. Abrimos el canvas de Ranking
        canvasRanking.SetActive(true);
    }

    public void Salir()
    {
        Application.Quit();
    }
}
