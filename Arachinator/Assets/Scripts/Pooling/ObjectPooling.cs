using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class ObjectPooling<T> : MonoBehaviour where T : MonoBehaviour
{
    public static ObjectPooling<T> Instance;

    [SerializeField] int numberOfObjects;
    [SerializeField] T theObject;


    Queue<T> queue = new Queue<T>();

    void Awake() => Instance ??= this;

    void Start()
    {
        for (var i = 0; i < numberOfObjects; i++)
        {
            var obj = Instantiate(theObject, Vector3.zero, Quaternion.identity);
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(transform);
            queue.Enqueue(obj);
        }
    }

    public T Get()
    {
        var obj = queue.Dequeue();
        obj.gameObject.SetActive(true);
        obj.transform.SetParent(null);
        return obj;
    }

    public T Get(Vector3 position, Quaternion rotation)
    {
        var obj = Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        return obj;
    }

    public void GiveBack(T obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        queue.Enqueue(obj);
    }

}

