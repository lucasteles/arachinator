using UnityEngine;
using System.Collections;
using System;

public class BFX_ShaderProperies : MonoBehaviour {

    public BFX_BloodSettings BloodSettings;

    public AnimationCurve FloatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float GraphTimeMultiplier = 1, GraphIntensityMultiplier = 1;
    public float TimeDelay = 0;

    private bool canUpdate;
    bool isFrized;
    private float startTime;

    private int cutoutPropertyID;
    int forwardDirPropertyID;
    float timeLapsed;

    private MaterialPropertyBlock props;
    private Renderer rend;

    public event Action OnAnimationFinished;

    private void Awake()
    {
        props = new MaterialPropertyBlock();
        rend = GetComponent<Renderer>();

        cutoutPropertyID = Shader.PropertyToID("_Cutout");
        forwardDirPropertyID = Shader.PropertyToID("_DecalForwardDir");

        OnEnable();
    }

    private void OnEnable()
    {
        startTime = Time.time + TimeDelay;
        canUpdate = true;

        GetComponent<Renderer>().enabled = true;

        rend.GetPropertyBlock(props);

        var eval = FloatCurve.Evaluate(0) * GraphIntensityMultiplier;
        props.SetFloat(cutoutPropertyID, eval);
        props.SetVector(forwardDirPropertyID, transform.up);
        rend.SetPropertyBlock(props);
    }

    private void OnDisable()
    {
        rend.GetPropertyBlock(props);

        var eval = FloatCurve.Evaluate(0) * GraphIntensityMultiplier;
        props.SetFloat(cutoutPropertyID, eval);

        rend.SetPropertyBlock(props);
        timeLapsed = 0;
    }



    private void Update()
    {
        if (!canUpdate) return;

        rend.GetPropertyBlock(props);

        var deltaTime = BloodSettings == null ? Time.deltaTime : Time.deltaTime * BloodSettings.AnimationSpeed;
        if (BloodSettings != null && BloodSettings.FreezeDecalDisappearance && (timeLapsed / GraphTimeMultiplier) > 0.3f) { }
        else timeLapsed += deltaTime;

        var eval = FloatCurve.Evaluate(timeLapsed / GraphTimeMultiplier) * GraphIntensityMultiplier;
        props.SetFloat(cutoutPropertyID, eval);

        if (BloodSettings != null) props.SetFloat("_LightIntencity", Mathf.Clamp(BloodSettings.LightIntensityMultiplier, 0.01f, 1f));

        if (timeLapsed >= GraphTimeMultiplier)
        {
            canUpdate = false;
            OnAnimationFinished?.Invoke();

        }

        var mat = rend.sharedMaterial;
        props.SetVector(forwardDirPropertyID, transform.up);
        rend.SetPropertyBlock(props);
    }

}
