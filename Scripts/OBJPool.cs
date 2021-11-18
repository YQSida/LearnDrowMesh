using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBJPool : MonoBehaviour
{
    public GameObject prefab;


    public Queue<GameObject> Pool = new Queue<GameObject>();
    public uint maxCount = 10;

    //扩容
    public void CheckPool()
    {
        while (Pool.Count <= maxCount)
        {

            var obj = Instantiate(prefab, transform);
            obj.transform.rotation = prefab.transform.rotation;
            obj.SetActive(false);
            Pool.Enqueue(obj);
        }
    }

    public GameObject GetObj(Vector3 pos=default)
    {
        CheckPool();
        var obj = Pool.Dequeue();
        obj.SetActive(true);
        obj.transform.position = pos;
        return obj;
    }

    public void RecycleObj(GameObject obj)
    {
        obj.SetActive(false);
        Pool.Enqueue(obj);
    }
    public void Clear()
    {
        Pool.Clear();
    }
  

    public void OnDestroy()
    {
        Clear();
    }

}
