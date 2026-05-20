using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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
    
    private int objetosRecogidos = 0;   // cuántos se han cogido ya
    private int objetosTotal = 0;   // cuántos había que coger
    
    void Start()
    {
        foreach (ObjectPicker obj in FindObjectsOfType<ObjectPicker>())
            pickeables.Add(obj.gameObject);

        canvasLista.SetActive(canvasActivo);
        SeleccionarObjetos();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            canvasActivo = !canvasActivo;
            canvasLista.SetActive(canvasActivo);
        }
    }
    
    void SeleccionarObjetos()
    {
        // Mezclar aleatoriamente
        for (int i = 0; i < pickeables.Count; i++)
        {
            int randomIndex = Random.Range(i, pickeables.Count);
            GameObject temp = pickeables[i];
            pickeables[i] = pickeables[randomIndex];
            pickeables[randomIndex] = temp;
        }

        // Seleccionar cuáles quedan activos respetando la regla de unicidad
        HashSet<TipoOBjeto> tiposYaElegidos = new HashSet<TipoOBjeto>();
        List<GameObject> seleccionados = new List<GameObject>();

        foreach (GameObject obj in pickeables)
        {
            if (cantidadActivos != -1 && seleccionados.Count >= cantidadActivos) break;

            ObjectPicker op = obj.GetComponent<ObjectPicker>();
            if (op == null) continue;

            // Si no es Unico y ya cogimos uno de ese tipo, descartarlo
            if (op.tipo != TipoOBjeto.Unico && tiposYaElegidos.Contains(op.tipo))
            {
                op.enabled = false;
                Outline ou = obj.GetComponent<Outline>();
                if (ou != null) ou.enabled = false;
                continue;
            }

            // Aceptar el objeto
            seleccionados.Add(obj);
            if (op.tipo != TipoOBjeto.Unico)
                tiposYaElegidos.Add(op.tipo);
        }

        // Desactivar los que no fueron seleccionados
        foreach (GameObject obj in pickeables)
        {
            if (seleccionados.Contains(obj)) continue;

            ObjectPicker op = obj.GetComponent<ObjectPicker>();
            Outline ou = obj.GetComponent<Outline>();
            if (op != null) op.enabled = false;
            if (ou != null) ou.enabled = false;
        }

        // Crear UI y contar total
        objetosTotal = seleccionados.Count;

        foreach (GameObject activo in seleccionados)
        {
            GameObject txt = Instantiate(prefabTexto, Vector2.zero, Quaternion.identity, notas.transform);
            txt.GetComponent<TextMeshProUGUI>().text = activo.GetComponent<ObjectPicker>().nombreDescripcion;
            objetosTXT.Add(txt);
        }
    }

    public void HeSidoRecogido(string text)
    {
        GameObject obj = objetosTXT.FirstOrDefault(
            x => x.GetComponent<TextMeshProUGUI>().text.Equals(text));

        if (obj != null)
        {
            obj.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Strikethrough;
            objetosRecogidos++;
        }
    }
    
    // HELPERS

    public bool TodosRecogidos() => objetosRecogidos >= objetosTotal && objetosTotal > 0;
    public int ObjetosRecogidos() => objetosRecogidos;    
    public int ObjetosTotal() => objetosTotal;
}
