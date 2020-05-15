using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvisionalManager : MonoBehaviour
{
    public static ProvisionalManager _instance;


    public VolumetricGraph currentGraph;
    public GameObject flyEnemy;
    
    public static ProvisionalManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<ProvisionalManager>();
            }

            return _instance;
        }
    }

    void Awake()
    {
        //currentGraph.CrearNodes();
        //currentGraph.CrearConnexions();
        //currentGraph.CrearGridNodes();
    }

    // Start is called before the first frame update
    void Start()
    {/*
        for (int i = 0; i < 5; i++)
        {
            GameObject g = Instantiate(flyEnemy, new Vector3(Random.Range(10, 30), Random.Range(10, 30), Random.Range(10, 30)), Quaternion.identity);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
