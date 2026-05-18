using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// ============================================================
///  ObjectManager.cs  —  Modificado para soportar ExitDoor
/// ============================================================
///  Cambios respecto al original:
///    • Lleva la cuenta de objetos recogidos (objetosRecogidos).
///    • Expone TodosRecogidos() → bool
///    • Expone ObjetosRecogidos() → int
///    • Expone ObjetosTotal() → int
///    • HeSidoRecogido() incrementa el contador interno.
/// ============================================================

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

    // ─── Contador interno ──────────────────────────────────────
    private int objetosRecogidos = 0;   // cuántos se han cogido ya
    private int objetosTotal = 0;   // cuántos había que coger

    // ══════════════════════════════════════════════════════════
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

    // ══════════════════════════════════════════════════════════
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

        // Desactivar los que sobran
        if (cantidadActivos != -1)
        {
            for (int i = cantidadActivos; i < pickeables.Count; i++)
            {
                ObjectPicker op = pickeables[i].GetComponent<ObjectPicker>();
                Outline ou = pickeables[i].GetComponent<Outline>();
                if (op != null) { op.enabled = false; }
                if (ou != null) { ou.enabled = false; }
            }
        }

        // Crear UI y contar total
        var activos = pickeables.Where(x => x.GetComponent<ObjectPicker>().enabled).ToList();
        objetosTotal = activos.Count;

        foreach (GameObject activo in activos)
        {
            GameObject txt = Instantiate(prefabTexto, Vector2.zero, Quaternion.identity, notas.transform);
            txt.GetComponent<TextMeshProUGUI>().text = activo.GetComponent<ObjectPicker>().nombreDescripcion;
            objetosTXT.Add(txt);
        }
    }

    // ══════════════════════════════════════════════════════════
    //  Llamado por ObjectPicker cuando se recoge un objeto
    // ══════════════════════════════════════════════════════════
    public void HeSidoRecogido(string text)
    {
        GameObject obj = objetosTXT.FirstOrDefault(
            x => x.GetComponent<TextMeshProUGUI>().text.Equals(text));

        if (obj != null)
        {
            obj.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Strikethrough;
            objetosRecogidos++;   // ← NUEVO: incrementar contador
        }
    }

    // ══════════════════════════════════════════════════════════
    //  API pública para ExitDoorInteraction y MenuWin
    // ══════════════════════════════════════════════════════════

    /// <summary>Devuelve true cuando el jugador ha recogido todos los objetos requeridos.</summary>
    public bool TodosRecogidos() => objetosRecogidos >= objetosTotal && objetosTotal > 0;

    /// <summary>Cuántos objetos ha recogido el jugador hasta ahora.</summary>
    public int ObjetosRecogidos() => objetosRecogidos;

    /// <summary>Total de objetos que hay que recoger en esta partida.</summary>
    public int ObjetosTotal() => objetosTotal;
}
