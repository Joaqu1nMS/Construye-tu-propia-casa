using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    [SerializeField] private RawImage imagen;
    // Start is called before the first frame update
    void Start()
    {       
        if (imagen == null)
        {
            imagen = GetComponent<RawImage>();
        } 
        SetOut();
    }

    public void SetIn()
    {
        imagen.color = new Color(imagen.color.r, imagen.color.g, imagen.color.b, 1);        
    }

    public void SetOut()
    {
        imagen.color = new Color(imagen.color.r, imagen.color.g, imagen.color.b, 0); 
    }

    public IEnumerator FadeIn(float duracion)
    {
        imagen.color = new Color(imagen.color.r, imagen.color.g, imagen.color.b, 0);        

        float t = 0.0f;        
        Color c = imagen.color;
        while (t < duracion)
        {
            t += Time.deltaTime;
            c.a = t / duracion;
            imagen.color = c;
            yield return null;
        }
        imagen.color = new Color(imagen.color.r, imagen.color.g, imagen.color.b, 1);        
    }

    public IEnumerator FadeOut(float duracion)
    {
        imagen.color = new Color(imagen.color.r, imagen.color.g, imagen.color.b, 1);        

        float t = 0.0f;        
        Color c = imagen.color;
        while (t < duracion)
        {
            t += Time.deltaTime;
            c.a = 1 - t / duracion;
            imagen.color = c;
            yield return null;
        }
        imagen.color = new Color(imagen.color.r, imagen.color.g, imagen.color.b, 0);  
    }

    public IEnumerator FadeInAndOut(float d)
    {
        yield return StartCoroutine(FadeIn(d));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeOut(d));
    }    
}
