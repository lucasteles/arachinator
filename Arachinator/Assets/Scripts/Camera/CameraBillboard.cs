using UnityEngine;

public class CameraBillboard : MonoBehaviour
{
    public bool BillboardX = true;
    public bool BillboardY = true;
    public bool BillboardZ = true;
    public float OffsetToCamera;
    Vector3 localStartPosition;
    Camera camera;

    void Start()
    {
        localStartPosition = transform.localPosition;
        camera = Camera.main;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward,
                                                               camera.transform.rotation * Vector3.up);
        if(!BillboardX || !BillboardY || !BillboardZ)
            transform.rotation = Quaternion.Euler(BillboardX ? transform.rotation.eulerAngles.x : 0f, BillboardY ? transform.rotation.eulerAngles.y : 0f, BillboardZ ? transform.rotation.eulerAngles.z : 0f);
        transform.localPosition = localStartPosition;
        transform.position += transform.rotation * Vector3.forward * OffsetToCamera;
        transform.Rotate(0,180,0);
    }
}
