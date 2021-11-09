using Assets.Scripts.Ui.HealthPoints;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerHealthPoints : MonoBehaviour, IDamageble
    {
        [SerializeField] float maxHealth;
        [SerializeField] PlayerHealthPointsUi ui;
        Rigidbody rb;

        public void TakeDamage(float amount)
        {
            ui.RemoveHealth(amount);
        }

        public void TakeHit(float amount, Vector3 from, float force)
        {
            rb.velocity = Vector3.zero;
            var direction = (transform.position - from).normalized;
            rb.AddForce(direction * force * rb.mass);
            ui.RemoveHealth(amount);
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        void Start()
        {
            ui.SetMaxHealth(maxHealth);
        }
    }
}
