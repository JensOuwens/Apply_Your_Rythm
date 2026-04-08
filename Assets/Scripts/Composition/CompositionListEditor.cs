using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
/// <summary>
/// An editor class that only exists inside Unity's editor, used for visualizing the compositionList
/// </summary>
[CustomEditor(typeof(CompositionList))]
public class CompositionListEditor : Editor
{
    const float CellSize = 24f;
    const float RowHeight = 24f;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "showVisual");
        
        GUILayout.Space(10);
        
        var compositionList = (CompositionList)target;
        
        if (GUILayout.Button("Show timeline", GUILayout.Width(CellSize * 4)))
        {
            compositionList.showVisual = !compositionList.showVisual;
            EditorUtility.SetDirty(compositionList);
        }
        if (compositionList.showVisual)
            DrawTimeline(compositionList, compositionList);
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTimeline(CompositionList compositionList, CompositionList compositionList1)
    {
        var beats = new List<List<BeatData>>();
        float biggestCount = 0;
        foreach (var composition in compositionList1.compositions)
        {
            var buildVisualList = composition.BuildVisualList();
            beats.Add(buildVisualList);
            if (biggestCount < buildVisualList.Count)
                biggestCount = buildVisualList.Count;
        }

        var rowRect = GUILayoutUtility.GetRect(CellSize * beats.Count, biggestCount * RowHeight);

        for (var i = 0; i < biggestCount; i++)
        {
            for (var j = 0; j < beats.Count; j++)
            {
                var cell = new Rect(
                    rowRect.x + j * CellSize,
                    rowRect.y + i * CellSize,
                    CellSize - 2,
                    RowHeight - 2);
                
                var beat = beats[j][i];
                
                if (beat == null)
                    EditorGUI.DrawRect(cell, new Color(0.15f, 0.15f, 0.15f));
                else
                {
                    Color color;
                    if (beat.type == BeatType.Tap)
                    {
                        if (beat.attribute == BeatAttribute.Water)
                            color = new Color(0.3f, 0.5f, 1f);
                        else
                            color = new Color(0.5f, 0.3f, 0.5f);
                    }
                    else
                    {
                        if (beat.attribute == BeatAttribute.Water)
                            color = new Color(0.5f, 0.7f, 1f);
                        else
                            color = new Color(0.5f, 0.3f, 0.3f);
                        cell.height += 2;
                    }

                    EditorGUI.DrawRect(cell, color);
                }

                GUI.Box(cell, GUIContent.none);
            }

        }
    }
}
#endif