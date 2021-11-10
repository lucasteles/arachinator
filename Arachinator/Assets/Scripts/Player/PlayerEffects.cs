using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Ui.HealthPoints;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [SerializeField]Material damageMaterial;
    [SerializeField]Material dissolveMaterial;

    Renderer[] renderers;
    Dictionary<Renderer, Material> originalMaterials;
    Dictionary<Renderer, Material> damageMaterialsCache;
    Dictionary<Renderer, Material> dissolveMaterialsCache;
    static readonly int FresnelLevel = Shader.PropertyToID("_FresnelLevel");
    static readonly int DissolveLevel = Shader.PropertyToID("_Dissolve");

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>().ToArray();
    }

    void Start()
    {
        damageMaterialsCache = InstantiateMaterials(damageMaterial);
        dissolveMaterialsCache = InstantiateMaterials(dissolveMaterial);

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

    public void UseFresnelShader() => SetMaterialFromCache(damageMaterialsCache);
    public void UseDissolveShader() => SetMaterialFromCache(dissolveMaterialsCache);

    public void SetDissolveOnAllMaterials(float dissolve)
    {
        foreach (var mat in dissolveMaterialsCache.Values)
            mat.SetFloat(DissolveLevel, dissolve);
    }

    public void SetFresnelOnAllMaterials(float dissolve)
    {
        foreach (var mat in damageMaterialsCache.Values)
            mat.SetFloat(FresnelLevel, dissolve);
    }

    public IEnumerator DissolveEffect(float step)
    {
        UseDissolveShader();
        for (var i = 0f; i <= 1; i+=step)
        {
            SetDissolveOnAllMaterials(i);
            yield return null;
        }
    }

    public IEnumerator DissolveRestoreEffect(float step)
    {
        for (var i = 1f; i >= 0; i-=step)
        {
            SetDissolveOnAllMaterials(i);
            yield return null;
        }

        RestoreMaterials();
    }

}
