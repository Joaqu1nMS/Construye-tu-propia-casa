using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ObjectManager : MonoBehaviour
{
    List<GameObject> pickeables = new List<GameObject>();
    List<GameObject> objetosTXT = new List<GameObject>();

    [Header("Objetos robables")]
    public int cantidadActivos = 3;

    [Header("Referencias")]
    public GameObject canvasLista;
    private bool canvasActivo = false;
    public GameObject notas;
    public GameObject prefabTexto;

    void Start()
    {
        // Obtener todos los ObjectPicker
        foreach (ObjectPicker obj in FindObjectsOfType<ObjectPicker>())
        {
            pickeables.Add(obj.gameObject);
        }
        canvasLista.SetActive(canvasActivo);
        SeleccionarObjetos();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!canvasActivo) canvasActivo = true;
            else canvasActivo = false;
            canvasLista.SetActive(canvasActivo);
        }
    }

    void SeleccionarObjetos()
    {
        // Mezclar lista aleatoriamente
        for (int i = 0; i < pickeables.Count; i++)
        {
            int randomIndex = Random.Range(i, pickeables.Count);

            GameObject temp = pickeables[i];
            pickeables[i] = pickeables[randomIndex];
            pickeables[randomIndex] = temp;
        }

        // Desactivar ObjectPicker del resto
        if (cantidadActivos != -1)
        {
            for (int i = cantidadActivos; i < pickeables.Count; i++)
            {
                ObjectPicker op = pickeables[i].GetComponent<ObjectPicker>();
                Outline ou = pickeables[i].GetComponent<Outline>();

                if (op != null)
                {
                    op.enabled = false;                
                    ou.enabled = false;
                }
            }
        }        

        // Crear la UI
        foreach (GameObject activo in pickeables.Where(x => x.GetComponent<ObjectPicker>().enabled))
        {
            GameObject txt = Instantiate(prefabTexto, Vector2.zero, Quaternion.identity, notas.transform);
            txt.GetComponent<TextMeshProUGUI>().text = activo.GetComponent<ObjectPicker>().nombreDescripcion;
            objetosTXT.Add(txt);
        }
    }

    public void HeSidoRecogido(string text)
    {
        GameObject obj = objetosTXT.FirstOrDefault(x => x.GetComponent<TextMeshProUGUI>().text.Equals(text));
        if (obj != null)
        {
            obj.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Strikethrough;
        }
    }
}