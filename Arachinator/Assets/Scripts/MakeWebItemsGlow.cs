using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MakeWebItemsGlow : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;
    [SerializeField] LayerMask layerMaskIgnore;
    [SerializeField] string[] tagsToIgnore;
    [SerializeField] Material material;
    static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
    static readonly int MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");
    static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");

    void Awake()
    {
        var webableObjects =
            FindObjectsOfType<GameObject>()
                .Where(x => Utils.IsInLayerMask(x, layerMask))
                .Where(x => !Utils.IsInLayerMask(x, layerMaskIgnore))
                .Where(x => !tagsToIgnore.Any(x.CompareTag))
                .Where(x => !x.TryGetComponent<ParticleSystem>(out _));

        foreach (var o in webableObjects)
        {
            if(!o.TryGetComponent<Renderer>(out var renderer))
                continue;

            var oldMaterial = renderer.sharedMaterial;
            var newMaterial = new Material(material);
            newMaterial.color = oldMaterial.color;
            newMaterial.mainTexture = oldMaterial.mainTexture;

            CopyTexture(oldMaterial, newMaterial, BumpMap);
            CopyTexture(oldMaterial, newMaterial, MetallicGlossMap);
            renderer.sharedMaterial = newMaterial;
        }

        void CopyTexture(Material from, Material to, int propId)
        {
            try
            {
                if (from.HasProperty(propId))
                    to.SetTexture(propId, from.GetTexture(propId));
            }
            catch { }
        }


    }
}
