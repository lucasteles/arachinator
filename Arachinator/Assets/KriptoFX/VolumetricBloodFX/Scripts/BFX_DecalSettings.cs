
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BFX_DecalSettings : MonoBehaviour
{
    public BFX_BloodSettings BloodSettings;
    public Transform parent;
    public float TimeHeightMax = 3.1f;
    public float TimeHeightMin = -0.1f;
    [Space]
    public Vector3 TimeScaleMax = Vector3.one;
    public Vector3 TimeScaleMin = Vector3.one;
    [Space]
    public Vector3 TimeOffsetMax = Vector3.zero;
    public Vector3 TimeOffsetMin = Vector3.zero;
    [Space]
    public AnimationCurve TimeByHeight = AnimationCurve.Linear(0, 0, 1, 1);

    private Vector3 startOffset;
    private Vector3 startScale;
    private float timeDelay;

    Transform t, tParent;
    BFX_ShaderProperies shaderProperies;

    Vector3 averageRay;
    bool isPositionInitialized;
    private Vector3 initializedPosition;

    private void Awake()
    {
        startOffset = transform.localPosition;
        startScale = transform.localScale;
        t = transform;
        tParent = parent.transform;
        shaderProperies = GetComponent<BFX_ShaderProperies>();
        shaderProperies.OnAnimationFinished += ShaderCurve_OnAnimationFinished;
    }

    private void ShaderCurve_OnAnimationFinished()
    {
        GetComponent<Renderer>().enabled = false;
    }

    private void Update()
    {
        if (!isPositionInitialized) InitializePosition();
        if (shaderProperies.enabled && initializedPosition.x < float.PositiveInfinity) transform.position = initializedPosition;
    }

    void InitializePosition()
    {
        GetComponent<Renderer>().enabled = false;

        var currentHeight = parent.position.y;
        var ground = BloodSettings.GroundHeight;

        var currentScale = parent.localScale.y;
        var scaledTimeHeightMax = TimeHeightMax * currentScale;
        var scaledTimeHeightMin = TimeHeightMin * currentScale;

        if (currentHeight - ground >= scaledTimeHeightMax || currentHeight - ground <= scaledTimeHeightMin)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            GetComponent<MeshRenderer>().enabled = true;
        }

        float diff = (tParent.position.y - ground) / scaledTimeHeightMax;
        diff = Mathf.Abs(diff);

        var scaleMul = Vector3.Lerp(TimeScaleMin, TimeScaleMax, diff);
        t.localScale = new Vector3(scaleMul.x * startScale.x, startScale.y, scaleMul.z * startScale.z);

        var lastOffset = Vector3.Lerp(TimeOffsetMin, TimeOffsetMax, diff);
        t.localPosition = startOffset + lastOffset;
        t.position = new Vector3(t.position.x, ground + 0.05f, t.position.z);


        timeDelay = TimeByHeight.Evaluate(diff);

        shaderProperies.enabled = false;
        Invoke("EnableDecalAnimation", Mathf.Max(0, timeDelay / BloodSettings.AnimationSpeed));

        if (BloodSettings.DecalRenderinMode == BFX_BloodSettings._DecalRenderinMode.AverageRayBetwenForwardAndFloor)
        {
            averageRay = GetAverageRay(tParent.position + tParent.right * 0.05f, tParent.right);

            float decalAngle = Vector3.Angle(Vector3.up, averageRay);
            var zRotation = Mathf.Clamp(decalAngle, -90, 90);
            var decalRotation = t.localRotation.eulerAngles;
            t.localRotation = Quaternion.Euler(decalRotation.x, decalRotation.y, -(zRotation) * 0.5f);

            var scaleRelativeToAngle = Mathf.Abs(zRotation) / 90f;
            var decalScale = t.localScale;
            decalScale.y = Mathf.Lerp(decalScale.y, decalScale.x * 1.5f, scaleRelativeToAngle);

            t.localScale = decalScale;
        }

        if (BloodSettings.ClampDecalSideSurface) Shader.EnableKeyword("CLAMP_SIDE_SURFACE");

        isPositionInitialized = true;
    }

    private void OnDisable()
    {
        if (BloodSettings.ClampDecalSideSurface) Shader.DisableKeyword("CLAMP_SIDE_SURFACE");
        isPositionInitialized = false;
        initializedPosition = Vector3.positiveInfinity;
    }

    Vector3 GetAverageRay(Vector3 start, Vector3 forward)
    {
        if (Physics.Raycast(start, -forward, out RaycastHit bulletRay))
        {
            return (bulletRay.normal + Vector3.up).normalized;
        }

        return Vector3.up;
    }

    void EnableDecalAnimation()
    {
        shaderProperies.enabled = true;
        initializedPosition = transform.position;
    }

    private void OnDrawGizmos()
    {
        if (t == null) t = transform;
        Gizmos.color = new Color(49 / 255.0f, 136 / 255.0f, 1, 0.03f);
        Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.color = new Color(49 / 255.0f, 136 / 255.0f, 1, 0.85f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);


    }
}
