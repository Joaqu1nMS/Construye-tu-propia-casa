using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorMinijuegoRecogida : MonoBehaviour
{
    [SerializeField] private RectTransform rTransformBarra;
    [SerializeField] private float velocidadMov = 500f;
    RectTransform mov;
    private float posIni;
    private float posFin;
    private float posActual;
    private int direccion = 1;

    // Start is called before the first frame update
    void Start()
    {        
        mov = GetComponent<RectTransform>();        

        //Debug.Log($"{rTransformBarra.gameObject.name}: {rTransformBarra.rect.width / 2} | {mov.rect.width / 2f}");
        Vector2 pos = mov.anchoredPosition;

        posIni = -rTransformBarra.rect.width / 2 + mov.rect.width / 2f;
        posActual = posIni;
        posFin = rTransformBarra.rect.width / 2 - mov.rect.width / 2f;

        pos.x = posIni;
        mov.anchoredPosition = pos;
    }

    // Update is called once per frame
    void Update()
    {
        posActual += Time.deltaTime * velocidadMov * direccion;

        if (posActual >= posFin)
        {
            posActual = posFin;
            direccion = -1;
        }
        else if (posActual <= posIni)
        {
            posActual = posIni;
            direccion = 1;
        }

        Vector2 pos = mov.anchoredPosition;
        pos.x = posActual;
        mov.anchoredPosition = pos;
    }

    public void ResetPosicion()
    {
        posActual = 0;
        direccion = 1;
    }
}
