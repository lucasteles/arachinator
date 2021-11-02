using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtMouse : MonoBehaviour
{
    Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
       var mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
       if (Physics.Raycast(mouseRay, out var hit))
           transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));
    }
}
