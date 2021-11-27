using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Ui.HealthPoints;
using UnityEngine;

public class EnemyEffects : MonoBehaviour
{
    [SerializeField]Material material;

    Renderer[] renderers;
    Dictionary<Renderer, Material> originalMaterials;
    Dictionary<Renderer, Material> deflectMaterialsCache;
    static readonly int FresnelLevel = Shader.PropertyToID("_FresnelLevel");

    void Awake() => renderers = GetComponentsInChildren<Renderer>().ToArray();

    void Start()
    {
        deflectMaterialsCache = InstantiateMaterials(material);
        originalMaterials =
            renderers.Select(r => (r, r.sharedMaterial))
                .ToDictionary(x => x.r, x => x.sharedMaterial);
    }

    Dictionary<Renderer, Material> InstantiateMaterials(Material material)
    {
        var cache = new Dictionary<Renderer, Material>();
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            var color = renderer.sharedMaterial.color;
            var tex = renderer.sharedMaterial.mainTexture;
            var newMaterial = new Material(material);
            newMaterial.color = color;
            newMaterial.mainTexture = tex;
            cache.Add(renderer, newMaterial);
        }
        return cache;
    }

    public void RestoreMaterials()
    {
        for (var i = 0; i < renderers.Length; i++)
            renderers[i].sharedMaterial = originalMaterials[renderers[i]];
    }

    public void SetMaterialFromCache(Dictionary<Renderer, Material> cache)
    {
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            var newMaterial = cache[renderer];
            renderer.sharedMaterial = newMaterial;
        }
    }

    public void UseDeflectShader() => SetMaterialFromCache(deflectMaterialsCache);
    public void SetFresnelOnAllMaterials(float level)
    {
        foreach (var mat in deflectMaterialsCache.Values)
            mat.SetFloat(FresnelLevel, level);
    }

}
