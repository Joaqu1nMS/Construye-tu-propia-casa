using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public class MenuRanking : MonoBehaviour
{
    [SerializeField] private Button volverRanking;
    [SerializeField] private TextMeshProUGUI textoRecord;

    void Start()
    {
        // El boton solo necesita que le asignemos la funcion una vez al iniciar el juego
        volverRanking.onClick.AddListener(() => Cerrar());
    }

    void OnEnable()
    {
        // Cada vez que este panel se active, se actualizaran los textos
        ActualizarTextosRanking();
    }

    private void ActualizarTextosRanking()
    {
        textoRecord.text = ObtenerTextoTop5();
    }

    private string ObtenerTextoTop5()
    {
        string clave = "Ranking_Robo";
        string datosGuardados = PlayerPrefs.GetString(clave, "");

        if (string.IsNullOrEmpty(datosGuardados))
        {
            return "1. --:--:--\n2. --:--:--\n3. --:--:--\n4. --:--:--\n5. --:--:--\n";
        }

        string[] entradas = datosGuardados.Split('|');
        string textoFinal = "";

        for (int i = 0; i < 5; i++)
        {
            if (i >= entradas.Length)
            {
                textoFinal += $"{i + 1}. --:--:--\n";
                continue;
            }

            string[] partes = entradas[i].Split(':');
            if (partes.Length == 2)
            {
                string nombre = partes[0];                
                float tiempo = float.Parse(partes[1], CultureInfo.InvariantCulture);

                // Formato -> 1. Juan: 00:10:22
                textoFinal += $"{i + 1}. {nombre}: {FormatearTiempo(tiempo)}\n";
            }
        }

        return textoFinal;
    }

    private string FormatearTiempo(float tiempo)
    {
        int minutos = Mathf.FloorToInt(tiempo / 60);
        int segundos = Mathf.FloorToInt(tiempo % 60);
        int milisegundos = Mathf.FloorToInt((tiempo * 100) % 100);
        return string.Format("{0:00}:{1:00}:{2:00}", minutos, segundos, milisegundos);
    }

    private void Cerrar()
    {
        GameManager.gameM?.BotonPresionadoSFX();
        gameObject.SetActive(false);
    }
}