using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]float speed = 5;

    Movement movement;

    void Awake()
    {
        movement = GetComponent<Movement>();
    }

	void Update ()
    {
        var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        movement.Move(input.normalized * speed);
    }
}
