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

public partial class ObjectPicker : MonoBehaviour
{
    private Outline lineado;
    [SerializeField] private GameObject player;
    [SerializeField] private float alcanceRecogida = 2f;    
    public string nombreDescripcion;
    public TipoOBjeto tipo;
    public AudioClip golpeObjeto;
    public AudioClip pickObjeto;

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
                        // Acierta - objeto recogido
                        GameManager.gameM.ReproducirSonido(null, pickObjeto, 0.5f);
                        FindObjectOfType<ObjectManager>().HeSidoRecogido(nombreDescripcion);
                        Destroy(this.gameObject);
                    } else
                    {
                        // Fallo - hace ruido
                        GameManager.gameM.ReproducirSonido(null, golpeObjeto, 0.5f);
                        FindObjectOfType<EnemyAIController>().NotifyLoudNoise(player.transform.position);                        
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

# if UNITY_EDITOR
public partial class ObjectPicker
{
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, alcanceRecogida);
    }
}
# endif
