using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinijuegoRecogida : MonoBehaviour
{
    [SerializeField] private RectTransform rTransformBarra;
    [SerializeField] private RectTransform zonaValida;    
    [SerializeField] private RectTransform cursor;

    public bool juegoAbierto = false;

    // Start is called before the first frame update
    void Start()
    {
        CerrarMinijuego();
    }

    public void IniciarMinijuego()
    {
        juegoAbierto = true;
        rTransformBarra.gameObject.SetActive(true);
        zonaValida.gameObject.SetActive(true);
        cursor.gameObject.SetActive(true);

        cursor.gameObject.GetComponent<CursorMinijuegoRecogida>().ResetPosicion();

        // Tamaño de zona valida
        Vector2 size = zonaValida.sizeDelta;
        size.x = Random.Range(50f, 200f);
        zonaValida.sizeDelta = size;

        // Posicion de zona valida
        float posIni = -rTransformBarra.rect.width / 2 + zonaValida.rect.width / 2f;        
        float posFin = rTransformBarra.rect.width / 2 - zonaValida.rect.width / 2f;
        Vector2 pos = zonaValida.anchoredPosition;
        pos.x = Random.Range(posIni, posFin);
        zonaValida.anchoredPosition = pos;
    }

    public void CerrarMinijuego()
    {
        juegoAbierto = false;
        rTransformBarra.gameObject.SetActive(false);
        zonaValida.gameObject.SetActive(false);
        cursor.gameObject.SetActive(false);
    }

    public bool HeGanado()
    {
        float cursorX = cursor.anchoredPosition.x;

        float zonaX = zonaValida.anchoredPosition.x;
        float zonaWidth = zonaValida.rect.width;

        float zonaMin = zonaX - zonaWidth / 2f;
        float zonaMax = zonaX + zonaWidth / 2f;

        CerrarMinijuego();

        return cursorX >= zonaMin && cursorX <= zonaMax;
    }
}
