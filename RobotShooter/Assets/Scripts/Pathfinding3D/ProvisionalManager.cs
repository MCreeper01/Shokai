using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class ProvisionalManager : MonoBehaviour
{
    public static ProvisionalManager _instance;

    GraphObject[] Graphs = new GraphObject[4];

    public GraphObject currentGraph
    {
        get
        {
            for (int i = 0; i < Graphs.Length; i++)
            {
                if (Graphs[i].ID == GameManager.instance.roundController.currentMap)
                {
                    return Graphs[i];
                }
            }
            return null;
        }
    }

    public static ProvisionalManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ProvisionalManager>();
            }

            return _instance;
        }
    }

    void Awake()
    {
        Graphs[0] = (GraphObject)AssetDatabase.LoadAssetAtPath("Assets/GraphAssets/Graph1.asset", typeof(GraphObject));
        Graphs[1] = (GraphObject)AssetDatabase.LoadAssetAtPath("Assets/GraphAssets/Graph2.asset", typeof(GraphObject));
        Graphs[2] = (GraphObject)AssetDatabase.LoadAssetAtPath("Assets/GraphAssets/Graph3.asset", typeof(GraphObject));
        Graphs[3] = (GraphObject)AssetDatabase.LoadAssetAtPath("Assets/GraphAssets/Graph4.asset", typeof(GraphObject));
        //Graphs = Resources.LoadAll<GraphObject>("Assets/GraphAssets");
        //Graphs = (GraphObject)AssetDatabase.LoadAllAssetsAtPath("Assets/GraphAssets/Graph" + i + 1 + );
        /*for (int i = 0; i < 4; i++)
        {
            Graphs[i] = (GraphObject)AssetDatabase.LoadAssetAtPath("Assets/GraphAssets/Graph" + i + 1 + ".asset", typeof(GraphObject));
        }*/
    }

    void Start()
    {
        //Debug.Log(Graphs[0].Graph.Count);
        /*
        Debug.Log(Graphs.Length);
        for (int i = 0; i < Graphs.Length; i++)
        {
            foreach (Node n in Graphs[i].Graph)
            {
                Debug.Log(n.position);
            }            
        }*/
    }
}
