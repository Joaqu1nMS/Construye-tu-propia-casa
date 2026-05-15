using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPicker : MonoBehaviour
{
    private Outline lineado;
    [SerializeField] private GameObject player;
    [SerializeField] private float alcanceRecogida = 2f;

    // Start is called before the first frame update
    void Start()
    {
        lineado = gameObject.GetComponent<Outline>();
        lineado.enabled = false;
        player = GameObject.FindObjectOfType<PlayerController>().gameObject;
    }

    // Update is called once per frame
    private void OnMouseOver()
    {        
        if (Vector3.Distance(transform.position, player.transform.position) <= alcanceRecogida)
        {
            lineado.enabled = true;
            if (Input.GetKey(KeyCode.E))
            {
                Debug.Log("ObjetoRecogido");
                Destroy(this.gameObject);
            }    
        } else
        {
            lineado.enabled = false;
        }
        
    }

    private void OnMouseExit()
    {        
        lineado.enabled = false;            
    }
}
