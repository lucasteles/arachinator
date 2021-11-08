using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BFX_RenderDepth : MonoBehaviour
{
    DepthTextureMode defaultMode;
    void OnEnable()
    {
        var cam = GetComponent<Camera>();
        defaultMode = cam.depthTextureMode;
        if (cam.renderingPath == RenderingPath.Forward)
        {
            cam.depthTextureMode |= DepthTextureMode.Depth;
        }

    }

    // Update is called once per frame
    void OnDisable()
    {
        GetComponent<Camera>().depthTextureMode = defaultMode;
    }
}
