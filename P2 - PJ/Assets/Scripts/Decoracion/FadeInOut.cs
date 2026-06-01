using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    
    // Start is called before the first frame update
    void Start()
    {       
        if (cg == null)
        {
            cg = GetComponentInParent<CanvasGroup>();
        } 
        SetOut();
    }

    public void SetIn()
    {
        cg.alpha = 1;       
    }

    public void SetOut()
    {
        cg.alpha = 0;
    }

    public IEnumerator FadeIn(float duracion)
    {
        cg.alpha = 0;

        float t = 0.0f;                
        while (t < duracion)
        {
            t += Time.deltaTime;
            cg.alpha = t / duracion;            
            yield return null;
        }
        cg.alpha = 1;       
    }

    public IEnumerator FadeOut(float duracion)
    {
        cg.alpha = 1;

        float t = 0.0f;                
        while (t < duracion)
        {
            t += Time.deltaTime;
            cg.alpha = 1 - t / duracion;
            
            yield return null;
        }
        cg.alpha = 0;
    }

    public IEnumerator FadeInAndOut(float d)
    {
        yield return StartCoroutine(FadeIn(d));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeOut(d));
    }    
}
