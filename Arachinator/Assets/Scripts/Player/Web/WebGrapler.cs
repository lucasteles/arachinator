using UnityEngine;

public class WebGrapler : MonoBehaviour {
    private Spring spring;
    private LineRenderer lineRenderer;
    private Vector3 currentGrapplePosition;
    WebPistol grapplingGun;
    [SerializeField] int quality;
    [SerializeField] float damper;
    [SerializeField] float strength;
    [SerializeField] float velocity;
    [SerializeField] float waveCount;
    [SerializeField] float waveHeight;
    [SerializeField] AnimationCurve affectCurve;

    void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        grapplingGun = GetComponent<WebPistol>();
        spring = new Spring();
        spring.Target = 0;
    }

    void LateUpdate() => DrawRope();

    void DrawRope() {
        if (!grapplingGun.TargetDefined()) {
            currentGrapplePosition = grapplingGun.ShotPoint;
            spring.Reset();
            if (lineRenderer.positionCount > 0)
                lineRenderer.positionCount = 0;
            return;
        }

        if (lineRenderer.positionCount == 0) {
            spring.Velocity = velocity;
            lineRenderer.positionCount = quality + 1;
        }

        spring.Damper =damper;
        spring.Strength = strength;
        spring.Update(Time.deltaTime);

        var grapplePoint = grapplingGun.Target;
        var gunTipPosition = grapplingGun.ShotPoint;
        var up = Quaternion.LookRotation((grapplePoint.Value - gunTipPosition).normalized) * Vector3.up;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint.Value, Time.deltaTime * 12f);

        for (var i = 0; i < quality + 1; i++) {
            var delta = i / (float) quality;
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
                         affectCurve.Evaluate(delta);

            lineRenderer.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
        }
    }
}
