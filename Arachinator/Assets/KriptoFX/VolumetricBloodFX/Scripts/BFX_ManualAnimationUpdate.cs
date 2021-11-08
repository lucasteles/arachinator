using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class BFX_ManualAnimationUpdate : MonoBehaviour
{
    public BFX_BloodSettings BloodSettings;
    public AnimationCurve AnimationSpeed = AnimationCurve.Linear(0, 0, 1, 1);
    public float FramesCount = 99;
    public float TimeLimit = 3;
    public float OffsetFrames = 0;

    private float currentTime;

    Renderer rend;
    private MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        rend = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        rend.enabled = true;

        rend.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat("_UseCustomTime", 1.0f);
        propertyBlock.SetFloat("_TimeInFrames", 0.0f);
        rend.SetPropertyBlock(propertyBlock);

        currentTime = 0;
    }

    void Update()
    {
        currentTime += Time.deltaTime * BloodSettings.AnimationSpeed;
        if (currentTime / TimeLimit > 1.0)
        {
            if (rend.enabled) rend.enabled = false;
            return;
        }

        var currentFrameTime = AnimationSpeed.Evaluate(currentTime / TimeLimit);
        currentFrameTime = currentFrameTime * FramesCount + OffsetFrames + 1.1f;
        float timeInFrames = (Mathf.Ceil(-currentFrameTime) / (FramesCount + 1)) + (1.0f / (FramesCount + 1));

        rend.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat("_LightIntencity", Mathf.Clamp(BloodSettings.LightIntensityMultiplier, 0.01f, 1f));
        propertyBlock.SetFloat("_TimeInFrames", timeInFrames);
        rend.SetPropertyBlock(propertyBlock);
    }
}
