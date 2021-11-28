using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaspEffects : MonoBehaviour
{
    WaspEye[] eyes;
    [SerializeField] Material dissolveMaterial;
    [SerializeField] float dissolveSpeed = .01f;
    [SerializeField] GameObject explosionPivot;

    void Awake()
    {
        eyes = GetComponentsInChildren<WaspEye>();
    }

    public IEnumerator OpenEyes()
    {
        var routiness = eyes
            .Select(eye => StartCoroutine(eye.OpenEyes()))
            .ToArray();

        foreach (var r in routiness)
            yield return r;
    }

    public IEnumerator CloseEyes()
    {
        var routiness = eyes
            .Select(eye => StartCoroutine(eye.CloseEyes()))
            .ToArray();

        foreach (var r in routiness)
            yield return r;
    }

    public IEnumerator Dissolve()
    {
        var dissolveLevel = Shader.PropertyToID("_Dissolve");
        var renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var renderer in renderers)
            for ( var i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                renderer.sharedMaterials[i] = dissolveMaterial;
                renderer.sharedMaterial = dissolveMaterial;
            }

        dissolveMaterial.SetFloat(dissolveLevel, 0);
        for (var i = 0f; i <= 1; i+=dissolveSpeed)
        {
            dissolveMaterial.SetFloat(dissolveLevel, i);
            yield return null;
        }

    }

    public IEnumerator BloodExplosions(GameObject hitEffect, float timeBetweenDeathExplosions)
    {
        while (true)
        {
            var offset = Utils.RandomVector3(-2f, 2f);
            var blood = Instantiate(hitEffect, explosionPivot.transform.position+offset, transform.rotation);
            blood.transform.Rotate(Vector3.up,Random.rotation.eulerAngles.y);
            blood.transform.localScale *= 10f;
            Destroy(blood, 2);
            yield return new WaitForSeconds(timeBetweenDeathExplosions);
        }
    }
}
