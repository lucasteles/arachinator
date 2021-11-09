using System;
using Assets.Scripts.Ui.HealthPoints;
using UnityEngine;

public class Player : MonoBehaviour, IDamageble
{
    [SerializeField]float speed = 5;
    [SerializeField] PlayerHealthPointsUi ui;

    Movement movement;
    Rigidbody rb;
    Life life;
    void Awake()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody>();
        life = GetComponent<Life>();
    }

    void Start()
    {
        ui.SetMaxHealth(life.MaxLife);
    }

    void Update ()
    {
        var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        movement.Move(input.normalized * speed);
    }
    public void TakeDamage(float amount) => ui.RemoveHealth(amount);

    public void TakeHit(float amount, Vector3 from, float force)
    {
        rb.velocity = Vector3.zero;
        var direction = (transform.position - from).normalized;
        rb.AddForce(direction * force, ForceMode.Acceleration);
        TakeDamage(amount);
    }
}
