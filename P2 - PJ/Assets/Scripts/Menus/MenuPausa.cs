using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject menuOpciones;
    [SerializeField] private bool opcionesActivo = false;
    private bool menuActivo = false;
    [SerializeField] private Button botonReanudar;
    [SerializeField] private Button botonOpciones;
    [SerializeField] private Button botonMenuPrincipal;

    void Start()
    {
        botonReanudar.onClick.AddListener(() => Reanudar());
        botonOpciones.onClick.AddListener(() => ActivarOpciones());
        botonMenuPrincipal.onClick.AddListener(() => MenuPrincipal());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !GameManager.gameM.isGameOver)
        {
            Debug.Log("PAUSE");
            ToggleMenu();
            TogglePausar();
            menuOpciones.SetActive(false);
            menuOpciones.GetComponent<MenuOpciones>().CerrarControles();
            
            GameManager.gameM?.TogglePause();
        }
    }

    private void ToggleMenu()
    {
        menuActivo = !menuActivo;
        menu.SetActive(menuActivo);
    }

    private void TogglePausar()
    {        
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
        } else
        {
            Time.timeScale = 0;
        }        
    }

    private void Reanudar()
    {
        GameManager.gameM?.BotonPresionadoSFX();
        ToggleMenu();
        Time.timeScale = 1;
        GameManager.gameM?.TogglePause();
    }

    private void ActivarOpciones()
    {
        GameManager.gameM?.BotonPresionadoSFX();
        menuOpciones.SetActive(true);
    }

    private void MenuPrincipal()
    {
        GameManager.gameM?.BotonPresionadoSFX();
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
        GameManager.gameM.CambiarCancion(0);
    }
}
