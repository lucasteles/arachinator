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
            queue.Enqueue(obj);
        }
    }

    public T GetObject()
    {
        var obj = queue.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public T Get(Vector3 position, Quaternion rotation)
    {
        var obj = GetObject();
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        return obj;
    }

    public void GiveBack(T obj)
    {
        obj.gameObject.SetActive(false);
        queue.Enqueue(obj);
    }

}

