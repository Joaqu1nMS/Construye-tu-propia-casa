using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class AnimacionJugar : MonoBehaviour
{
    [SerializeField] GameObject salon;
    [SerializeField] GameObject cuarto;
    [SerializeField] GameObject camara;
    [SerializeField] GameObject puertaPrincipal;    
    [SerializeField] Vector3 offsetDestino;
    [SerializeField] private float duracion;
    public AudioClip switchLight;

    public void LanzarAnimacion()
    {
        StartCoroutine(MoverCamara(duracion + 1));
        StartCoroutine(Animacion());
    }

    private IEnumerator MoverCamara(float tiempo)
    {
        Vector3 inicio = camara.transform.position;
        Vector3 destino = puertaPrincipal.transform.position + offsetDestino;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / tiempo; // controla la duración total
            camara.transform.position = Vector3.Lerp(inicio, destino, t);
            yield return null;
        }

        camara.transform.position = destino; // asegurar posición final exacta
        
    }

    private void CargarJuego()
    {
        SceneManager.LoadScene(1);        
    }

    private IEnumerator Animacion()
    {
        yield return new WaitForSeconds(duracion * 1 / 5);
        GameManager.gameM.ReproducirSonido(null, switchLight, 0.5f);
        foreach (VentanaAnimada v in cuarto.GetComponentsInChildren<VentanaAnimada>())
        {
            v.TurnOn();
        }

        yield return new WaitForSeconds(duracion * 2 / 5);
        GameManager.gameM.ReproducirSonido(null, switchLight, 0.5f);
        foreach (VentanaAnimada v in salon.GetComponentsInChildren<VentanaAnimada>())
        {
            v.TurnOff();
        }

        yield return new WaitForSeconds(duracion * 2 / 5);
        GameManager.gameM.ReproducirSonido(null, switchLight, 0.5f);
        foreach (VentanaAnimada v in cuarto.GetComponentsInChildren<VentanaAnimada>())
        {
            v.TurnOff();
        }
        
        StartCoroutine(GameManager.gameM.CambiarEscena(1, 1f));
    }
}
