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
            if (Input.GetKey(KeyCode.E))
            {
                Debug.Log("ObjetoRecogido");
                FindObjectOfType<ObjectManager>().HeSidoRecogido(nombreDescripcion);
                Destroy(this.gameObject);
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
