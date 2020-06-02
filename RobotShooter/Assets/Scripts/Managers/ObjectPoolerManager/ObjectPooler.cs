using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler
{
    private readonly Transform parent;
    private readonly List<GameObject> pool;
    private readonly GameObject prefab;

    public ObjectPooler(int num, GameObject prefab, Transform parent)
    {
        var scale = prefab.transform.localScale;
        this.prefab = prefab;
        this.parent = parent;
        pool = new List<GameObject>();
        for (var o = 0; o < num; o++)
        {
            var item = Object.Instantiate(prefab, parent);
            pool.Add(item);
            //item.transform.localScale = scale;
            item.SetActive(false);
            Debug.Log("Heyy");
        }
    }

    public GameObject GetPooledObject()
    {
        for (var o = 0; o < pool.Count; o++)
            if (!pool[o].activeInHierarchy)
                return pool[o];

        var item = Object.Instantiate(prefab, parent);
        pool.Add(item);
        item.SetActive(false);

        return item;
    }
}