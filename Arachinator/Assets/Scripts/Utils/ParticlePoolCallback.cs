using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePoolCallback : MonoBehaviour
{
    [SerializeField] Pools poolName;

    public void OnParticleSystemStopped()
    {
        ObjectPooling.GiveBack(poolName, gameObject);
    }

}
