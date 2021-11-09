using UnityEngine;

namespace Assets.Scripts.Enemies
{
    public class EnemyDamageDealer : MonoBehaviour
    {
        [SerializeField] float damage;
        [SerializeField] float force = 10f;

        void OnCollisionEnter(Collision collision)
        {
            var other = collision.gameObject;
             if (other.CompareTag("Player") && other.GetComponent<IDamageble>() is { } damageble)
                 damageble.TakeHit(
                     damage,
                     collision.collider.ClosestPoint(transform.position),
                     force);
        }
    }
}
