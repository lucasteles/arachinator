using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    Rigidbody rb;
    Vector3 velocity = Vector3.zero;

    HashSet<object> locks = new HashSet<object>();
	void Start () => rb = GetComponent<Rigidbody>();

    public Vector3 Direction => velocity.normalized;
    public bool IsLocked()
    {
        if (locks.Count > 0)
            return true;

        return false;
    }

	void FixedUpdate ()
    {
        if (IsLocked()) return;

        rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
    }

	public void Move(Vector3 velocity) => this.velocity = velocity;

    public void LookAt(Vector3 point)
    {
        var target = new Vector3(point.x, transform.position.y, point.z);
        transform.LookAt(target);
    }

    public void LockWith(object owner)
    {
        if (!locks.Contains(owner))
            locks.Add(owner);
    }

    public object Lock()
    {
        var key = Guid.NewGuid();
        LockWith(key);
        return key;
    }

    public void Unlock(object owner)
    {
        if (locks.Contains(owner))
            locks.Remove(owner);
    }

    public void Lock(float seconds)
    {
        var key = Lock();
        StartCoroutine(LockForSeconds(key, seconds));
    }

    IEnumerator LockForSeconds(object key, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Unlock(key);
    }

}
