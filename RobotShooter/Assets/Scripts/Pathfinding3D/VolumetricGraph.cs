using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumetricGraph : MonoBehaviour
{
    [HideInInspector]
    public List<Node> Graph = new List<Node>();
    public LayerMask wallMask;
    public float distanceBtwNodes = 1;
    public int geometryLayer;
    public float erosionOffset;

    // Start is called before the first frame update
    void Start()
    {
        CrearNodes();
        CrearConnexions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    GameObject[] FindActiveGameObjectsWithLayer(int layer)
    {
        GameObject[] goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        List<GameObject> goList = new List<GameObject>();
        for (int i = 0; i < goArray.Length; i++)
        {
            if (goArray[i].layer == layer && goArray[i].activeInHierarchy)
            {
                goList.Add(goArray[i]);
            }
        }

        if (goList.Count == 0)
        {
            return null;
        }

        return goList.ToArray();
    }

    public void CrearNodes()
    {
        Graph.Clear();
        GameObject[] objectsWithLayer = FindActiveGameObjectsWithLayer(geometryLayer);
        Collider c;
        for (int i = 0; i < objectsWithLayer.Length; i++)
        {
            c = objectsWithLayer[i].GetComponent<Collider>();
            Vector3 dist = new Vector3(c.bounds.max.x - c.bounds.min.x + (erosionOffset * 2), c.bounds.max.y - c.bounds.min.y + (erosionOffset * 2), c.bounds.max.z - c.bounds.min.z + (erosionOffset * 2));
            int boxX = Mathf.RoundToInt(dist.x / distanceBtwNodes);
            int boxY = Mathf.RoundToInt(dist.y / distanceBtwNodes);
            int boxZ = Mathf.RoundToInt(dist.z / distanceBtwNodes);
            for (int x = 0; x <= boxX; x++)
            {
                for (int y = 0; y <= boxY; y++)
                {
                    for (int z = 0; z <= boxZ; z++)
                    {
                        if ((x == 0 || x == boxX || y == 0 || y == boxY || z == 0 || z == boxZ) && (y * distanceBtwNodes - dist.y / 2 + objectsWithLayer[i].transform.position.y) > 0)
                        {
                            Node pn = new Node();
                            pn.position = new Vector3(x * distanceBtwNodes - dist.x / 2 + objectsWithLayer[i].transform.position.x, y * distanceBtwNodes - dist.y / 2 + objectsWithLayer[i].transform.position.y, z * distanceBtwNodes - dist.z / 2 + objectsWithLayer[i].transform.position.z);
                            Graph.Add(pn);
                        }
                    }
                }
            }
        }
    }

    public void CrearConnexions()
    {
        foreach (Node pn in Graph)
        {
            foreach (Node successor in Graph)
            {
                if (pn.Connections.Count >= 25)
                {
                    break;
                }
                Ray r = new Ray
                {
                    origin = pn.position,
                    direction = successor.position - pn.position
                };

                RaycastHit hit;
                if (!Physics.Raycast(r.origin, r.direction, out hit, 500, wallMask.value))
                {
                    Connection c = new Connection();
                    c.cost = Mathf.RoundToInt(r.direction.magnitude);
                    c.successor = successor;
                    pn.Connections.Add(c);
                }
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        if (Graph.Count > 0)
        {
            foreach (Node n in Graph)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(n.position, 0.2f);
            }
        }
    }
}
