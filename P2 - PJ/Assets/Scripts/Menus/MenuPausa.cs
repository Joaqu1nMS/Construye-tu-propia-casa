using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject menuOpciones;
    private bool menuActivo = false;
    [SerializeField] private Button botonReanudar;
    [SerializeField] private Button botonOpciones;
    [SerializeField] private Button botonMenuPrincipal;

    private MenuOpciones scriptOpciones;

    void Start()
    {
        botonReanudar.onClick.AddListener(() => Reanudar());
        botonOpciones.onClick.AddListener(() => ActivarOpciones());
        botonMenuPrincipal.onClick.AddListener(() => MenuPrincipal());

        if (menuOpciones != null)
        {
            scriptOpciones = menuOpciones.GetComponent<MenuOpciones>();
        }

        // Asegurarnos de que al empezar el juego, el raton esto bloqueado y oculto
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        
        bool isGameOver = GameManager.gameM != null && GameManager.gameM.isGameOver;

        // Cambiado a KeyCode.Escape
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            if (menuActivo)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    private void Pausar()
    {
        menuActivo = true;
        if (menu != null) menu.SetActive(true);
        Time.timeScale = 0; // Congela el juego
        
        // Liberar y mostrar el raton
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameManager.gameM?.TogglePause(); // Pausa la musica
    }

    private void Reanudar()
    {
        menuActivo = false;
        if (menu != null) menu.SetActive(false);
        
        // Cerramos tambien el menu de opciones si estuviera abierto
        if (menuOpciones != null) menuOpciones.SetActive(false);
        if (scriptOpciones != null) scriptOpciones.CerrarControles();

        Time.timeScale = 1; // Descongela el juego

        // Volver a bloquear y ocultar el raton
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameManager.gameM?.BotonPresionadoSFX();
        GameManager.gameM?.TogglePause(); // Reanuda la musica
    }

    private void ActivarOpciones()
    {
        GameManager.gameM?.BotonPresionadoSFX();
        if (menuOpciones != null) menuOpciones.SetActive(true);
    }

    private void MenuPrincipal()
    {
        GameManager.gameM?.BotonPresionadoSFX();
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
        GameManager.gameM?.CambiarCancion(0);
    }
}