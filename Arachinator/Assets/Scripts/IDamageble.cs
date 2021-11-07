
using UnityEngine;

public interface IDamageble
{
    void TakeHit(float amount, Vector3 from, float force);
    void TakeDamage(float amount);

}
