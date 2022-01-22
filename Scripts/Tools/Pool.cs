
using System.Collections.Generic;
using UnityEngine;

public class Pool
{
    public Stack<GameObject> inactive = new Stack<GameObject>();
    public GameObject parent;

    public Pool(GameObject parentIn)
    {
        parent = parentIn;
    }
}
