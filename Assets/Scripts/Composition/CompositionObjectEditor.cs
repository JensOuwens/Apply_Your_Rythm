#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// An editor class that only exists inside Unity's editor, adds a button to CompositionObject for sorting its data
/// </summary>
[CustomEditor(typeof(CompositionObject))]
public class CompositionObjectEditor : Editor
{
    const float CellSize = 24f;
    const float RowHeight = 24f;
    
    public override void OnInspectorGUI()
    {
        // Draw everything except dataVisual
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "dataVisual", "showVisual");

        GUILayout.Space(10);

        var composition = (CompositionObject)target;
        var dataVisual = composition.BuildVisualList();
        
        if (GUILayout.Button("Sort"))
        {
            composition.data.Sort();
            EditorUtility.SetDirty(composition);
        }
        GUILayout.Space(10);
        
        if (GUILayout.Button("Show timeline", GUILayout.Width(CellSize * 4)))
        {
            composition.showVisual = !composition.showVisual;
            EditorUtility.SetDirty(composition);
        }
        if (composition.showVisual)
            DrawTimeline(composition, dataVisual);
        
        serializedObject.ApplyModifiedProperties();
    }
    
    void DrawTimeline(CompositionObject comp, List<BeatData> dataVisual)
    {
        if (dataVisual == null) return;

        // GUILayout.Space(5);
        var rowRect = GUILayoutUtility.GetRect(CellSize, dataVisual.Count *RowHeight);

        for (var i = 0; i < dataVisual.Count; i++)
        {
            var cell = new Rect(
                rowRect.x,
                rowRect.y + i * CellSize,
                CellSize - 2,
                RowHeight - 2);

            var beat = dataVisual[i];

            if (beat == null)
                EditorGUI.DrawRect(cell, new Color(0.15f, 0.15f, 0.15f));
            else
            {
                var c = beat.type == BeatType.Tap
                    ? new Color(0.3f, 0.8f, 0.3f)
                    : new Color(0.3f, 0.5f, 1f);

                EditorGUI.DrawRect(cell, c);
            }

            GUI.Box(cell, GUIContent.none);
        }
    }
}
#endif