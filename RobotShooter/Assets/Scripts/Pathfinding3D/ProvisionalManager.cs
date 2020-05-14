using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvisionalManager : MonoBehaviour
{
    public static ProvisionalManager _instance;


    public VolumetricGraph currentGraph;
    
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
        currentGraph.CrearNodes();
        currentGraph.CrearConnexions();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
