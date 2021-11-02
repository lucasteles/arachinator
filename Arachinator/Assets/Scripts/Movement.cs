using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Movement : MonoBehaviour
{
    CharacterController controller;
    [SerializeField] float speed = 3;

    Vector3 velocity = Vector3.zero;
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        
       var velocity = Vector3.zero;
       if (Input.GetKey(KeyCode.W)) velocity += transform.forward;
       if (Input.GetKey(KeyCode.S)) velocity -= transform.forward;
       if (Input.GetKey(KeyCode.A)) velocity -= transform.right;
       if (Input.GetKey(KeyCode.D)) velocity += transform.right;
       this.velocity = velocity.normalized;
    }

    void FixedUpdate()
    {
       controller.Move(velocity * speed * Time.deltaTime);
    }
}
