using UnityEngine;

public class DieOnFall : MonoBehaviour
{

    void Update()
    {
        if (transform.position.y < -10)
            GetComponent<Life>().Die();

    }
}
