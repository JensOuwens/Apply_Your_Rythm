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
    public override void OnInspectorGUI()
    {
        // Draw all normal serialized fields first
        DrawDefaultInspector();

        GUILayout.Space(10);

        var composition = (CompositionObject)target;

        // Sort button
        if (!GUILayout.Button("Sort")) return;
        var temp = new List<List<CompositionObject.BeatData>>(composition.data.Count);
        for (var i = 0; i < composition.data.Count; i++)
        {
            List<CompositionObject.BeatData> row = new(composition.data[i]);
            row.Sort();
            temp[i] = row;
        }
        composition.data = temp;

        EditorUtility.SetDirty(composition);
        AssetDatabase.SaveAssets();
    }
}
#endif