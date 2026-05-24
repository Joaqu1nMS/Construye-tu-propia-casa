using UnityEngine;
using UnityEditor;

# if UNITY_EDITOR
[CustomEditor(typeof(VentanaAnimada))]
public class VentanaAnimadaMenu : Editor
{
    public override void OnInspectorGUI()
    {
        // Dibujar inspector normal
        DrawDefaultInspector();

        GUILayout.Space(10);

        VentanaAnimada controller = (VentanaAnimada)target;

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Encender"))
        {
            controller.TurnOn();

            // Para que se vea en editor inmediatamente
            EditorUtility.SetDirty(controller);
        }

        if (GUILayout.Button("Apagar"))
        {
            controller.TurnOff();

            EditorUtility.SetDirty(controller);
        }

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Toggle"))
        {
            controller.Toggle();

            EditorUtility.SetDirty(controller);
        }
    }
}
# endif