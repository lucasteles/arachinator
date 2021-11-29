using UnityEngine;

namespace Assets.Scripts.Enemies
{
    public class EnemyDamageDealer : MonoBehaviour
    {
        public float damage;
        public float force = 10f;

        void OnCollisionEnter(Collision collision)
        {
            var other = collision.gameObject;
             if (other.CompareTag("Player") && other.GetComponent<IDamageble>() is { } damageble)
                 damageble.TakeHit(
                     damage,
                     collision.contacts[0].point,
                     force);
        }
    }
}
