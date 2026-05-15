using System.Linq;
using UnityEngine;

public class VentanaAnimada : MonoBehaviour
{
    [Header("Cristal")]
    [SerializeField] private Renderer glassRenderer;

    [SerializeField] private Material lightOnMaterial;
    [SerializeField] private Material lightOffMaterial;

    [Header("Luz hija")]
    [SerializeField] private Light roomLight;

    [SerializeField] private bool isOn;
    [SerializeField] private int luzIndex = 0;

    private void Start()
    {        
        ApplyState();
    }

    public void TurnOn()
    {
        isOn = true;
        ApplyState();
    }

    public void TurnOff()
    {
        isOn = false;
        ApplyState();
    }

    public void Toggle()
    {
        isOn = !isOn;
        ApplyState();
    }

    private void ApplyState()
    {
        // Cambiar material
        if (glassRenderer != null)
        {
            Material[] mats = glassRenderer.materials; // copia del array
            mats[luzIndex] = isOn ? lightOnMaterial : lightOffMaterial;
            glassRenderer.materials = mats; // reasignar el array modificado
        }

        // Encender/apagar luz
        if (roomLight != null)
        {
            roomLight.enabled = isOn;
        }
    }
}