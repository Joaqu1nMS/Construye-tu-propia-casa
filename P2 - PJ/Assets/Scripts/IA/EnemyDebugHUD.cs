using UnityEngine;

[RequireComponent(typeof(FuzzyLogicController))]
[RequireComponent(typeof(EnemyFSM))]
public class EnemyDebugHUD : MonoBehaviour
{
    [SerializeField] private bool showHUD = true;
    [SerializeField] private Vector2 hudPosition = new Vector2(10f, 10f);

    private FuzzyLogicController fuzzy;
    private EnemyFSM             fsm;

    private readonly Color colorLow    = new Color(0.2f, 0.8f, 0.2f);
    private readonly Color colorMedium = new Color(1.0f, 0.7f, 0.0f);
    private readonly Color colorHigh   = new Color(0.9f, 0.1f, 0.1f);

    private void Awake()
    {
        fuzzy = GetComponent<FuzzyLogicController>();
        fsm   = GetComponent<EnemyFSM>();
    }

    private void OnGUI()
    {
        if (!showHUD || fuzzy == null) return;

        float s     = fuzzy.SuspicionLevel;
        float v     = fuzzy.VisionValue;
        float r     = fuzzy.NoiseValue;
        string label = fuzzy.GetSuspicionLabel();
        string state = fsm.CurrentState.ToString();

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box)   { fontSize = 13 };
        GUIStyle lblStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };

        float x = hudPosition.x;
        float y = hudPosition.y;
        float w = 260f;

        // Background box
        GUI.Box(new Rect(x, y, w, 160f), $"Enemy AI – {gameObject.name}", boxStyle);

        y += 24f;

        // State
        GUI.Label(new Rect(x + 8f, y, w - 16f, 22f), $"FSM State: <b>{state}</b>", lblStyle);
        y += 22f;

        // Vision
        GUI.Label(new Rect(x + 8f, y, w - 16f, 22f),
            $"Vision  (V): {VisionLabel(v)}  [{v:F2}]", lblStyle);
        y += 22f;

        // Noise
        GUI.Label(new Rect(x + 8f, y, w - 16f, 22f),
            $"Noise   (R): {NoiseLabel(r)}   [{r:F2}]", lblStyle);
        y += 22f;

        // Suspicion label
        Color prev = GUI.color;
        GUI.color = SuspicionColor(s);
        GUI.Label(new Rect(x + 8f, y, w - 16f, 22f),
            $"Suspicion:  {label}  ({s:F1})", lblStyle);
        GUI.color = prev;
        y += 22f;

        // Suspicion bar
        Rect barBg  = new Rect(x + 8f, y, w - 16f, 14f);
        Rect barFill = new Rect(x + 8f, y, (w - 16f) * (s / 100f), 14f);
        GUI.Box(barBg, GUIContent.none);
        Color barColor = SuspicionColor(s);
        barColor.a = 0.85f;
        GUI.color  = barColor;
        GUI.DrawTexture(barFill, Texture2D.whiteTexture);
        GUI.color  = prev;
    }

    private Color SuspicionColor(float s)
    {
        if (s < 40f) return colorLow;
        if (s < 75f) return colorMedium;
        return colorHigh;
    }

    private static string VisionLabel(float v)
    {
        if (v >= 1f)   return "CLEAR  ";
        if (v >= 0.5f) return "PARTIAL";
        return "NULL   ";
    }

    private static string NoiseLabel(float r)
    {
        if (r >= 1f)   return "LOUD  ";
        if (r >= 0.5f) return "LIGHT ";
        return "SILENT";
    }
}
