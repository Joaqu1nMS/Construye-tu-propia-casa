using UnityEngine;

/// <summary>
/// Reads Vision (V) and Noise (R) fuzzy inputs every frame,
/// applies the five fuzzy rules, and outputs Suspicion Level (S ∈ [0,100]).
/// </summary>
[RequireComponent(typeof(EnemySensor))]
public class FuzzyLogicController : MonoBehaviour
{
    // ── Inspector – Fuzzy Rate Tuning ──────────────────────────────────────────
    [Header("Suspicion Rates (per second)")]
    [SerializeField, Range(0f, 100f)] private float suspicionIncreaseRateVisually = 35f; // Rule 2: partial vision
    [SerializeField, Range(0f, 100f)] private float suspicionIncreaseRateHearing  = 10f; // Rule 4: light hearing
    [SerializeField, Range(0f, 100f)] private float suspicionIncreaseRateLoud     = 20f; // Rule 3: continued loud noise
    [SerializeField, Range(0f, 100f)] private float suspicionDecayRate            = 15f; // Rule 5: silent + no vision

    [Header("Suspicion Freeze")]
    [Tooltip("In SEARCH state the decay is slowed to this fraction of the normal decay rate.")]
    [SerializeField, Range(0f, 1f)] private float searchDecayMultiplier = 0.2f;

    // ── Public Outputs ─────────────────────────────────────────────────────────
    public float SuspicionLevel { get; private set; }

    /// <summary>Raw fuzzy vision value: 0=Null, 0.5=Partial, 1=Clear.</summary>
    public float VisionValue { get; private set; }

    /// <summary>Raw fuzzy noise value: 0=Silent, 0.5=Light, 1=Loud.</summary>
    public float NoiseValue { get; private set; }

    // ── Fuzzy Thresholds (constants for readability) ───────────────────────────
    private const float V_PARTIAL = 0.5f;
    private const float V_CLEAR   = 1.0f;
    private const float R_LIGHT   = 0.5f;
    private const float R_LOUD    = 1.0f;
    private const float EPSILON   = 0.01f;

    // ── Private ────────────────────────────────────────────────────────────────
    private EnemySensor _sensor;
    private EnemyFSM    _fsm;        // Needed to read current state for freeze logic
    private bool        _rule3Primed; // Tracks whether Rule 3 instant jump already fired

    // ── Unity ──────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _sensor = GetComponent<EnemySensor>();
        _fsm    = GetComponent<EnemyFSM>();
    }

    private void Update()
    {
        SampleInputs();
        ApplyFuzzyRules();
        SuspicionLevel = Mathf.Clamp(SuspicionLevel, 0f, 100f);
    }

    // ── Input Sampling ─────────────────────────────────────────────────────────
    private void SampleInputs()
    {
        VisionValue = _sensor.GetVisionValue();
        NoiseValue  = _sensor.GetNoiseValue();
    }

    // ── Fuzzy Rule Engine ──────────────────────────────────────────────────────
    private void ApplyFuzzyRules()
    {
        float dt = Time.deltaTime;

        // Rule 1 – Critical Vision: Clear sight → instant 100
        if (Mathf.Approximately(VisionValue, V_CLEAR))
        {
            Debug.Log("TE HE VISTO");
            SuspicionLevel = 100f;
            _rule3Primed   = false;
            return;
        }

        // Rule 2 – Partial Vision: accumulate suspicion
        if (Mathf.Approximately(VisionValue, V_PARTIAL))
        {
            Debug.Log("Me parece haber visto algo");
            SuspicionLevel += suspicionIncreaseRateVisually * dt;
            _rule3Primed    = false;
            return;
        }

        // V == Null from here ──────────────────────────────────────────────────

        // Rule 3 – Direct Hearing: Null vision + Loud noise → jump to 60 if below, then keep rising
        if (Mathf.Approximately(NoiseValue, R_LOUD))
        {
            Debug.Log("SONIDO CERCA");
            if (!_rule3Primed || SuspicionLevel < 60f)
            {
                SuspicionLevel = Mathf.Max(SuspicionLevel, 60f);
                _rule3Primed   = true;
            }
            SuspicionLevel += suspicionIncreaseRateLoud * dt;
            return;
        }

        // Rule 3 no longer active
        _rule3Primed = false;

        // Rule 4 – Light Hearing: Null vision + Light noise → slow accumulate
        if (Mathf.Approximately(NoiseValue, R_LIGHT))
        {
            Debug.Log("SONIDO LEJOS");
            SuspicionLevel += suspicionIncreaseRateHearing * dt;
            return;
        }

        // Rule 5 – Decay: No vision, no noise
        Debug.Log("TODO EN ORDEN");
        float decayMultiplier = (_fsm != null && _fsm.CurrentState == EnemyFSM.EnemyState.Search)
            ? searchDecayMultiplier
            : 1f;

        SuspicionLevel -= suspicionDecayRate * decayMultiplier * dt;
    }

    // ── Public API ─────────────────────────────────────────────────────────────
    /// <summary>Force-set suspicion (used by FSM for Search→Chase on regained LOS).</summary>
    public void SetSuspicion(float value) => SuspicionLevel = Mathf.Clamp(value, 0f, 100f);

    /// <summary>Returns the linguistic label of the current suspicion band.</summary>
    public string GetSuspicionLabel()
    {
        if (SuspicionLevel < 40f) return "LOW";
        if (SuspicionLevel < 75f) return "MEDIUM";
        return "HIGH";
    }
}
