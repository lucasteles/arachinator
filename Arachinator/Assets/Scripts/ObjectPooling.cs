using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum  Pools
{
    Shell,
    Bullet,
    MuzzleFlash,
    HitParticle,
    Cuspe
}

[Serializable]
public struct PoolObject
{
    public Pools name;
    public int numberOfObjects;
    public GameObject theObject;
}

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance;

    [SerializeField] PoolObject[] poolingConfig;

    Dictionary<Pools, Queue<GameObject>> pool = new Dictionary<Pools, Queue<GameObject>>();
    Dictionary<Pools, Transform> handlers = new Dictionary<Pools, Transform>();
    void Awake()
    {
        if (Instance) Destroy(Instance);
        Instance = this;
    }

    void Start()
    {
        foreach (var config in poolingConfig)
        {
            var handler = new GameObject();
            handler.transform.SetParent(transform);

            handler.name = config.name.ToString();
            handlers.Add(config.name, handler.transform);
            var queue = new Queue<GameObject>();
            pool.Add(config.name, queue);
            for (var i = 0; i < config.numberOfObjects; i++)
            {
                var obj = Instantiate(config.theObject, Vector3.zero, Quaternion.identity);
                obj.SetActive(false);
                obj.transform.SetParent(handler.transform);
                queue.Enqueue(obj);
            }
        }

    }

    public GameObject GetObject(Pools name,Vector3 position, Quaternion rotation)
    {
        var queue = pool[name];
        var obj = queue.Dequeue();
        obj.transform.SetParent(null);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void GiveItBack(Pools name, GameObject obj)
    {
        var queue = pool[name];
        if (queue.Contains(obj)) return;

        obj.gameObject.SetActive(false);
        var handler = handlers[name];
        var transform = obj.transform;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        obj.transform.SetParent(handler);
        queue.Enqueue(obj);
    }
    public void GiveItBack(Pools name, GameObject obj, float seconds)
    {
        IEnumerator timer()
        {
            yield return new WaitForSeconds(seconds);
            GiveItBack(name, obj);
        }
        StartCoroutine(timer());
    }

    public static GameObject Get(Pools name, Vector3 position, Quaternion rotation)
        => Instance.GetObject(name, position, rotation);

    public static void GiveBack(Pools name, GameObject obj) => Instance.GiveItBack(name, obj);
    public static void GiveBack(Pools name, GameObject obj, float seconds) => Instance.GiveItBack(name, obj, seconds);
}

