using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Movement : MonoBehaviour
{
    Rigidbody rb;
    Vector3 velocity = Vector3.zero;
    bool isLocked;

	void Start () => rb = GetComponent<Rigidbody>();

	void FixedUpdate ()
    {
        if (isLocked) return;
        // rb.AddForce(velocity, ForceMode.VelocityChange);
        // rb.velocity = new Vector3(
        //     Mathf.Clamp(rb.velocity.x, -maxVelocity.x, maxVelocity.x),
        //     rb.velocity.y,
        //     Mathf.Clamp(rb.velocity.z, -maxVelocity.y, maxVelocity.y)
        // );
        rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
    }

	public void Move(Vector3 velocity) => this.velocity = velocity;

    public void LookAt(Vector3 point)
    {
        var target = new Vector3(point.x, transform.position.y, point.z);
        transform.LookAt(target);
    }


    public void Lock() => isLocked = true;
    public void Unlock() => isLocked = false;

    public void Lock(float seconds)
    {
        Lock();
        Invoke(nameof(Unlock), seconds);
    }

}
