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
    //ranking
    [SerializeField] private Button botonRanking;

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

        botonRanking.onClick.AddListener(() => MostrarRanking());

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
        botonRanking.gameObject.SetActive(false);
        botonOpciones.gameObject.SetActive(false);
        botonSalir.gameObject.SetActive(false);

        GameManager.gameM?.BotonPresionadoSFX();
        toggleJugar = !toggleJugar;
        animJugar.LanzarAnimacion();
    }

    private void MostrarRanking()
    {
        GameManager.gameM?.BotonPresionadoSFX();
        canvasRanking.SetActive(true);
    }

    public void Salir()
    {
        Application.Quit();
    }
}
