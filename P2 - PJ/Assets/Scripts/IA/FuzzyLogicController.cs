using UnityEngine;

[RequireComponent(typeof(EnemySensor))]
public class FuzzyLogicController : MonoBehaviour
{
    [Header("Suspicion Rates (per second)")]
    [SerializeField, Range(0f, 100f)] private float suspicionIncreaseRateVisually = 35f; 
    [SerializeField, Range(0f, 100f)] private float suspicionIncreaseRateHearing  = 10f; 
    [SerializeField, Range(0f, 100f)] private float suspicionIncreaseRateLoud     = 20f; 
    [SerializeField, Range(0f, 100f)] private float suspicionDecayRate            = 1f; 

    [Header("Suspicion Freeze")]
    [Tooltip("In SEARCH state the decay is slowed to this fraction of the normal decay rate.")]
    [SerializeField, Range(0f, 1f)] private float searchDecayMultiplier = 0.2f;

    public float SuspicionLevel { get; private set; }

    public float VisionValue { get; private set; }

    public float NoiseValue { get; private set; }

    private const float V_PARTIAL = 0.5f;
    private const float V_CLEAR   = 1.0f;
    private const float R_LIGHT   = 0.5f;
    private const float R_LOUD    = 1.0f;
    private const float EPSILON   = 0.01f;

    private EnemySensor sensor;
    private EnemyFSM    fsm;       
    private bool        rule3Primed; 

    private void Awake()
    {
        sensor = GetComponent<EnemySensor>();
        fsm    = GetComponent<EnemyFSM>();
    }

    private void Update()
    {
        SampleInputs();
        ApplyFuzzyRules();
        SuspicionLevel = Mathf.Clamp(SuspicionLevel, 0f, 100f);
    }

    private void SampleInputs()
    {
        VisionValue = sensor.GetVisionValue();
        NoiseValue  = sensor.GetNoiseValue();
    }

    private void ApplyFuzzyRules()
    {
        float dt = Time.deltaTime;

        if (Mathf.Approximately(VisionValue, V_CLEAR))
        {
            //Debug.Log("TE HE VISTO");
            SuspicionLevel = 100f;
            rule3Primed   = false;
            return;
        }

        if (Mathf.Approximately(VisionValue, V_PARTIAL))
        {
            //Debug.Log("Me parece haber visto algo");
            SuspicionLevel += suspicionIncreaseRateVisually * dt;
            rule3Primed    = false;
            return;
        }

        if (Mathf.Approximately(NoiseValue, R_LOUD))
        {
            //Debug.Log("SONIDO CERCA");
            if (!rule3Primed || SuspicionLevel < 60f)
            {
                SuspicionLevel = Mathf.Max(SuspicionLevel, 60f);
                rule3Primed   = true;
            }
            SuspicionLevel += suspicionIncreaseRateLoud * dt;
            return;
        }

        rule3Primed = false;

        if (Mathf.Approximately(NoiseValue, R_LIGHT))
        {
            //Debug.Log("SONIDO LEJOS");
            SuspicionLevel += suspicionIncreaseRateHearing * dt;
            return;
        }

        //Debug.Log("TODO EN ORDEN");
        float decayMultiplier = (fsm != null && fsm.CurrentState == EnemyFSM.EnemyState.Search)
            ? searchDecayMultiplier
            : 1f;

        SuspicionLevel -= suspicionDecayRate * decayMultiplier * dt;
    }

    public void SetSuspicion(float value) => SuspicionLevel = Mathf.Clamp(value, 0f, 100f);

    public string GetSuspicionLabel()
    {
        if (SuspicionLevel < 40f) return "LOW";
        if (SuspicionLevel < 75f) return "MEDIUM";
        return "HIGH";
    }
}
