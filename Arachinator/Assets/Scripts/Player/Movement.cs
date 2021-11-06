using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Movement : MonoBehaviour
{
    Rigidbody rb;

    Vector3 velocity = Vector3.zero;

	void Start () => rb = GetComponent<Rigidbody>();

	void FixedUpdate () =>
        rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime);

	public void Move(Vector3 velocity) => this.velocity = velocity;

    public void LookAt(Vector3 point)
    {
        var target = new Vector3(point.x, transform.position.y, point.z);
        transform.LookAt(target);
    }
}
