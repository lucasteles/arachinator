using UnityEngine;

namespace Assets.Scripts.Enemies
{
    public class EnemyDamageDealer : MonoBehaviour
    {
        [SerializeField] float damage;
        [SerializeField] float force = 10f;

        private void OnCollisionEnter(Collision collision)
         {
             if (collision.gameObject.GetComponent<IDamageble>() is { } damageble)
             {
                 damageble.TakeHit(damage, collision.collider.ClosestPoint(transform.position), force);
             }
         }
    }
}
