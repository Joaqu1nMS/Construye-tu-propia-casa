using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TipoOBjeto
{
    LibrosSalon,
    LamparasDePie,    
    Vinilos,
    Macetas,
    Altavoces,
    Unico
}

public class ObjectPicker : MonoBehaviour
{
    private Outline lineado;
    [SerializeField] private GameObject player;
    [SerializeField] private float alcanceRecogida = 2f;
    public string nombreDescripcion;
    public TipoOBjeto tipo;

    private MinijuegoRecogida minijuegoRecogida;

    // Start is called before the first frame update
    void Start()
    {
        lineado = gameObject.GetComponent<Outline>();
        lineado.enabled = false;
        player = GameObject.FindObjectOfType<PlayerController>().gameObject;
        if (nombreDescripcion.Equals(""))
        {
            nombreDescripcion = gameObject.name;
        }
        minijuegoRecogida = FindObjectOfType<MinijuegoRecogida>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return; 
        
    }

    // Update is called once per frame
    private void OnMouseOver()
    {        
        if (!enabled) return;
        if (Vector3.Distance(transform.position, player.transform.position) <= alcanceRecogida)
        {
            lineado.enabled = true;
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!minijuegoRecogida.juegoAbierto)
                {
                    minijuegoRecogida.IniciarMinijuego(); 
                    player.GetComponent<PlayerController>().isBlocked = true;   
                } else
                {
                    if (minijuegoRecogida.HeGanado())
                    {
                        Debug.Log("ObjetoRecogido");
                        FindObjectOfType<ObjectManager>().HeSidoRecogido(nombreDescripcion);
                        Destroy(this.gameObject);
                    } else
                    {
                        // REPRODUCIR SONIDO
                        Debug.Log("CAGASTE");
                    }
                    player.GetComponent<PlayerController>().isBlocked = false;
                }
            }    
        } else
        {
            lineado.enabled = false;
        }
        
    }

    private void OnMouseExit()
    {        
        if (!enabled) return;
        lineado.enabled = false;            
    }
}
