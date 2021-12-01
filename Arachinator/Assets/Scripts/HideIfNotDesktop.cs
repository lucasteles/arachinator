using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideIfNotDesktop : MonoBehaviour
{
    void Awake()
    {
        if (Enviroment.IsMobile)
            gameObject.SetActive(false);
    }
}
