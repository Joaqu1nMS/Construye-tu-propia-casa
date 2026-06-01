using UnityEngine;

[RequireComponent(typeof(EnemySensor))]
public class FuzzyLogicController : MonoBehaviour
{
    [Header("Variables de sospecha")]
    [SerializeField, Range(0f, 100f)] private float incrementoSospechaPorVision = 35f; 
    [SerializeField, Range(0f, 100f)] private float incrementoSospechaPorSonido = 10f; 
    [SerializeField, Range(0f, 100f)] private float incrementoPorSonidoFuerte = 20f; 
    [SerializeField, Range(0f, 100f)] private float decaySospecha = 1f; 

    [Tooltip("Para manejar el decay en SEARCH")]
    [SerializeField, Range(0f, 1f)] private float multDecaySearch = 0.2f;

    public float nivelSospecha { get; private set; }

    public float valorVision { get; private set; }

    public float sonidoVision { get; private set; }

    private const float VISION_PARCIAL = 0.5f;
    private const float VISION_CLARA   = 1.0f;
    private const float SONIDO   = 0.5f;
    private const float SONIDO_FUERTE    = 1.0f;

    private EnemySensor sensor;
    private EnemyFSM fsm;       
    private bool regla3; 

    private void Awake()
    {
        sensor = GetComponent<EnemySensor>();
        fsm = GetComponent<EnemyFSM>();
    }

    private void Update()
    {
        SampleInputs();
        ApplyFuzzyRules();
        nivelSospecha = Mathf.Clamp(nivelSospecha, 0f, 100f);
    }

    private void SampleInputs()
    {
        valorVision = sensor.GetVisionValue();
        sonidoVision  = sensor.GetNoiseValue();
    }

    private void ApplyFuzzyRules()
    {
        float dt = Time.deltaTime;

        if (Mathf.Approximately(valorVision, VISION_CLARA))
        {
            //Debug.Log("TE HE VISTO");
            nivelSospecha = 100f;
            regla3   = false;
            return;
        }

        if (Mathf.Approximately(valorVision, VISION_PARCIAL))
        {
            //Debug.Log("Me parece haber visto algo");
            nivelSospecha += incrementoSospechaPorVision * dt;
            regla3    = false;
            return;
        }

        if (Mathf.Approximately(sonidoVision, SONIDO_FUERTE))
        {
            //Debug.Log("SONIDO CERCA");
            if (!regla3 || nivelSospecha < 60f)
            {
                nivelSospecha = Mathf.Max(nivelSospecha, 60f);
                regla3 = true;
            }
            nivelSospecha += incrementoPorSonidoFuerte * dt;
            return;
        }

        regla3 = false;

        if (Mathf.Approximately(sonidoVision, SONIDO))
        {
            //Debug.Log("SONIDO LEJOS");
            nivelSospecha += incrementoSospechaPorSonido * dt;
            return;
        }

        //Debug.Log("TODO EN ORDEN");
        float decayMultiplier = (fsm != null && fsm.estadoActual == EnemyFSM.EnemyState.Search)
            ? multDecaySearch
            : 1f;

        nivelSospecha -= decaySospecha * decayMultiplier * dt;
    }

    public void SetSuspicion(float value) => nivelSospecha = Mathf.Clamp(value, 0f, 100f);

    public string GetSuspicionLabel()
    {
        if (nivelSospecha < 40f) return "LOW";
        if (nivelSospecha < 75f) return "MEDIUM";
        return "HIGH";
    }
}
