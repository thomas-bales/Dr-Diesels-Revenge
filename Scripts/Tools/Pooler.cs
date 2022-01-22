using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Pooler
{
    public static Dictionary<string, Pool> pools = new Dictionary<string, Pool>();

    public static void Spawn(GameObject prefab, Vector3 pos, Quaternion rot, int limit = 0)
    {
        string key = prefab.name.Replace("Clone", "");
        GameObject go;

        //checks if the pool exists, otherwise creates a new one and adds it to the dictionary
        if (pools.ContainsKey(key))
        {
            //check if the pool is empty, if not, use object from pool
            if (pools[key].inactive.Count == 0 && (limit == 0 || pools[key].parent.transform.childCount < limit))
            {
                GameObject newObject = Object.Instantiate(prefab, pos, rot, pools[key].parent.transform);
                newObject.name = prefab.name.Replace("Clone", "");
            }
            else if (pools[key].parent.transform.childCount >= limit && pools[key].inactive.Count == 0)
            {
                Debug.Log("Out of bullets!");
            }
            else
            {
                go = pools[key].inactive.Pop();
                go.transform.position = pos;
                go.transform.rotation = rot;
                go.SetActive(true);
            }
        }
        else
        {
            GameObject newParent = new GameObject($"{key}_POOL");
            Object.DontDestroyOnLoad(newParent);
            Pool newPool = new Pool(newParent);
            pools.Add(key, newPool);

            GameObject newObject = Object.Instantiate(prefab, pos, rot, pools[key].parent.transform);
            newObject.name = prefab.name.Replace("Clone", "");
        }
    }

    public static void Despawn(GameObject gameObject)
    {
        string key = gameObject.name.Replace("Clone", "");

        pools[key].inactive.Push(gameObject);
        gameObject.transform.position = pools[key].parent.transform.position;
        gameObject.SetActive(false);
    }
}
