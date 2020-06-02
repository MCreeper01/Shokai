using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GraphMaker))]
[CanEditMultipleObjects]
public class GraphEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GraphMaker g = (GraphMaker)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Bake"))
        {
            g.CrearNodes();
            g.CrearConnexions();
            AssetDatabase.SaveAssets();
            Debug.Log(AssetDatabase.GetAssetPath(g.go));
        }
    }
}
