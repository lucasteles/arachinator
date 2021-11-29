using UnityEngine;

public class EnemyDamageDealer : MonoBehaviour
{
    public float damage;
    public float force = 10f;
    public bool active = true;

    void OnCollisionEnter(Collision collision)
    {
        var other = collision.gameObject;
         if (active && other.CompareTag("Player") && other.GetComponent<IDamageble>() is { } damageble)
             damageble.TakeHit(
                 damage,
                 collision.contacts[0].point,
                 force);
    }
}
